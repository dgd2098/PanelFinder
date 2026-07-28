using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionCube : MonoBehaviour
{
    public GameObject ifcPosition; //하이라키창내에 IFCObject아래에 IFCPosition오브젝트
    public int dir;
    public Vector3 startCameraPos;
    private Vector3 startCameraRot;

    private void Start()
    {
        startCameraPos = Camera.main.transform.position;
        startCameraRot = Camera.main.transform.localEulerAngles;
    }
    public void CunnectCubeMethod()
    {
        if(dir == 0) //정면
        {
            FrontCamera();
        }
        else if (dir == 1) //후면
        {
            BackCamera();
        }
        else if (dir == 2) //윗
        {
            TopCamera();
        }
        else if (dir == 3) //아랫
        {
            BottomCamera();
        }
        else if (dir == 4) //오른
        {
            RightCamera();
        }
        else if (dir == 5) //왼
        {
            LeftCamera();
        }
    }

    public void TopCamera()
    {
        //mainCam.transform.position = new Vector3(-0.5f, 1.67f, -15f);
        //mainCam.transform.Rotate(Vector3.zero);

        //정면 포지션, 정면 각도값
        ifcPosition.transform.localPosition = Vector3.zero;
        ifcPosition.transform.localEulerAngles = Vector3.zero;

        Camera.main.transform.localEulerAngles = startCameraRot;
        Camera.main.transform.position = startCameraPos;
        Camera.main.orthographicSize = 28;
    }

    public void BottomCamera()
    {
        //후면 포지션, 후면 각도값
        ifcPosition.transform.localPosition = Vector3.zero;
        ifcPosition.transform.localEulerAngles = new Vector3(0f, 0f, 180f);

        Camera.main.transform.localEulerAngles = startCameraRot;
        Camera.main.transform.position = startCameraPos;
        Camera.main.orthographicSize = 28;
    }

    public void FrontCamera()
    {
        //윗면 포지션, 윗면 각도값
        ifcPosition.transform.localPosition = Vector3.zero;
        ifcPosition.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);

        Camera.main.transform.localEulerAngles = startCameraRot;
        Camera.main.transform.position = startCameraPos;
        Camera.main.orthographicSize = 28;
    }

    public void BackCamera()
    {
        //아랫면 포지션, 아랫면 각도값
        ifcPosition.transform.localPosition = Vector3.zero;
        ifcPosition.transform.localEulerAngles = new Vector3(90f, 0f, 0f);

        Camera.main.transform.localEulerAngles = startCameraRot;
        Camera.main.transform.position = startCameraPos;
        Camera.main.orthographicSize = 28;
    }

    public void LeftCamera()
    {
        //오른쪽 포지션, 오른쪽 각도값
        ifcPosition.transform.localPosition = Vector3.zero;
        ifcPosition.transform.localEulerAngles = new Vector3(0f, -90f, 90f);

        Camera.main.transform.localEulerAngles = startCameraRot;
        Camera.main.transform.position = startCameraPos;
        Camera.main.orthographicSize = 28;
    }

    public void RightCamera()
    {
        //왼쪽 포지션, 왼쪽 각도값
        ifcPosition.transform.localPosition = Vector3.zero;
        ifcPosition.transform.localEulerAngles = new Vector3(0f, 90f, -90f);

        Camera.main.transform.localEulerAngles = startCameraRot;
        Camera.main.transform.position = startCameraPos;
        Camera.main.orthographicSize = 28;
    }

    public void ThreeView()
    {
        //Camera.main.orthographic = !Camera.main.orthographic;

        //if(!Camera.main.orthographic)
        //{

        //}

        ifcPosition.transform.localPosition = new Vector3(28.8f, 0f, 22.9f);
        ifcPosition.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);

        Camera.main.transform.localEulerAngles = new Vector3(16.434f, -28.926f, -1.383f);
        Camera.main.transform.position = startCameraPos;
        Camera.main.orthographicSize = 28;
    }

    float globalAlpha = 1f;

    public void TransparentObject()
    {
        if (globalAlpha == 1f) globalAlpha = 0.5f;
        else if (globalAlpha == 0.5f) globalAlpha = 0f;
        else globalAlpha = 1f;

        Shader.SetGlobalFloat("_GlobalAlpha", globalAlpha);
    }
}
