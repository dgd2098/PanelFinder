using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;

public class CreateObject : MonoBehaviour
{
    private string assetPath; //
    public string filename = "Test.ifc";
    public GameObject ifcFilePanel;

    public GameObject ifcButton;
    public Transform buttonParent;

    //FireBase
    //FirebaseStorage storage;
    //StorageReference storageReference;

    private void Start()
    {
        //assetPath = Application.dataPath;

        //storage = FirebaseStorage.DefaultInstance;
        //storageReference = storage.GetReferenceFromUrl("gs://ifc-file.appspot.com/");

        //StorageReference reference = storageReference.Child("Test.ifc");

        //reference.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        //{
        //    if (!task.IsFaulted && !task.IsCanceled)
        //    {
        //        StartCoroutine(GetFile(Convert.ToString(task.Result)));
        //    }
        //});
    }

    private IEnumerator GetFile(string url)
    {
        UnityWebRequest request;

        using (request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if(string.IsNullOrWhiteSpace(request.error))
            {
                string vp = assetPath + "/Resources/" + "IFCTest.ifc";

                if (!Directory.Exists(vp))
                {
                    FileStream fs = new FileStream(vp, FileMode.OpenOrCreate, FileAccess.Write);
                    byte[] file = request.downloadHandler.data;
                    fs.Write(file, 0, file.Length);
                    fs.Close();

                    //AssetDatabase.ImportAsset(vp, ImportAssetOptions.ForceUpdate);

                    yield break;
                }
                else
                {
                    Debug.Log(request.error);
                }
            }
        }
    }

    public void CreateAsset(string fileName)
    {
        GameObject go = Resources.Load<GameObject>(fileName);
        Instantiate(go);
    }

    public void PanelActive()
    {
        bool isActive = false;

        isActive = ifcFilePanel.activeSelf ? false : true;

        if (isActive)
        {
            GameObject go = Instantiate(ifcButton, buttonParent);
            Button btn = go.GetComponent<Button>();
            btn.onClick.AddListener(AddButtonMethod);
        }
        else
        {

        }

        ifcFilePanel.SetActive(isActive);
    }

    private void AddButtonMethod()
    {
        CreateAsset("IFCTest");
    }
}