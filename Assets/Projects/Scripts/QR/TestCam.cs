using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using ZXing;
using TMPro;

public class TestCam : MonoBehaviour
{
    public RawImage cameraViewImage;
    private BarcodeReader _barcodeReader;

    public TMP_Text testText;

    private void Awake()
    {
        _barcodeReader = new BarcodeReader();
    }

    private void Update()
    {
        ProcessQRScanning();
    }

    private void ProcessQRScanning()
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

        WebCamTexture cameraTexture = new WebCamTexture(devices[selectedCameraindex].name);
        cameraViewImage.texture = cameraTexture;
        cameraTexture.Play();

        if (cameraTexture == null)
            return;

        Color32[] pixelData = cameraTexture.GetPixels32();

        Result result =  _barcodeReader.Decode(pixelData, cameraTexture.width, cameraTexture.height);

        if(result != null)
        {
            testText.text = result.Text;
        }
    }
}
