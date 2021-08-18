#if EDITORC
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using System.Globalization;

#if UNITY_EDITOR
using UnityEditor;


[Serializable]
public class LicenseValidation
{
    public string Username;
    public string Password;
}

public class LoginExtendEditor : EditorWindow {

    private string accountPath = "Assets/Resources/StoryBoard/Account/";
    private string fileName = "Account.csv";

    private string username = "", password = "", display = "Awaiting new Account Credentials...";

    private string response = "";
    private DateTime expirationDate = DateTime.UtcNow;
    private EditorCoroutine m_loginCorouritine;

    [MenuItem("MAGES/Account Login", priority = 100)]
    public static void NewEventAction()
    {
        GetWindow(typeof(LoginExtendEditor));
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(display, GUILayout.Height(40));

        //accountPath = EditorGUILayout.TextField("Path to file:", accountPath);
        //fileName = EditorGUILayout.TextField("File:", fileName);

        username = EditorGUILayout.TextField("Username:", username);
        password = EditorGUILayout.PasswordField("Password:", password);

        if (GUILayout.Button("Save Account Credentials"))
        {
            if(string.IsNullOrEmpty(accountPath) || string.IsNullOrEmpty(fileName) ||
                string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                display = "FAILED: None of the values below must be empty!";
                return;
            }

            m_loginCorouritine = EditorCoroutineUtility.StartCoroutine(CheckDeveloperLogin(username, password, result =>
            {
                try
                {
                    expirationDate = DateTime.ParseExact(result, "MM/dd/yyyy", CultureInfo.InvariantCulture);

                    if (!Directory.Exists(accountPath))
                        Directory.CreateDirectory(accountPath);

                    if (File.Exists(accountPath + fileName))
                        File.Delete(accountPath + fileName);

                    using (FileStream fs = File.Create(accountPath + fileName))
                    {
                        byte[] info = new UTF8Encoding(true).GetBytes(username + "\n" + Encryption.StringEncryption.Encrypt(password));
                        fs.Write(info, 0, info.Length);
                    }

                    AssetDatabase.Refresh();

                    response = "SUCCESS: Login credentials are valid!\n License expiration date: " + expirationDate.Date.ToShortDateString();
                    display = "New Credentials Saved!";
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    response = "ERROR: " + result;
                    display = "Credentials not saved!";
                }
            }), this);
        }

        EditorGUILayout.LabelField(response, GUILayout.Height(60));
    }

    private IEnumerator CheckDeveloperLogin(string username, string password, Action<string> response)
    {
        LicenseValidation validationForm = new LicenseValidation
        {
            Username = username,
            Password = password
        };

        string uri = "https://login.oramavr.com/" + "api/developers/cansignin";
        string postToJson = JsonUtility.ToJson(validationForm);

        UnityWebRequest request = new UnityWebRequest(uri, "POST");
        byte[] jsonToSend = new UTF8Encoding().GetBytes(postToJson);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            response(request.error);
        }

        if (request.result == UnityWebRequest.Result.ProtocolError)
        {
            var respBody = request.downloadHandler.text;
            response(respBody + "\n" + request.error);
        }

        if (request.responseCode == 200)
        {
            var respBody = (request.downloadHandler.text);
            response(respBody);
        }
    }

    private void OnDisable()
    {
        if (m_loginCorouritine != null)
        {
            EditorCoroutineUtility.StopCoroutine(m_loginCorouritine);
        }
    }
}

#endif
#endif