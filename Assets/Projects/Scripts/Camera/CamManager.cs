using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CamManager : MonoBehaviour
{
    enum GESTURE
    {
        MOVE = 1,
        ZOOM,
    }

    [Header("CAMERA")]
    public Camera mainCam;
    public float zoomSpeed; // 줌 속도
    public float moveMultiplier; // 줌 속도
    public float rotateMultiplier;
    private float minZoom = 1f; // 최소 줌 거리
    private float maxZoom = 50f; // 최대 줌 거리

    [Header("OBJECT")]
    public GameObject ifcObj;
    public float ifcRotSpeed;
    private Vector3 lastMousePosition;

    //Move&Rotation
    public bool isMoveOrRot; //true == Rot, false == Move
    public Sprite moveImage;
    public Sprite rotImage;
    public Image moveRotImage;

    private void Awake()
    {
        mainCam.enabled = true;
        isMoveOrRot = false; //기본 무브
        GameManager.Instance.camMode = CamMode.Camera;
    }

    private void Update()
    {
        MoveRotCam(); //회전 이동
        ZoomCam(); //확대 축소

        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit(); // 앱 종료
            }
        }
    }

    private void ZoomCam()
    {
        //deltaPosition은 가장 마지막 프레임에서 발생했던 터치의 위치와 현재 프레임에서 발생한 터치 위치 차이를 반환

        if (Input.touchCount == (int)GESTURE.ZOOM)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            //각각 터치의 이전 위치를 계산한다.
            Vector2 touchZeroPrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touchOnePrevPos = touch1.position - touch1.deltaPosition;

            // 현재 및 이전 중심점
            Vector2 prevMidPoint = (touchZeroPrevPos + touchOnePrevPos) * 0.5f;
            Vector2 currentMidPoint = (touch0.position + touch1.position) * 0.5f;

            //(목적지-현재위치).magnitude : 남은 거리
            //이전 위치와 현재 위치간의 거리를 계산한다
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touch0.position - touch1.position).magnitude;

            //두 손가락 간의 거리를 계산한다
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // 확대/축소 이전의 orthographicSize 저장
            float prevOrthoSize = 0;
            float zoomFactor = 0;
            Vector3 touchWorldPos = Vector3.zero;

            if (mainCam.orthographic)
            {
                prevOrthoSize = mainCam.orthographicSize;

                //줌인, 줌아웃
                mainCam.orthographicSize += deltaMagnitudeDiff * zoomSpeed;
                mainCam.orthographicSize = Mathf.Clamp(mainCam.orthographicSize, minZoom, maxZoom);

                // 확대/축소 비율 계산
                zoomFactor = mainCam.orthographicSize / prevOrthoSize;

                // 터치 중심 위치를 월드 좌표로 변환
                touchWorldPos = mainCam.ScreenToWorldPoint(new Vector3(currentMidPoint.x, currentMidPoint.y, mainCam.nearClipPlane));
            }
            else
            {
                float minFOV = 3f;
                float maxFOV = 100f;

                prevOrthoSize = mainCam.fieldOfView;

                // FOV로 줌인/아웃 (delta가 작아질수록 줌인)
                mainCam.fieldOfView += deltaMagnitudeDiff * zoomSpeed;
                mainCam.fieldOfView = Mathf.Clamp(mainCam.fieldOfView, minFOV, maxFOV);

                // 확대/축소 비율 계산
                zoomFactor = mainCam.fieldOfView / prevOrthoSize;

                // 터치 중심 위치를 월드 좌표로 변환
                Ray ray = mainCam.ScreenPointToRay(currentMidPoint);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    touchWorldPos = hit.point;
                }
                else
                {
                    touchWorldPos = mainCam.transform.position + ray.direction * 10f; // 기본 fallback
                }
            }
            

            // 카메라 위치 조정 (줌인/아웃 중심 유지)
            Vector3 camToTouch = touchWorldPos - mainCam.transform.position;
            mainCam.transform.position += camToTouch * (1 - zoomFactor);
        }

        //PC
        float scroll = Input.GetAxis("Mouse ScrollWheel"); // 마우스 휠 입력 받기
        if (scroll != 0)
        {
            zoomSpeed = 10;
            mainCam.orthographicSize -= scroll * zoomSpeed;
            mainCam.orthographicSize = Mathf.Clamp(mainCam.orthographicSize, minZoom, maxZoom);
        }
    }
    private Vector3 lastMousePos;
    private void MoveRotCam()
    {
        if (Input.touchCount == (int)GESTURE.MOVE)
        {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Moved) //터치가 이동중
            {
                if(isMoveOrRot) //true 회전 상태일 때
                {
                    if(GameManager.Instance.camMode == CamMode.Camera)
                    {
                        Vector2 delta = touch.deltaPosition; //터치 이동량 계산
                        float camZoom = mainCam.orthographicSize;
                        float rotateSpeed = (camZoom / Screen.height) * rotateMultiplier;

                        float rotX = delta.x * rotateSpeed * Time.deltaTime; // 위/아래 이동 X축 회전
                        float rotY = delta.y * rotateSpeed * Time.deltaTime; // 좌/우 이동 Y축 회전

                        //ifcObj.transform.Rotate(rotX, rotY, 0, Space.World);

                        ifcObj.transform.Rotate(Vector3.up, -rotX, Space.World);  // 좌우 회전
                        ifcObj.transform.Rotate(Vector3.right, rotY, Space.World);  // 상하 회전
                    }
                    else if(GameManager.Instance.camMode == CamMode.Object) //GameManager.Instance.zoomCamObject
                    {
                        Vector2 delta = touch.deltaPosition; //터치 이동량 계산
                        float camZoom = mainCam.orthographicSize;
                        float rotateSpeed = (camZoom / Screen.height) * rotateMultiplier;

                        //float rotX = delta.y * rotateSpeed * Time.deltaTime; // 위/아래 이동 X축 회전
                        //float rotY = delta.x * rotateSpeed * Time.deltaTime; // 좌/우 이동 Y축 회전

                        //// 기존 로컬 회전 가져오기
                        //Vector3 current = GameManager.Instance.zoomCamObject.transform.localEulerAngles;

                        //// 드래그 방향에 맞게 회전값 추가
                        //current.x -= rotX;   // 위로 드래그하면 위로 회전
                        //current.y += rotY;   // 오른쪽 드래그하면 오른쪽으로 회전

                        //// 변경된 값을 반영
                        //GameManager.Instance.zoomCamObject.transform.localEulerAngles = current;

                        float rotX = -delta.y * rotateSpeed; // 위아래 드래그 → X축 회전
                        float rotY = -delta.x * rotateSpeed;  // 좌우 드래그 → Y축 회전

                        // localRotation 기준 회전
                        GameManager.Instance.zoomCamObject.transform.localRotation *= Quaternion.Euler(rotX, rotY, 0);

                        //float rotSpeed = ifcRotSpeed * Time.deltaTime;
                        //Vector2 delta = touch.deltaPosition; //터치 이동량 계산

                        //float rotX = delta.x * rotSpeed; // 좌우 회전 (Y축)
                        //float rotY = -delta.y * rotSpeed; // 상하 회전 (X축, 부호 주의)

                        //// 카메라 회전 (월드 기준 회전)
                        //mainCam.transform.Rotate(Vector3.up, rotX, Space.World);         // 좌우 Y축 회전
                        //mainCam.transform.Rotate(Vector3.right, rotY, Space.Self);       // 상하 X축 회전 (카메라 기준)
                    }
                }
                else //false 움직임 상태일 때 
                {
                    float camZoom = mainCam.orthographicSize; //메인캠 줌인 줌아웃 크기 0.025f 기본 움직임 값
                    float moveSpeed = (camZoom / Screen.height) * moveMultiplier;

                    Vector2 delta = touch.deltaPosition; //터치 이동량 계산
                    float deltaX = delta.x * moveSpeed; //
                    float deltaZ = delta.y * moveSpeed; //

                    ifcObj.transform.localPosition = 
                        new Vector3(ifcObj.transform.localPosition.x - deltaX, ifcObj.transform.localPosition.y, ifcObj.transform.localPosition.z - deltaZ); // - deltaZ
                }
            }
        }

        //마우스회전방법 1
        //if(Input.GetMouseButton(0))
        //{
        //    ifcObj.transform.Rotate(0f, -Input.GetAxis("Mouse X") * ifcRotSpeed, 0f, Space.World);
        //    ifcObj.transform.Rotate(Input.GetAxis("Mouse Y") * ifcRotSpeed, 0f, 0f);
        //}

        //if (Input.GetMouseButtonDown(0))
        //{
        //    lastMousePos = Input.mousePosition;
        //}

        //if (Input.GetMouseButton(0))
        //{
        //    Vector3 delta = Input.mousePosition - lastMousePos;
        //    lastMousePos = Input.mousePosition;

        //    float camZoom = mainCam.orthographicSize;
        //    float rotateSpeed = (camZoom / Screen.height) * rotateMultiplier;

        //    float rotX = delta.x * rotateSpeed * Time.deltaTime;
        //    float rotY = delta.y * rotateSpeed * Time.deltaTime;

        //    GameManager.Instance.zoomCamObject.transform.localRotation *= Quaternion.Euler(rotY, -rotX, 0);
        //}
    }

    public void ClickMoveBtn()
    {
        isMoveOrRot = !isMoveOrRot;

        if (!isMoveOrRot)
        {
            moveRotImage.sprite = moveImage;
        }
        else
        {
            moveRotImage.sprite = rotImage;
        }
    }
}
