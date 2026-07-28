using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class UserDataManager
{
    private static string filePath => Path.Combine(Application.persistentDataPath, "UserData.json");

    public static void SaveUserData(string userId, string plainPassword, bool stayLoggedIn)
    {
        UserData data = new UserData
        {
            userId = userId,
            encryptedPassword = plainPassword,//AESHelper.Encrypt(plainPassword),
            stayLoggedIn = stayLoggedIn
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
        Debug.Log("파일 경로 : " + filePath);
    }

    public static UserData LoadUserData()
    {
        if (!File.Exists(filePath)) return null;

        string json = File.ReadAllText(filePath);
        Debug.Log(filePath);
        return JsonUtility.FromJson<UserData>(json);
    }

    public static void ClearUserData()
    {
        if (File.Exists(filePath))
        { 
            File.Delete(filePath);
        }
    }
}
