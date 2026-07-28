using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QRNameTag : MonoBehaviour
{
    private Camera mainCamera;
    private Transform camPos;
    private Vector3 initialScale;
    public float scaleMultiplier = 0.35f;

    public GameObject zoomObject;

    private static float InitSize;
    private static Vector3 InitPostion;
    private static bool isZoom = false;
    private static GameObject currentZoomTarget = null; //확대시 기존 오브젝트인지 다른 오브젝트인지 확인을 위한 변수

    void Start()
    {
        mainCamera = Camera.main;
        camPos = mainCamera.transform;
        initialScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + camPos.forward);
        ScaleChange();
    }

    private void ScaleChange() //카메라 시야가 축소하면 커지고 확대하면 작아지게
    {
        float zoom = mainCamera.orthographicSize;

        //너무 작아지지 않게 줌이 1이면 스케일을 1로 고정
        transform.localScale = zoom == 1 ? Vector3.one : initialScale * zoom * scaleMultiplier;
    }

    public void ZoomObj()
    {
        //같은 오브젝트를 클릭했을 때
        if (currentZoomTarget == zoomObject && mainCamera.orthographicSize == 1)
        {
            mainCamera.transform.position = InitPostion;
            mainCamera.orthographicSize = InitSize;
            currentZoomTarget = null;

            if(GameManager.Instance.qrManager.isSingleSidedProcessing)
            {
                Debug.Log("Camera");
                GameManager.Instance.qrManager.NonSingleSidedProcessing();
                GameManager.Instance.camMode = CamMode.Camera;
            }

            //isZoom = false;
            return;
        }

        InitPostion = mainCamera.transform.position;
        InitSize = mainCamera.orthographicSize;

        Vector3 objectPos = transform.position;//meshRenderer.bounds.center;

        // 오브젝트가 화면 중심에 오도록 평면상 위치 계산
        Vector3 targetScreenPoint = mainCamera.WorldToViewportPoint(objectPos);

        Vector3 delta = new Vector3(
            targetScreenPoint.x - 0.5f,
            targetScreenPoint.y - 0.5f,
            0f
        );

        // viewport delta를 world delta로 변환
        Vector3 worldDelta = mainCamera.transform.right * delta.x * mainCamera.orthographicSize * mainCamera.aspect * 2f +
                             mainCamera.transform.up * delta.y * mainCamera.orthographicSize * 2f;

        mainCamera.transform.position += worldDelta;

        mainCamera.orthographicSize = 1f;
        currentZoomTarget = zoomObject;

        if(!GameManager.Instance.qrManager.isSingleSidedProcessing)
        {
            Debug.Log("Object");
            GameManager.Instance.qrManager.OnSingleSidedProcessing(currentZoomTarget);
            GameManager.Instance.camMode = CamMode.Object;
        }
    }
}
