using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidFileSaver : MonoBehaviour
{
    public static void SaveToDownloads(string fileName, string content)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Java Å¬·¡½º: com.tmens.downloader.DownloadSaver
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                using (AndroidJavaClass saver = new AndroidJavaClass("com.tmens.downloader.DownloadSaver"))
                {
                    saver.CallStatic("saveTextToDownload", activity, fileName, content);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveToDownloads Error: " + e.Message);
        }
#else
        Debug.Log("SaveToDownloads only works on Android device.");
#endif
    }
}
