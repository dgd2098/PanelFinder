using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using ZXing;
using UnityEngine.Android;
using UnityEngine.UI;
using TMPro;
using IfcToolkit;
using GLTFast;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using DG.Tweening;
using System.Linq;
using ZXing.Common;

public class QRManager : MonoBehaviour
{
    [Header("QR")]
    public RawImage cameraViewImage;
    public TMP_Text resultText;
    private WebCamTexture webCamTexture;
    private Coroutine qrCoroutine;

    /// <summary>
    /// Download 변수
    /// </summary>
    private bool isStatus; //true == QR, false == DOWN
    private string beforeQrText;
    private List<string> qrLogs = new List<string>();
    public ToggleGroup toggleGroup;
    public string fileName = "qrsave";
    public TMP_Text downQrText;
    public GameObject downImage;

    [Header("IFC")]
    public GameObject ifcObject; //Hierarchy창에 IFCPosition오브젝트
    public GameObject qrNameTag;
    public Material outLineMat;
    public DirectionCube directionCube;
    private bool isNameTag = false;
    public Material borderMaterial;
    public EdgeEffect edgeEffect;

    // QRManager 클래스 안, 필드 영역에 추가
    [Header("NameTag 설정")]
    public Vector3 nameTagOffset = new Vector3(0, 0.3f, 0); // 오브젝트 기준 위치 보정
    public float nameTagScale = 0.005f;                       // 라벨 전체 스케일 (절대값)
    public float nameTagFontSize = 0.005f;                    // 폰트 크기 (절대값)
    //List
    private List<GameObject> nameTagList = new List<GameObject>(); //생성한 네임태그를 저장할 리스트 변수
    private List<GameObject> outlineList = new List<GameObject>(); //아웃라인을 활성화 오브젝트를 저장할 리스트 변수
    /// <summary>
    /// Layer 변수
    /// </summary>
    private List<GameObject> wallList = new List<GameObject>(); //wall오브젝트 리스트
    private List<GameObject> floorList = new List<GameObject>(); //floor오브젝트 리스트
    private List<GameObject> panelList = new List<GameObject>(); //panel오브젝트 리스트
    private List<GameObject> openList = new List<GameObject>(); //open오브젝트 리스트
    private List<GameObject> slabList = new List<GameObject>(); //open오브젝트 리스트
    private List<GameObject> wallEdgeList = new List<GameObject>(); //wall 경계선 오브젝트 리스트
    private List<GameObject> floorEdgeList = new List<GameObject>(); //floor 경계선 오브젝트 리스트
    private List<GameObject> panelEdgeList = new List<GameObject>(); //floor 경계선 오브젝트 리스트
    private List<GameObject> openEdgeList = new List<GameObject>(); //floor 경계선 오브젝트 리스트
    private List<GameObject> slabEdgeList = new List<GameObject>(); //floor 경계선 오브젝트 리스트
    public GameObject LayerObject;
    public GameObject model;

    private void Start()
    {
        isStatus = true;
        beforeQrText = "";
    }

    public void QRButton(string status)
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

        if (WebCamTexture.devices.Length == 0)
        {
            Debug.Log("No Cam");
            return;
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        int selectedCameraindex = -1;

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == false)
            {
                selectedCameraindex = i;
                break;
            }
        }

        if (selectedCameraindex >= 0)
        {
            webCamTexture = new WebCamTexture(devices[selectedCameraindex].name, 1920, 1080, 30);
            //webCamTexture.requestedFPS = 60;
            cameraViewImage.texture = webCamTexture;
            cameraViewImage.gameObject.SetActive(true);

            if (status.Equals("DOWNSCAN"))
                downImage.SetActive(true);

            webCamTexture.Play();
            qrCoroutine = StartCoroutine(ScanQRCode(status));

            cameraViewImage.rectTransform.localEulerAngles = new Vector3(0, 0, -webCamTexture.videoRotationAngle);
            cameraViewImage.rectTransform.localScale = webCamTexture.videoVerticallyMirrored ?
                new Vector3(1, -1, 1) : new Vector3(1, 1, 1);
        }
    }

    IEnumerator ScanQRCode(string status)
    {
        IBarcodeReader barcodeReader = new BarcodeReader
        {
            Options = new DecodingOptions
            {
                TryHarder = true,
                PureBarcode = false,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
            }
        };
        bool isQr = true;
        bool isDecoding = false;

        while (isQr)
        {
            try
            {
                if (!isDecoding && webCamTexture != null && webCamTexture.width > 100 && webCamTexture.isPlaying)
                {
                    // 버퍼 복사
                    Color32[] buffer = webCamTexture.GetPixels32();
                    Color32[] frame = new Color32[buffer.Length];
                    Array.Copy(buffer, frame, buffer.Length);
                    int width = webCamTexture.width;
                    int height = webCamTexture.height;

                    isDecoding = true;

                    // 디코딩 스레드 시작
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            Result result = barcodeReader.Decode(frame, width, height);
                            if (result != null)
                            {
                                string qrData = result.Text;

                                // 결과는 메인스레드에서 처리
                                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                                {
                                    if (qrData.StartsWith("http"))
                                    {
                                        Application.OpenURL(qrData);
                                    }
                                    else
                                    {
                                        isStatus = status.Equals("QRSCAN") ? true : false;

                                        QRSeparate(qrData, status);
                                    }

                                    if(status.Equals("QRSCAN"))
                                    {
                                        webCamTexture.Stop();
                                        cameraViewImage.gameObject.SetActive(false); // QR 스캔 종료
                                        isQr = false;
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("QR Decode Thread Error: " + ex.Message);
                        }
                        finally
                        {
                            isDecoding = false;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("QR Frame Processing Error: " + ex.Message);
            }

            yield return new WaitForSeconds(0.2f); // 스캔 간격
        }
    }

    public void QRStop()
    {
        if (qrCoroutine == null)
            return;

        //다운로드 버튼을 눌렀을 시
        if (!isStatus)
        {
            SaveToFile();
        }

        webCamTexture.Stop();
        cameraViewImage.gameObject.SetActive(false);
        StopCoroutine(qrCoroutine);
    }

    private void QRSeparate(string qrtxt, string status)
    {
        //qrtxt = "K6250400000038\nAL - WALL\n(400 + 150) * 2450\n\nDSU LTD\nTUMARSANA";

        string[] lines = qrtxt.Split('\n');
        
        if(lines.Length > 2) //가장 기본적인 QR코드, 2번째 3번째 줄에 내가 원하는 값이 있다는 조건
        {
            string saveText1 = lines.Length > 1 ? lines[1] : ""; //세번째 개행문자값이 확실하다면 이 삼항연산자 사용
            string saveText2 = lines.Length > 2 ? lines[2] : "";

            string saveText = saveText1 + "|" + saveText2;

            if (status.Equals("QRSCAN"))
            {
                int panelCount = ReadQR(saveText);
                resultText.text = saveText + " (" + panelCount.ToString() + ")";
            }
            else
            {
                if (!beforeQrText.Equals(saveText))
                {
                    string fileLogs = "";
                    string toggleText = "";

                    Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault();

                    //선택한 토글의 텍스트값을 가져오기
                    if (activeToggle != null)
                    {
                        toggleText = activeToggle.GetComponentInChildren<TMP_Text>().text;
                    }
                    else
                    {
                        resultText.text = "Return";
                        return;
                    }

                    fileLogs = toggleText + ","; //"\n"
                    string restring = saveText.Replace("|", ",");

                    fileLogs = fileLogs + restring;
                    qrLogs.Add(fileLogs);
                    beforeQrText = saveText;
                    downQrText.text = saveText;
                    Vibrate(500);
                }
            }
        }
        else if(lines.Length > 1) //2번째 줄에 내가 원하는 값이 있다는 조건
        {
            string saveText1 = lines.Length > 0 ? lines[0] : ""; //세번째 개행문자값이 확실하다면 이 삼항연산자 사용
            string saveText2 = lines.Length > 1 ? lines[1] : ""; //세번째 개행문자값이 확실하다면 이 삼항연산자 사용

            string saveText = saveText1 + "|" + saveText2;

            if (status.Equals("QRSCAN"))
            {
                saveText = saveText1 + "|" + saveText2;
                int panelCount = ReadQR(saveText);
                resultText.text = saveText + " (" + panelCount.ToString() + ")";
            }
            else
            {
                if(!beforeQrText.Equals(saveText))
                {
                    string fileLogs = "";
                    string toggleText = "";

                    Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault();

                    //선택한 토글의 텍스트값을 가져오기
                    if (activeToggle != null)
                    {
                        toggleText = activeToggle.GetComponentInChildren<TMP_Text>().text;
                    }
                    else
                    {
                        resultText.text = "Return";
                        Debug.Log("Return");
                        return;
                    }

                    fileLogs = toggleText + ","; //"\n"
                    string restring = saveText.Replace("|", ",");

                    fileLogs = fileLogs + restring;
                    qrLogs.Add(fileLogs);
                    beforeQrText = saveText;
                    downQrText.text = saveText;
                    Vibrate(500);
                }
            }
        }
        else //1줄 짜리일 때 넘겨줘야할 값을 정해야함
        {
            string stack = Regex.Replace(qrtxt, @"\s+", "");

            if (status.Equals("QRSCAN"))
            {
                int panelCount = ReadQR(qrtxt);

                resultText.text = qrtxt + " (" + panelCount.ToString() + ")";
            }
            else
            {
                if (!beforeQrText.Equals(stack))
                {
                    string fileLogs = "";
                    string toggleText = "";

                    Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault();

                    //선택한 토글의 텍스트값을 가져오기
                    if (activeToggle != null)
                    {
                        toggleText = activeToggle.GetComponentInChildren<TMP_Text>().text;
                    }
                    else
                    {
                        resultText.text = "Return";
                        return;
                    }

                    fileLogs = toggleText + ","; //"\n"
                    string restring = stack.Replace("|", ",");

                    fileLogs = fileLogs + restring;
                    qrLogs.Add(fileLogs);
                    beforeQrText = stack;
                    downQrText.text = qrtxt;
                    Vibrate(500);
                }
            }
        }
    }

    public void DirButton()
    {
        string[] allowedFileTypes = new string[] { "glb", "*/*" };

        // 파일 선택기 실행
        NativeFilePicker.PickFile((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                // 파일 읽기
                if(path.EndsWith(".glb"))
                {
                    GlbImport(path);
                    DestroyNameTag();
                }
                else
                {
                    DestroyNameTag();
                }
            }
            else
            {
                Debug.Log("파일 선택이 취소되었습니다.");
            }
        }, allowedFileTypes);
    }

    public async void GlbImport(string path)
    {
        byte[] glbBytes = null;

        try
        {
            // 백그라운드에서 파일 로드
            glbBytes = await Task.Run(() => File.ReadAllBytes(path));
        }
        catch (System.Exception e)
        {
            Debug.LogError("GLB 파일 읽기 실패: " + e.Message);
            return;
        }

        var gltf = new GltfImport();

        bool success = await gltf.Load(path);

        if (!success)
        {
            ShowToast("GLB Model Loading Failed");
            return;
        }

        bool instSuccess = await gltf.InstantiateMainSceneAsync(ifcObject.transform);

        if (!instSuccess)
            return;

        if (GameManager.Instance.ifcChildObj != null)
        {
            //GameManager.Instance.ifcChild 오브젝트 교체후 삭제
            GameObject go = GameManager.Instance.ifcChildObj;
            GameManager.Instance.ifcChildObj = ifcObject.transform.GetChild(1).gameObject;
            GameManager.Instance.ifcChildObj.transform.localEulerAngles = new Vector3(0, 180, 0);
            InitObject(go);

            MeshRenderer[] renderers = GameManager.Instance.ifcChildObj.GetComponentsInChildren<MeshRenderer>();
            Bounds bounds = renderers[0].bounds; //바운드의 시작지점

            foreach (MeshRenderer render in renderers)
            {
                render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                render.receiveShadows = false;
                render.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                render.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                render.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

                // ⚠️ render.sharedMaterials → render.materials (인스턴스 머티리얼)
                foreach (var mat in render.materials)
                {
                    if (mat == null) continue;

                    mat.enableInstancing = true;

                    if (mat.shader != null && mat.shader.name.Contains("glTF/PbrMetallicRoughness"))
                    {
                        
                        if (mat.HasProperty("_Mode"))
                        {
                            mat.SetFloat("_Mode", 2); // 2 = Blend 원래 2임
                        }

                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;

                        // 투명도 조절 (alphaValue는 원하는 투명도)
                        float alphaValue = 1f; // 0.0 = 완전 투명, 1.0 = 불투명

                        if (mat.HasProperty("baseColorFactor"))
                        {
                            Color c = mat.GetColor("baseColorFactor");
                            c.a = alphaValue;
                            mat.SetColor("baseColorFactor", c);
                        }
                    }
                    else if (mat.HasProperty("_Surface"))
                    {
                        // URP Lit 대응 (혹시 다른 셰이더일 경우)
                        mat.SetFloat("_Surface", 1f);
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }
                }

                if (render.gameObject.GetComponent<CameraCulling>() == null)
                    render.gameObject.AddComponent<CameraCulling>();

                // Array로 나눈 오브젝트들
                string distinguish = DivideIFCObject(render.gameObject);

                // 경계선 오브젝트를 위한 함수
                GameObject f = edgeEffect.EdgeCreate(render.gameObject, GameManager.Instance.ifcChildObj.transform);

                if (distinguish.Equals("벽")) wallEdgeList.Add(f);
                else if (distinguish.Equals("바닥")) floorEdgeList.Add(f);
                else if (distinguish.Equals("판넬")) panelEdgeList.Add(f);
                else if (distinguish.Equals("오픈")) openEdgeList.Add(f);
                else if (distinguish.Equals("슬라브")) slabEdgeList.Add(f);

                // 중앙위치 찾기용 bounds 통합
                bounds.Encapsulate(render.bounds);
            }

            CombineMeshes(wallList, wallList, "CombineWall");
            CombineMeshes(floorList, floorList, "CombineFloor");
            CombineMeshes(openList, openList, "CombineFloor");
            //CombineMeshes(panelList, panelList, "CombineFloor");
            CombineMeshes(slabList, slabList, "CombineFloor");

            //Edge오브젝트 메쉬합치기
            CombineMeshes(wallEdgeList, wallList, "CombineEdgeWall");
            CombineMeshes(floorEdgeList, floorList, "CombineEdgeFloor");
            CombineMeshes(panelEdgeList, panelList, "CombineEdgePanel");
            CombineMeshes(openEdgeList, openList, "CombineEdgeOpen");
            CombineMeshes(slabEdgeList, slabList, "CombineEdgeOpen");

            Vector3 boundsCenter = bounds.center;
            Vector3 defaultPostion = GameManager.Instance.ifcChildObj.transform.position;

            GameManager.Instance.ifcChildObj.transform.position = new Vector3(defaultPostion.x - boundsCenter.x, defaultPostion.y, defaultPostion.z - boundsCenter.z);

            //오브젝트 생성 후 기존 카메라 위치로 변경
            directionCube.TopCamera();

            //2025-11-22추가
            CreateLabelsForAllGlbObjects();
        }
    }
    float globalAlpha = 1f;
    public void TransObejct()
    {
        if (globalAlpha == 1f) globalAlpha = 0.5f;
        else if (globalAlpha == 0.5f) globalAlpha = 0f;
        else globalAlpha = 1f;

        SetTransparencyExcluding(globalAlpha);
    }

    private void CreateLabelsForAllGlbObjects()
    {
        // 기존에 만들어진 네임태그 있으면 전부 삭제
        //DestroyNameTag();

        if (GameManager.Instance.ifcChildObj == null)
        {
            Debug.LogWarning("CreateLabelsForAllGlbObjects: ifcChildObj is null");
            return;
        }

        // GLB 안의 모든 MeshRenderer 찾아오기
        MeshRenderer[] renderers = GameManager.Instance.ifcChildObj.GetComponentsInChildren<MeshRenderer>();

        foreach (var render in renderers)
        {
            if (render == null) continue;

            Transform t = render.transform;

            string rawName = t.name;

            string[] pipeParts = rawName.Split('\n');
            if (pipeParts.Length < 2) continue;

            // 첫 번째 파트
            string firstPart = pipeParts[2];

            // 첫 번째 파트 안에서 다시 줄 나눔
            string[] lineParts = firstPart.Split('|');

            // 첫 줄만 사용
            if (lineParts.Length < 2) continue;
            string labelText = lineParts[0].Trim();

            // 네임태그 생성
            //CreateObjectSizeText(t, labelText);
            PlaceTextOnLargestFace(t.gameObject, labelText);
            //StartCoroutine(RenderText(labelText, render));
        }

        // 자동으로 생성된 상태이므로, isNameTag = true 상태가 됨 (NameTagCreateWithText 안에서 처리)
    }

    public GameObject objectSizeText;

    private void CreateObjectSizeText(Transform typ, string labelText)
    {
        MeshRenderer meshRenderer = typ.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            return;

        GameObject tag = Instantiate(objectSizeText, typ);

        //위치
        Vector3 leftBottomFront = new Vector3(
            meshRenderer.bounds.center.x,
            meshRenderer.bounds.min.y,
            meshRenderer.bounds.min.z
        );

        //tag.transform.position = leftBottomFront + nameTagOffset;
        //tag.transform.rotation = Quaternion.LookRotation(typ.forward, typ.up);

        MeshFilter meshFilter = typ.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;

        // ---- 1) 메쉬 중앙 위치(월드 공간) ----
        Vector3 meshCenter = typ.TransformPoint(meshRenderer.bounds.center);

        // ---- 2) 메쉬의 위쪽 방향(노멀) 얻기 ----
        // 일반적으로 노멀 0번만 가져와도 충분함. 
        // 필요하면 평균 노멀을 계산할 수도 있음.
        Vector3 normal = mesh.normals[0];
        Vector3 worldNormal = typ.TransformDirection(normal).normalized;

        // ---- 3) 텍스트 위치 설정 ----
        //tag.transform.position = meshCenter + worldNormal;// * distance;
        tag.transform.position = leftBottomFront;

        // ---- 4) 텍스트 회전도 노멀 방향을 바라보도록 ----
        tag.transform.rotation = Quaternion.LookRotation(worldNormal);

        // 텍스트
        TMP_Text qrText = tag.GetComponentInChildren<TMP_Text>();
        if (qrText != null)
        {
            string vertical = "";
            foreach(char c in labelText)
            {
                vertical += c + "\n";
            }

            qrText.text = vertical;
        }
    }

    public void PlaceTextOnLargestFace(GameObject obj, string labelText)
    {
        MeshFilter mf = obj.GetComponent<MeshFilter>();
        if (mf == null) return;

        // 1. 텍스트 오브젝트 생성 (또는 기존 것 사용)
        // 기존에 생성된 텍스트가 있다면 제거하고 새로 만듭니다.
        foreach (Transform child in obj.transform)
        {
            if (child.name == "SizeTag")
            {
                Destroy(child.gameObject);
            }
        }

        GameObject tag = Instantiate(objectSizeText, obj.transform);

        // TMP_Text 혹은 Text 컴포넌트 가져오기
        TMP_Text text3D = tag.GetComponentInChildren<TMP_Text>();
        if (text3D != null) text3D.text = labelText;

        Mesh mesh = mf.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // --- 1) 가장 넓은 삼각형(Main Face) 찾기 ---
        float maxArea = 0;
        Vector3 faceNormalLocal = Vector3.up;
        Vector3 bestV0 = Vector3.zero;
        Vector3 bestV1 = Vector3.zero;
        Vector3 bestV2 = Vector3.zero;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];

            Vector3 cross = Vector3.Cross(v1 - v0, v2 - v0);
            float area = cross.magnitude * 0.5f;

            if (area > maxArea)
            {
                maxArea = area;
                faceNormalLocal = cross.normalized;
                bestV0 = v0;
                bestV1 = v1;
                bestV2 = v2;
            }
        }

        // --- 2) 위치 계산 보정 (삼각형 중심 -> 메쉬 중심 투영) ---
        Vector3 boundsCenterLocal = mesh.bounds.center;

        // 평면 방정식 활용: 점 P를 평면(Normal, PointOnPlane)에 투영
        Vector3 projectedCenterLocal = boundsCenterLocal - faceNormalLocal * Vector3.Dot(faceNormalLocal, boundsCenterLocal - bestV0);

        Vector3 worldCenter = obj.transform.TransformPoint(projectedCenterLocal);
        Vector3 worldNormal = obj.transform.TransformDirection(faceNormalLocal);

        // --- 3) 회전 방향 계산 (가장 긴 변을 텍스트의 가로축으로) ---
        Vector3 d01 = bestV1 - bestV0;
        Vector3 d12 = bestV2 - bestV1;
        Vector3 d20 = bestV0 - bestV2;

        Vector3 longestDirLocal = d01;
        float maxLen = d01.sqrMagnitude;

        if (d12.sqrMagnitude > maxLen) { maxLen = d12.sqrMagnitude; longestDirLocal = d12; }
        if (d20.sqrMagnitude > maxLen) { maxLen = d20.sqrMagnitude; longestDirLocal = d20; }

        longestDirLocal.Normalize();
        Vector3 worldLongestDir = obj.transform.TransformDirection(longestDirLocal);

        // --- 4) 최종 적용 ---

        // [수정] LookRotation 설정 변경
        // 텍스트가 판넬 긴 방향으로 일자로 누우려면:
        // - 텍스트의 Y(Up)축      == 표면의 Normal (worldNormal)
        // - 텍스트의 X(Right)축   == 판넬의 긴 변 (worldLongestDir)
        // - 텍스트의 Z(Forward)축 == X축과 Y축의 외적

        Vector3 targetUp = worldNormal; // 텍스트의 위쪽 방향을 표면 법선으로 설정
        Vector3 targetRight = worldLongestDir;

        // Up 벡터와 Right 벡터를 직교화(Orthogonalize)하여 정확한 Forward 벡터 계산
        Vector3 targetForward = Vector3.Cross(targetRight, targetUp).normalized;

        // 만약 긴 변 방향 계산이 잘못되어 forward가 0이면 기본값 사용
        if (targetForward == Vector3.zero)
        {
            targetForward = Vector3.forward;
        }

        // 최종 회전값 생성: (바라보는 방향, 위쪽 방향)
        // 텍스트 오브젝트가 Y축이 위를 향하고 Z축이 앞을 향하는 기본 상태라고 가정합니다.
        tag.transform.rotation = Quaternion.LookRotation(targetForward, targetUp);

        // 위치 적용 (+Normal 방향으로 살짝 띄우기)
        float surfaceOffset = 0.03f; // 표면에서 살짝 띄우기
        tag.transform.position = worldCenter + worldNormal * surfaceOffset;
    }

    private Quaternion GetMeshRotation(MeshFilter mf)
    {
        Mesh mesh = mf.sharedMesh;
        if (mesh == null) return Quaternion.identity;

        // 첫 번째 삼각형 면을 기준으로 법선 계산
        int[] tris = mesh.triangles;
        Vector3[] verts = mesh.vertices;

        if (tris.Length < 3) return Quaternion.identity;

        Vector3 a = verts[tris[0]];
        Vector3 b = verts[tris[1]];
        Vector3 c = verts[tris[2]];

        Vector3 normal = Vector3.Cross(b - a, c - a).normalized;

        // 월드 좌표 반영
        Vector3 worldNormal = mf.transform.TransformDirection(normal);

        // Up 벡터를 임의로 Y축 사용
        Vector3 worldUp = mf.transform.up;

        return Quaternion.LookRotation(worldNormal, worldUp);
    }

    /// <summary>
    /// 선택된 오브젝트를 제외하고 나머지 MeshRenderer를 투명하게 적용
    /// alphaValue: 0~1
    /// </summary>
    public void SetTransparencyExcluding(float alphaValue)
    {
        HashSet<GameObject> excludeSet = new HashSet<GameObject>(outlineList);

        // IFC 모델 전체 렌더러 가져오기
        MeshRenderer[] renderers = GameManager.Instance.ifcChildObj.GetComponentsInChildren<MeshRenderer>();

        foreach (var render in renderers)
        {
            if (excludeSet.Contains(render.gameObject))
                continue; // 제외 대상이면 스킵

            // MaterialPropertyBlock 생성
            var block = new MaterialPropertyBlock();

            // 현재 렌더러에 적용된 PropertyBlock 불러오기
            render.GetPropertyBlock(block);

            // GLTF 기반 머티리얼은 _BaseColorFactor 또는 _Color 사용
            Color color = Color.white;
            var sharedMat = render.sharedMaterial;

            if (sharedMat.HasProperty("baseColorFactor"))
                color = sharedMat.GetColor("baseColorFactor");
            else if (sharedMat.HasProperty("_BaseColor"))
                color = sharedMat.GetColor("_BaseColor");
            else if (sharedMat.HasProperty("_Color"))
                color = sharedMat.GetColor("_Color");

            // 투명도만 조절
            color.a = alphaValue;

            // PropertyBlock에 적용
            if (sharedMat.HasProperty("baseColorFactor"))
                block.SetColor("baseColorFactor", color);
            else
                block.SetColor("_Color", color);

            render.SetPropertyBlock(block);

            // 블렌드 모드 설정 (한 번만 하면 됨)
            //sharedMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            //sharedMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            //sharedMat.SetInt("_ZWrite", 0);
            //sharedMat.DisableKeyword("_ALPHATEST_ON");
            //sharedMat.EnableKeyword("_ALPHABLEND_ON");
            //sharedMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            //sharedMat.renderQueue = 3000;
        }
    }
    public float focusRadius = 5f;
    public bool isSingleSidedProcessing = false; //현재 단면처리가 활성화되어있는지 확인하는 변수
    public void OnSingleSidedProcessing(GameObject target)
    {
        MeshRenderer[] renderers = GameManager.Instance.ifcChildObj.GetComponentsInChildren<MeshRenderer>();
        MeshRenderer targetRenderer = target.GetComponent<MeshRenderer>();
        GameObject baseObject = new GameObject();
        baseObject.transform.position = targetRenderer.bounds.center;
        baseObject.transform.parent = target.transform.parent;

        foreach (var t in renderers)
        {
            if (t.GetComponent<Renderer>() == null) continue;

            float dist = Vector3.Distance(t.bounds.center, targetRenderer.bounds.center);

            if(dist <= focusRadius)
            {
                t.enabled = true;
                t.gameObject.transform.parent = baseObject.transform;
            }
            else
            {
                t.enabled = false;
            }
        }

        GameManager.Instance.zoomCamObject = baseObject;
        isSingleSidedProcessing = true;
    }

    public void NonSingleSidedProcessing()
    {
        Transform zoom = GameManager.Instance.zoomCamObject.transform;
        zoom.transform.localRotation = Quaternion.identity;
        Transform newParent = zoom.parent;

        int count = zoom.childCount;

        for (int i = count - 1; i >= 0; i--)
        {
            zoom.GetChild(i).SetParent(newParent, true);
        }

        Destroy(GameManager.Instance.zoomCamObject);

        MeshRenderer[] renderers = GameManager.Instance.ifcChildObj.GetComponentsInChildren<MeshRenderer>(true);

        foreach (var t in renderers)
        {
            //if (t.GetComponent<Renderer>() == null) continue;

            t.enabled = true;
        }

        isSingleSidedProcessing = false;
    }

    private void InitObject(GameObject go)
    {
        Destroy(go);
        //실제 오브젝트 리스트
        wallList.Clear();
        floorList.Clear();
        panelList.Clear();
        openList.Clear();
        slabList.Clear();

        //경계선 오브젝트 리스트
        wallEdgeList.Clear();
        floorEdgeList.Clear();
        panelEdgeList.Clear();
        openEdgeList.Clear();
        slabEdgeList.Clear();

        //Layer오브젝트
        wallDiagonal.SetActive(false);
        floorDiagonal.SetActive(false);
        panelDiagonal.SetActive(false);
        openDiagonal.SetActive(false);

        isSingleSidedProcessing = false;
    }

    public int ReadQR(string saveText)
    {
        string stack = Regex.Replace(saveText, @"\s+", "");
        int qrCount = 1;
        Transform[] types = ifcObject.GetComponentsInChildren<Transform>();

        ObjectDeactive();
        DestroyNameTag();

        string nameAfterNewline = "";

        //QR스캔 후 네임태그와 아웃라인 활성화부분
        foreach (Transform typ in types)
        {
            string name = typ.name;
            string[] parts = name.Split(new[] { "\n" }, System.StringSplitOptions.None);

            // 이름의 세 번째 줄 가져오기
            if (parts.Length > 2)
            {
                nameAfterNewline = parts[2];
            }

            // 공백 제거
            string cleanName = Regex.Replace(nameAfterNewline, @"\s+", "");

            // QR 값과 판넬 이름 비교
            string[] qrParts = stack.Split('|');       // QR 값 분리: ["AL-WALL", "250*150"]
            if (qrParts.Length < 2) continue;          // 형식이 잘못되었으면 스킵
            string qrPrefix = qrParts[0];
            string qrSize = qrParts[1];

            string[] panelParts = cleanName.Split('|'); // 판넬 이름 분리

            if (panelParts.Length > 2 &&
                panelParts[1] == qrPrefix &&
                panelParts.Skip(2).Contains(qrSize)) //panelParts[0]에는 AL-WALL같은 
            {
                MeshRenderer checkMr = typ.GetComponent<MeshRenderer>();

                if(checkMr != null)
                {
                    // 조건이 맞으면 네임태그 생성 및 오브젝트 활성화
                    NameTagCreate(typ, qrCount);
                    ObjectActive(typ.gameObject);
                    qrCount++;
                }
            }
        }

        return qrCount - 1; //현재 판넬의 수량을 기입하기 위한 int형 리턴
    }

    public void TestQR() //QR코드 확인을 위한 함수, 나중에 지워야함
    {
        //string stack = "K6250400000038\nAL - WALL\n(400 + 150) * 2450\n\nDSU LTD\nTUMARSANA";
        string stack = "SLAB PANEL|600*1200";
        ReadQR(stack);
    }

    public void Test1QR() //QR코드 확인을 위한 함수, 나중에 지워야함
    {
        //string stack = "K6250400000038\nAL - WALL\n(400 + 150) * 2450\n\nDSU LTD\nTUMARSANA";
        string stack = "450*150+2400+600";
        string result = stack.Replace("AL-WALL|", "");
        string cleanStack = Regex.Replace(result, @"\s+", "");
        Debug.Log(cleanStack);
        int lineLength = cleanStack.Length;

        string[] lines = stack.Split('\n');

        int qrCount = 1;
        Transform[] types = ifcObject.GetComponentsInChildren<Transform>();

        ObjectDeactive();
        DestroyNameTag();

        //QR스캔 후 네임태그와 아웃라인 활성화부분
        foreach (Transform typ in types)
        {
            string name = typ.name;
            string cleanName = Regex.Replace(name, @"\s+", "");

            if (cleanName.Contains(cleanStack) && !string.IsNullOrWhiteSpace(name))
            {
                NameTagCreate(typ, qrCount); //NameTag생성함수
                ObjectActive(typ.gameObject); //오브젝트 아웃라인 생성함수

                qrCount++; //텍스트에 저장할 숫자값 증가
            }
        }
    }

    private string DivideIFCObject(GameObject obj)
    {
        string objName = obj.name;

        string[] parts = objName.Split(new[] { "\n" }, System.StringSplitOptions.None);
        string nameAfterNewline = "";
        string distinguish = "";

        if (parts.Length > 1)
        {
            nameAfterNewline = parts[1];
        }
        else
        {
            distinguish = "없음";
            return distinguish;
        }

        if (nameAfterNewline.Equals("벽"))
        {
            distinguish = "벽";
            wallList.Add(obj);
        }
        else if(nameAfterNewline.Contains("바닥"))
        {
            distinguish = "바닥";
            floorList.Add(obj);
        }
        else if (nameAfterNewline.Contains("판넬"))
        {
            distinguish = "판넬";
            panelList.Add(obj);
        }
        else if (nameAfterNewline.Contains("SLAB PANEL") || nameAfterNewline.Contains("END BEAM") || nameAfterNewline.Contains("MIDDLE BEAM") || nameAfterNewline.Contains("PROP HEAD"))
        {
            distinguish = "슬라브";
            slabList.Add(obj);
        }
        else
        {
            distinguish = "오픈";
            openList.Add(obj);
        }

        return distinguish;
    }

    private void CombineMeshesByMaterial(List<GameObject> objectList, List<GameObject> edgeList, string baseName)
    {
        if (objectList.Count == 0) return;

        // Material별로 나눌 Dictionary
        Dictionary<Material, List<GameObject>> materialDict = new Dictionary<Material, List<GameObject>>();

        foreach (GameObject obj in objectList)
        {
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (mr == null || mr.sharedMaterial == null) continue;

            Material mat = mr.sharedMaterial;

            if (!materialDict.ContainsKey(mat))
            {
                materialDict[mat] = new List<GameObject>();
            }
            materialDict[mat].Add(obj);
        }

        // 각 재질별로 CombineMeshes 호출
        int matIndex = 0;

        foreach (var kvp in materialDict)
        {
            string combinedName = baseName + "_Mat" + matIndex;
            CombineMeshes(kvp.Value, edgeList, combinedName);
            matIndex++;
        }
    }

    private void CombineMeshes(List<GameObject> objectList, List<GameObject> addList, string combinedName)
    {
        if (objectList.Count == 0) return;

        GameObject combinedObj = new GameObject(combinedName);
        combinedObj.transform.position = Vector3.zero;
        combinedObj.transform.rotation = Quaternion.identity;

        MeshFilter combinedFilter = combinedObj.AddComponent<MeshFilter>();
        MeshRenderer combinedRenderer = combinedObj.AddComponent<MeshRenderer>();

        List<CombineInstance> combineInstances = new List<CombineInstance>();
        Material sharedMaterial = null;

        foreach (GameObject obj in objectList)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();

            if (mf == null || mf.sharedMesh == null) continue;

            CombineInstance ci = new CombineInstance();
            ci.mesh = mf.sharedMesh;
            ci.transform = obj.transform.localToWorldMatrix;
            combineInstances.Add(ci);

            if (sharedMaterial == null) sharedMaterial = mr.sharedMaterial;

            //obj.SetActive(false);
            Destroy(obj);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.name = combinedName + "Mesh";
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
        combinedFilter.mesh = combinedMesh;
        combinedRenderer.material = sharedMaterial;

        combinedObj.transform.parent = GameManager.Instance.ifcChildObj.transform;

        objectList.Clear();
        addList.Add(combinedObj);
    }

    public List<GameObject> CombineMeshesSafe(List<GameObject> objectList, string baseName)
    {
        List<GameObject> combinedObjects = new List<GameObject>();

        if (objectList == null || objectList.Count == 0) return combinedObjects;

        // 1. Material 별로 분류
        Dictionary<Material, List<MeshFilter>> materialGroups = new Dictionary<Material, List<MeshFilter>>();

        foreach (GameObject obj in objectList)
        {
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            MeshFilter mf = obj.GetComponent<MeshFilter>();

            if (mr == null || mf == null || mf.sharedMesh == null) continue;

            Material mat = mr.sharedMaterial;
            if (mat == null) continue;

            if (!materialGroups.ContainsKey(mat))
            {
                materialGroups[mat] = new List<MeshFilter>();
            }

            materialGroups[mat].Add(mf);

            // 원본 오브젝트는 합친 후 끄기
            obj.SetActive(false);
        }

        // 2. 각 Material 그룹별로 Combine 실행
        int matIndex = 0;
        foreach (var kvp in materialGroups)
        {
            Material material = kvp.Key;
            List<MeshFilter> meshFilters = kvp.Value;

            List<CombineInstance> combineInstances = new List<CombineInstance>();

            foreach (MeshFilter mf in meshFilters)
            {
                Mesh mesh = mf.sharedMesh;

                // SubMesh 단위로 처리 (Topology가 Triangles인 경우만)
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    if (mesh.GetTopology(sub) != MeshTopology.Triangles) continue;

                    CombineInstance ci = new CombineInstance();
                    ci.mesh = mesh;
                    ci.subMeshIndex = sub;
                    ci.transform = mf.transform.localToWorldMatrix;
                    combineInstances.Add(ci);
                }
            }

            if (combineInstances.Count == 0) continue;

            // 합친 메쉬 생성
            Mesh combinedMesh = new Mesh();
            combinedMesh.name = $"{baseName}_Mat{matIndex}_Mesh";
            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

            // 새 오브젝트 생성
            GameObject combinedObj = new GameObject($"{baseName}_Mat{matIndex}");
            MeshFilter combinedFilter = combinedObj.AddComponent<MeshFilter>();
            MeshRenderer combinedRenderer = combinedObj.AddComponent<MeshRenderer>();

            combinedFilter.sharedMesh = combinedMesh;
            combinedRenderer.sharedMaterial = material;

            combinedObjects.Add(combinedObj);

            matIndex++;
        }

        return combinedObjects;
    }

    private void DestroyNameTag()
    {
        //NameTag오브젝트가 남아있으면 삭제하고 List정리하는 부분
        foreach (var obj in nameTagList)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        nameTagList.Clear();
    }

    private void NameTagCreate(Transform typ, int count) //네임태그 생성함수
    {
        //현재 오브젝트의 렌더러접근(네임태그 오브젝트의 위치를 찾기 위해)
        MeshRenderer meshRenderer = typ.transform.GetComponent<MeshRenderer>();

        //숫자 이미지 생성
        GameObject tag = Instantiate(qrNameTag, typ); //네임태그 오브젝트 생성
        tag.transform.position = meshRenderer.bounds.center; //x - 0.4f, y  + 0.3f 위치
        TMP_Text qrText = tag.GetComponentInChildren<TMP_Text>(); //텍스트 참조
        qrText.text = count.ToString(); //텍스트를 현재 카운트로 변경
        nameTagList.Add(tag);
        isNameTag = true; //네임태그 생성시 활성화가 되있는 상태이기 때문

        QRNameTag qrName = tag.GetComponentInChildren<QRNameTag>(); //생성한 네임태그 오브젝트의 QRNameTag스크립트를 참조
        qrName.zoomObject = typ.gameObject; //QRNameTag에 버튼을 클릭했을 때 카메라위치를 찾기위해 오브젝트 저장
    }

    public void NameTagActive() //네임태그 활성화 비활성화 함수
    {
        if(nameTagList.Count > 1) //현재 리스트에 값이 들어와있다면
        {
            if(isNameTag) //활성상태
            {
                for (int i = 0; i < nameTagList.Count; i++)
                {
                    nameTagList[i].gameObject.SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < nameTagList.Count; i++)
                {
                    nameTagList[i].gameObject.SetActive(true);
                }
            }

            isNameTag = !isNameTag;
        }
    }

    public void ObjectActive(GameObject go) //아웃라인 활성화 함수
    {
        MeshRenderer mr = go.GetComponent<MeshRenderer>();

        //if (mr != null)
        //{
        //    Material[] oldMat = mr.materials;
        //    Material[] newMat = new Material[oldMat.Length + 1];

        //    for (int i = 0; i < oldMat.Length; i++)
        //    {
        //        newMat[i] = oldMat[i];
        //    }

        //    newMat[newMat.Length - 1] = outLineMat;

        //    mr.materials = newMat;
        //}

        go.AddComponent<MaterialSaver>();
        MaterialSaver ms = go.GetComponent<MaterialSaver>();
        ms.previousMat = mr.material;
        mr.material = outLineMat;

        outlineList.Add(go);
    }

    public void ObjectDeactive() //활성화한 오브젝트의 메테리얼을 원상태로 돌리는 함수
    {
        //NameTag오브젝트가 남아있으면 삭제하고 List정리하는 부분
        foreach (var obj in outlineList)
        {
            if (obj != null)
            {
                MeshRenderer mr = obj.GetComponent<MeshRenderer>();
                MaterialSaver ms = obj.GetComponent<MaterialSaver>();

                mr.material = ms.previousMat;
                Destroy(ms);

                //Material[] oldMat = mr.materials;
                //Material[] newMat = new Material[oldMat.Length - 1];

                //for (int i = 0; i < newMat.Length; i++)
                //{
                //    newMat[i] = oldMat[i];
                //}

                //mr.materials = newMat;
            }
        }

        outlineList.Clear();
    }

    private void SaveToFile()
    {
        if (qrLogs.Count == 0)
            return;

        string timestamp = System.DateTime.Now.ToString("yyMMddHHmmss");
        string fullFileName = $"{fileName}({timestamp})";
        string fileLogs = "";

        //foreach (string st in qrLogs)
        //{
        //    fileLogs += st + "\n";
        //}

        Dictionary<string, int> counts = new Dictionary<string, int>();

        for (int i = 0; i < qrLogs.Count; i++)
        {
            string key = qrLogs[i];

            if (counts.ContainsKey(key))
                counts[key]++;
            else
                counts[key] = 1;
        }

        foreach (var kv in counts)
        {
            fileLogs += $"{kv.Key},{kv.Value}\n";
        }

        downImage.SetActive(false);
        AndroidFileSaver.SaveToDownloads(fullFileName, fileLogs);
        qrLogs.Clear();
        ShowToast("Saved successfully!");
    }

    public void ShowToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (activity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>(
                        "makeText", activity, message, toastClass.GetStatic<int>("LENGTH_SHORT"));
                    toast.Call("show");
                }));
            }
        }
#endif
    }
    public void Vibrate(long milliseconds)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaObject vibrator = context.Call<AndroidJavaObject>("getSystemService", "vibrator");

        if (vibrator != null)
        {
            vibrator.Call("vibrate", milliseconds);
        }
#endif
    }

    private bool isWall = false;
    private bool isFloor = false;
    private bool isPanel = false;
    private bool isOpen = false;
    private bool isSlab = false;

    [Header("LayerObject")]
    public GameObject wallDiagonal;
    public GameObject floorDiagonal;
    public GameObject panelDiagonal;
    public GameObject openDiagonal;
    public GameObject slabDiagonal;

    public void LayerActiveInactive(string option)
    {
        if(option.Equals("WALL"))
        {
            foreach(GameObject go in wallList)
            {
                go.SetActive(isWall);
            }

            wallDiagonal.SetActive(!isWall);
            isWall = !isWall;
        }
        else if (option.Equals("FLOOR"))
        {
            foreach (GameObject go in floorList)
            {
                go.SetActive(isFloor);
            }

            floorDiagonal.SetActive(!isFloor);
            isFloor = !isFloor;
        }
        else if (option.Equals("PANEL"))
        {
            foreach (GameObject go in panelList)
            {
                go.SetActive(isPanel);
            }

            panelDiagonal.SetActive(!isPanel);
            isPanel = !isPanel;
        }
        else if (option.Equals("OPEN"))
        {
            foreach (GameObject go in openList)
            {
                go.SetActive(isOpen);
            }

            openDiagonal.SetActive(!isOpen);
            isOpen = !isOpen;
        }
        else if (option.Equals("SLAB"))
        {
            foreach (GameObject go in slabList)
            {
                go.SetActive(isSlab);
            }

            slabDiagonal.SetActive(!isSlab);
            isSlab = !isSlab;
        }
    }

    public void SetActiveLayer()
    {
        RectTransform panel = LayerObject.GetComponent<RectTransform>();
        Vector3 startPos = Vector3.zero;
        Vector3 endPos = new Vector3(0, -150f, 0);
        float fadeDuration = 0.3f;
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>(); // 페이드 대상

        if (!LayerObject.activeSelf)
        {
            panel.anchoredPosition = endPos;
            LayerObject.SetActive(true);
            canvasGroup.DOFade(1f, 0.3f); // 다시 보여주기
            panel.DOAnchorPos(startPos, fadeDuration).SetEase(Ease.OutCubic);
        }
        else
        {
            panel.DOAnchorPos(endPos, fadeDuration).SetEase(Ease.InCubic);
            canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InCubic).OnComplete(() =>
            {
                LayerObject.SetActive(false); // 모두 끝난 후 비활성화
            });
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}