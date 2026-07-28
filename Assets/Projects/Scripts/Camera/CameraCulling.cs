using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCulling : MonoBehaviour
{
    private Renderer rend;
    private Camera cam;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        cam = Camera.main;
    }

    private void Update()
    {
        if (rend == null || cam == null) return;

        // 카메라의 뷰 Frustum 계산
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

        //오브젝트의 바운드가 카메라 뷰 안에 있는지 확인
        bool isVisible = GeometryUtility.TestPlanesAABB(planes, rend.bounds);

        rend.forceRenderingOff = !isVisible;
    }
}
