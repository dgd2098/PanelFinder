using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchObject : MonoBehaviour
{
    private Camera mainCam;

    public Material clickMat;
    private Material originMat;
    private GameObject previousObject;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = transform.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // 마우스 왼쪽 버튼 클릭 감지, 화면터치
        //if (Input.GetMouseButtonDown(0) || Input.touchCount == 1)
        //{
        //    Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit;

        //    if (Physics.Raycast(ray, out hit))
        //    {
        //        GameObject selectedObject = hit.collider.gameObject;
        //        MeshRenderer objMesh = selectedObject.GetComponent<MeshRenderer>();

        //        if(objMesh.materials.Length == 2) //QR로 찾은 오브젝트
        //        {
        //            if(selectedObject != previousObject)
        //            {
        //                //기존 오브젝트 원상복구
        //                if(previousObject != null) //기존 오브젝트가 null이 아닐 때, 나중에 QR 초기화 했을 때 확인해야함 기존 오브젝트에서 메테리얼 갯수가 달라지기 때문
        //                {
        //                    Material[] previousMat = new Material[objMesh.materials.Length];

        //                    previousMat[0] = objMesh.materials[0];
        //                    previousMat[1] = originMat;

        //                    MeshRenderer previousMesh = previousObject.GetComponent<MeshRenderer>();
        //                    previousMesh.materials = previousMat;
        //                }

        //                //현재 선택한 오브젝트 메테리얼 변경
        //                originMat = objMesh.materials[1];

        //                Material[] newMat = new Material[objMesh.materials.Length];

        //                newMat[0] = objMesh.materials[0];
        //                newMat[1] = clickMat;

        //                objMesh.materials = newMat;
        //            }

        //            previousObject = selectedObject.gameObject;
        //        }
        //    }
        //}
    }
}
