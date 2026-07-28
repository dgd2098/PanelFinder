using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CamMode
{
    Object,   // 오브젝트 기준
    Camera    // 카메라 기준
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        isIfc = true;
    }

    public static GameManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }


    public GameObject ifcChildObj; //실제로 움직이는, 불러온 ifc오브젝트
    public QRManager qrManager;
    public FocusCameraSystem focusCameraSystem;
    public bool isIfc; //ifc오브젝트가 현재 있는지 확인하는 변수
    public CamMode camMode;
    public GameObject zoomCamObject;
}
