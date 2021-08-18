using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class ORamaVRApplications : EditorWindow
{
    [MenuItem("MAGES/ORamaVR Downloads/MedicalSampleApp")]
    static void DownloadMedicalSampleApp()
    {
        DownloadApplication("MedicalSampleApp", "https://www.dropbox.com/s/8kp8ir0r3fq8rva/MedicalSampleApp.unitypackage?dl=1");
    }

    [MenuItem("MAGES/ORamaVR Downloads/CVRSB")]
    static void DownloadCVRSB()
    {
        DownloadApplication("CVRSB", "https://www.dropbox.com/s/muhyfwixgoclc36/CVRSB.unitypackage?dl=1");
    }

    //-----------------------------------------------------------------
    private static void DownloadApplication(string product, string url)
    {
        float downloadDataProgress = 0.0f;
        bool canceled = false;
        
        string filename = product + ".unitypackage";
        // Get existing open window or if none, make a new one:
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            var operation = webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("There was the following error during downloading " + product + " : " + webRequest.error + ".\nPlease consider trying again.");
            }

            while (!operation.isDone)
            {

                downloadDataProgress = webRequest.downloadProgress * 100;
                if (EditorUtility.DisplayCancelableProgressBar("Downloading " + product, "Please wait, while " + product + " is being downloaded... (" + (int)downloadDataProgress + "%)", downloadDataProgress / 100.0f))
                {
                    canceled = true;
                    webRequest.Abort();
                    break;
                }
            }
            EditorUtility.ClearProgressBar();


            if (canceled)
            {
                EditorUtility.DisplayDialog("Cancelled", "The downloading process was interrupted by the user.", "OK");
                return;
            }
            else if (operation.webRequest.error != null)
            {
                EditorUtility.DisplayDialog("Error", "There was an error while downloading " + product + " . Please try again.", "OK");
                canceled = true;
            }

            if (!canceled)
            {

                File.WriteAllBytes(Application.dataPath + "\\" + filename, webRequest.downloadHandler.data);
            }

        }

        if (!canceled)
        {
            AssetDatabase.ImportPackage(Application.dataPath + "\\" + filename, true);
            FileUtil.DeleteFileOrDirectory(Application.dataPath + "\\" + filename);
        }

        AssetDatabase.Refresh();
        
    }
}
