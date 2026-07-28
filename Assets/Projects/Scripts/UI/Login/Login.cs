using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MySql.Data.MySqlClient;
using UnityEngine.Networking;

public class Login : MonoBehaviour
{
    public Sprite passActiveImage;
    public Sprite passInactiveImage;
    public Button passButton;
    public TMP_InputField IDInput;
    public TMP_InputField passwordInput;
    public Toggle stayLoggedInToggle;

    private bool isPass;

    private void Start()
    {
        UserData savedData = UserDataManager.LoadUserData();

        if (savedData != null && savedData.stayLoggedIn)
        {
            //string decryptedPw = AESHelper.Decrypt(savedData.encryptedPassword);
            //StartCoroutine(LoginRequest(savedData.userId, savedData.encryptedPassword, savedData.stayLoggedIn));
            IDInput.text = savedData.userId;
            passwordInput.text = savedData.encryptedPassword;
            stayLoggedInToggle.isOn = savedData.stayLoggedIn;
        }
    }

    public void OnLoginButton()
    {
        //ConnectSql(IDInput.text, passwordInput.text, stayLoggedInToggle.isOn);
        StartCoroutine(LoginRequest(IDInput.text, passwordInput.text, stayLoggedInToggle.isOn));
    }

    public IEnumerator LoginRequest(string id, string pw, bool stay)
    {
        string loginURL = "http://211.115.71.26:51911/login.php";

        WWWForm form = new WWWForm();

        form.AddField("username", id);
        form.AddField("password", pw);

        UnityWebRequest www = UnityWebRequest.Post(loginURL, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;

            //LoginResponse response = JsonUtility.FromJson<LoginResponse>(json);
            //resultText.text = "Connecting: " + json;

            if (json.Trim().Equals("﻿로그인 성공"))
            {
                //Json파일로 로그인정보 저장 로직
                if (stay)
                {
                    UserDataManager.SaveUserData(id, pw, stay);
                }
                else
                {
                    UserDataManager.ClearUserData();
                }

                gameObject.SetActive(false);
            }
            else
            {
                ShowToast("Invalid ID or PASSOWRD. Please try again.");
            }
        }
        else
        {
            ShowToast("Connect Error");
        }
    }

    public void ConnectSql(string id, string pw, bool stay)
    {
        //string connStr = "Server=211.115.71.26;Port=53306;Database=Thenx_User_1709;Uid=LoginUser;Pwd=Thenx0074322!#00;SslMode=None;AllowUserVariables=True;Pooling=False;";
        //  Server주소 : 211.115.71.26
        string connStr = "Server = 211.115.71.26; Port=53306;Database=Tmens_User_1709;Uid=LoginUser;Pwd=Tmens0074322!#@@;SslMode=None;AllowUserVariables=True;Pooling=False";

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            try
            {
                conn.Open();
                Debug.Log("연결 성공");

                string query = "CALL $sp_Login3(@cUserID, @PWD)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cUserID", id);
                cmd.Parameters.AddWithValue("@PWD", pw);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read(); // 첫 줄 읽기
                        string nowTime = reader["NowTime"].ToString();
                        string userID = reader["cUserID"].ToString();
                        string pwd = reader["sPWD"].ToString();

                        //Json파일로 로그인정보 저장 로직
                        if(stay)
                        {
                            UserDataManager.SaveUserData(id, pw, stay);
                        }
                        else
                        {
                            UserDataManager.ClearUserData();
                        }

                        gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.Log("실패: The ID or password does not match");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.Log("연결 실패 : " + ex.Message);
            }
        }
    }

    public void PassButton()
    {
        string currentText = "";

        if (!isPass)
        {
            passButton.image.sprite = passActiveImage;
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
            currentText = passwordInput.text; // 기존 텍스트 저장
            passwordInput.text = "";
            passwordInput.text = currentText;
        }
        else
        {
            passButton.image.sprite = passInactiveImage;
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            currentText = passwordInput.text; // 기존 텍스트 저장
            passwordInput.text = "";
            passwordInput.text = currentText;
        }

        isPass = !isPass;
    }

    public void ShowToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (activity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>(
                        "makeText", activity, message, toastClass.GetStatic<int>("LENGTH_SHORT"));
                    toast.Call("show");
                }));
            }
        }
#endif
    }
}
