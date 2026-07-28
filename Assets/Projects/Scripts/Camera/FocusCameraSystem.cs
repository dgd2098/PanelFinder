using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusCameraSystem : MonoBehaviour
{
    public Camera mainCamera;
    public Camera focusCamera;
    public string focusLayerName = "Focus";

    private GameObject currentFocusObject;
    private int focusLayer;
    private int defaultLayer;

    public GameObject testObject;

    void Start()
    {
        focusLayer = LayerMask.NameToLayer(focusLayerName);
        defaultLayer = LayerMask.NameToLayer("Default");

        if (focusCamera == null)
        {
            Debug.LogError("Focus Camera not assigned!");
            return;
        }

        // 포커스 카메라가 메인카메라의 위치/회전/필드값을 항상 따라가도록 설정
        focusCamera.clearFlags = CameraClearFlags.Depth;
        focusCamera.depth = mainCamera.depth + 1;
        focusCamera.cullingMask = 1 << focusLayer;
    }

    public void TestButtons()
    {
        Focus(testObject);
    }

    public void Focus(GameObject target)
    {
        if (target == null) 
            return;

        // 모든 기존 포커스 해제
        //foreach (GameObject go in FindObjectsOfType<GameObject>())
        //{
        //    if (go.layer == focusLayer)
        //        go.layer = defaultLayer;
        //}

        // 기존 오브젝트의 레이어를 default레이어로 변경
        if(currentFocusObject != null)
            currentFocusObject.layer = defaultLayer;

        //포커스 카메라 설정
        focusCamera.orthographicSize = mainCamera.orthographicSize;
        focusCamera.transform.position = mainCamera.transform.position;
        focusCamera.transform.rotation = mainCamera.transform.rotation;

        // 새 포커스 대상 설정
        target.layer = focusLayer;
        currentFocusObject = target;
    }

    public void ClearFocus()
    {
        foreach (GameObject go in FindObjectsOfType<GameObject>())
        {
            if (go.layer == focusLayer)
                go.layer = defaultLayer;
        }
    }
}
