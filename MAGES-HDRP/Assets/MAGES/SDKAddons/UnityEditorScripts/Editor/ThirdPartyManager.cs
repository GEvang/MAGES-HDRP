using ovidVR.GameController;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

public class ThirdPartyManager : EditorWindow
{
    private enum SDK_Types{
        OCULUS,
        STEAMVR,
        WAVESDK, 
        DISSONANCE,
        FINALIK
    }

    static AddRequest ARequest;
    static RemoveRequest RRequest;

    [MenuItem("MAGES/Third Party SDK Manager/Install/Wave SDK Support")]
    static void DownloadWaveSDK()
    {
        int windowResult = EditorUtility.DisplayDialogComplex("Warning", "Wave SDK needs the XR Plugin Management package to operate correctly. It is recommended to install it in case you do not have it.", "Download Wave SDK", "Cancel", "Install XR Plugin Management");
        if (windowResult == 0)
        {            
            ARequest = Client.Add("com.htc.upm.wave.xrsdk");
            EditorApplication.update += InstallWaveFollowUp;
        }
        else if(windowResult == 2)
        {
            Client.Add("com.unity.xr.management");
        }
    }
    static void InstallWaveFollowUp()
    {
        if (ARequest.IsCompleted)
        {
            Client.Add("com.htc.upm.wave.native");
            EditorApplication.update -= InstallWaveFollowUp;
        }
    }

    [MenuItem("MAGES/Third Party SDK Manager/Install/Install Unity XR Management")]
    static void DownloadXRManagement()
    {
        SettingsService.OpenProjectSettings("Project/XR Plug-in Management");

        bool windowResult = 
            EditorUtility.DisplayDialog("XR Devices Installation/Selection",
            "After XR installation is finished, please select the supported devices for your project",
            "Ok", "Cancel");

        if(windowResult)
        {
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
            Client.Add("com.unity.xr.management");
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
        }
        else
        {

        }      

    }

    [MenuItem("MAGES/Third Party SDK Manager/Install/Dissonance")]
    static void DownloadDissonance()
    {
        if (EditorUtility.DisplayDialog("Install Dissonance", "You will be redirected to the Unity Asset Store to download Dissonance.", "OK"))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/audio/dissonance-voice-chat-70078");
        }
    }

    [MenuItem("MAGES/Third Party SDK Manager/Install/Dissonance", true)]
    static bool allowDissonance()
    {
        return !checkDissonance();
    }

    [MenuItem("MAGES/Third Party SDK Manager/Install/Final IK")]
    static void DownloadFinalIK()
    {
        if (EditorUtility.DisplayDialog("Install Final IK", "You will be redirected to the Unity Asset Store to download Final IK.", "OK"))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/animation/final-ik-14290");
        }
    }

    [MenuItem("MAGES/Third Party SDK Manager/Install/Dissonance", true)]
    static bool allowFinalIK()
    {
        return !checkFinalIK();
    }   

    [MenuItem("MAGES/Third Party SDK Manager/Uninstall/Dissonance")]
    static void UninstallDissonance()
    {
        SDKUninstall(SDK_Types.DISSONANCE);
    }

    [MenuItem("MAGES/Third Party SDK Manager/Uninstall/Dissonance", true)]
    static bool checkDissonance()
    {
#if DISSONANCE
        return true;
#else
        return false;
#endif
    }

    [MenuItem("MAGES/Third Party SDK Manager/Uninstall/Final IK")]
    static void UninstallFinalIK()
    {
        SDKUninstall(SDK_Types.DISSONANCE);
    }

    [MenuItem("MAGES/Third Party SDK Manager/Uninstall/Final IK", true)]
    static bool checkFinalIK()
    {
#if FINAL_IK
        return true;
#else
        return false;
#endif
    }

    [MenuItem("MAGES/Cameras/Universal XR")]
    static void initXRUniversalController()
    {
       if(File.Exists(Application.dataPath + "\\Resources\\MAGESres\\Cameras\\Universal_XR_Rig.prefab"))
       {
            GameObject[] cameras = GameObject.FindGameObjectsWithTag("MainCamera");

            if (cameras.Count() > 0)
                EditorUtility.DisplayDialog("Warning", "MAGES detected another camera in the scene. Please consider removing it for best experience.", "OK");

            UnityEngine.Object XRController = AssetDatabase.LoadAssetAtPath("Assets\\Resources\\MAGESres\\Cameras\\Universal_XR_Rig.prefab", typeof(GameObject));
            if(XRController != null)
            {
                GameObject devController = GameObject.Find("OvidVRDeviceController");

                DeviceController[] deviceControllers = devController.GetComponents<DeviceController>();
                for (int i = 0; i < deviceControllers.Length; i++)
                    DestroyImmediate(deviceControllers[i]);                

                GameObject XRControllerRig = Instantiate(XRController) as GameObject;
                XRControllerRig.name = "Universal_XR_Rig";

                XR_UniversalController XRDeviceController = devController.AddComponent<XR_UniversalController>();

                devController.GetComponent<OvidVRControllerClass>().leftController = GameObject.Find("LeftController");
                devController.GetComponent<OvidVRControllerClass>().rightController = GameObject.Find("RightController");
                XRDeviceController.cameraRig  = GameObject.Find("Universal_XR_Rig");
                XRDeviceController.cameraHead = GameObject.Find("Universal_XR_Rig/head");


                EditorUtility.DisplayDialog("Camera Load Success", "The Universal_XR_Rig prefab was loaded successfully.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Camera Load Failure", "There was an error while loading the XR camera.", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Camera Load Error", "The camera prefab for Universal XR Controller was not found.", "OK");
        }
    }

    [MenuItem("MAGES/Cameras/Desktop3D")]
    static void init2DoFCamera()
    {
        GameObject[] cameras = GameObject.FindGameObjectsWithTag("MainCamera");

        if(cameras.Count() > 0)
            EditorUtility.DisplayDialog("Warning", "MAGES detected another camera in the scene. Please consider removing it for best experience.", "OK");

        OvidVRControllerClass.Get.controllerType = OvidVRControllerClass.ControllerTypes.NoController;

        GameObject Camera2DoF = Resources.Load("MAGESres/Cameras/2DoFCamera") as GameObject;

        Camera2DoF = Instantiate(Camera2DoF);
        Camera2DoF.name = "2DoFCamera";

        GameObject devController = GameObject.Find("OvidVRDeviceController");

        DeviceController[] deviceControllers = devController.GetComponents<DeviceController>();
        for (int i = 0; i < deviceControllers.Length; i++)
            DestroyImmediate(deviceControllers[i]);


        NonVRController nonVRController = devController.AddComponent<NonVRController>();
        devController.GetComponent<NonVRController>().DOF = ControllerDOF.SixDOF;
        devController.GetComponent<OvidVRControllerClass>().leftController = Camera2DoF.transform.Find("LeftController").gameObject;
        devController.GetComponent<OvidVRControllerClass>().rightController = Camera2DoF.transform.Find("RightController").gameObject;
        nonVRController.cameraRig = GameObject.Find("2DoFCamera");
        nonVRController.cameraHead = GameObject.Find("Camera (eyes)");
    }

    [MenuItem("MAGES/Cameras/PointAndClickCamera (experimental)")]
    static void initPointAndClickCamera()
    {
        GameObject[] cameras = GameObject.FindGameObjectsWithTag("MainCamera");

        if (cameras.Count() > 0)
            EditorUtility.DisplayDialog("Warning", "MAGES detected another camera in the scene. Please consider removing it for best experience.", "OK");

        OvidVRControllerClass.Get.controllerType = OvidVRControllerClass.ControllerTypes.NoController;

        GameObject CameraPaC = Resources.Load("MAGESres/Cameras/PointAndClickCamera") as GameObject;

        var cameraGO = GameObject.Find("Cameras");
        if (!cameraGO)
        {
            cameraGO = new GameObject();
            cameraGO.name = "Cameras";
        }

        CameraPaC = Instantiate(CameraPaC, cameraGO.transform);
        CameraPaC.name = "PointAndClickCamera";

        GameObject devController = GameObject.Find("OvidVRDeviceController");

        DeviceController[] deviceControllers = devController.GetComponents<DeviceController>();
        for (int i = 0; i < deviceControllers.Length; i++)
            DestroyImmediate(deviceControllers[i]);

        devController.AddComponent<PointAndClickController>();
        devController.GetComponent<PointAndClickController>().DOF = ControllerDOF.TwoDOF;
        devController.GetComponent<OvidVRControllerClass>().leftController = CameraPaC.transform.Find("Controller (left)").gameObject;
        devController.GetComponent<OvidVRControllerClass>().rightController = CameraPaC.transform.Find("Controller (right)").gameObject;
    }

    [MenuItem("MAGES/Third Party SDK Manager/Android Manifest Generator/Vive Focus Plus Build")]
    static void GenerateViveManifest()
    {
        if (File.Exists(Application.dataPath + "//Plugins//Android//AndroidManifest.xml"))
        {
            File.Delete(Application.dataPath + "//Plugins//Android//AndroidManifest.xml");
        }
        string XmlManifest = "<?xml version=\"1.0\" encoding=\"utf-8\"?><manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" package=\""+Application.identifier+"\" xmlns:tools=\"http://schemas.android.com/tools\"> <application android:icon=\"@drawable/app_icon\" android:label=\"@string/app_name\" android:theme=\"@style/Theme.WaveVR.Loading\" android:resizeableActivity=\"false\" tools:replace=\"android:theme\">  <meta-data android:name=\"com.htc.vr.content.NumDoFHmd\" android:value=\"6DoF\"/> <meta-data android:name=\"com.htc.vr.content.NumDoFController\" android:value=\"6DoF\" /> <meta-data android:name=\"com.htc.vr.content.NumController\" android:value=\"2\"/> <meta-data android:name=\"minWaveSDKVersion\" android:value=\"1\"/> <activity android:name=\"com.htc.vr.unity.WVRUnityVRActivity\" android:label=\"@string/app_name\" android:configChanges=\"density|fontScale|keyboard|keyboardHidden|layoutDirection|locale|mnc|mcc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|uiMode|touchscreen\" android:enableVrMode=\"@string/wvr_vr_mode_component\"> <intent-filter> <action android:name=\"android.intent.action.MAIN\"/> <category android:name=\"android.intent.category.LAUNCHER\"/> <category android:name=\"com.htc.intent.category.VRAPP\"/> </intent-filter> <meta-data android:name=\"unityplayer.UnityActivity\" android:value=\"true\"/> <meta-data android:name=\"unityplayer.SkipPermissionsDialog\" android:value=\"true\"/> </activity> </application>​ <uses-permission android:name=\"android.permission.RECORD_AUDIO\"/> <uses-permission android:name=\"vive.wave.vr.oem.data.OEMDataRead\"/> <uses-permission android:name=\"vive.wave.vr.oem.data.OEMDataWrite\"/> </manifest> ";
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(Application.dataPath + "//Plugins//Android//", "AndroidManifest.xml")))
        {
            outputFile.WriteLine(XmlManifest);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("MAGES/Third Party SDK Manager/Android Manifest Generator/Oculus Quest Build")]
    static void GenerateQuestManifest()
    {
        if (File.Exists(Application.dataPath + "//Plugins//Android//AndroidManifest.xml"))
        {
            File.Delete(Application.dataPath + "//Plugins//Android//AndroidManifest.xml");
        }
        string XmlManifest = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?><manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" android:installLocation=\"auto\"><uses-feature android:name=\"android.hardware.vr.headtracking\" android:version=\"1\" android:required=\"true\"/><application android:allowBackup=\"false\"><activity android:theme=\"@android:style/Theme.Black.NoTitleBar.Fullscreen\" android:configChanges=\"locale|fontScale|keyboard|keyboardHidden|mcc|mnc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|touchscreen|uiMode\" android:launchMode=\"singleTask\" android:name=\"com.unity3d.player.UnityPlayerActivity\" android:excludeFromRecents=\"true\"> <intent-filter> <action android:name=\"android.intent.action.MAIN\"/><category android:name=\"android.intent.category.INFO\"/></intent-filter></activity><meta-data android:name=\"unityplayer.SkipPermissionsDialog\" android:value=\"false\"/></application></manifest>";
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(Application.dataPath + "//Plugins//Android//", "AndroidManifest.xml")))
        {
            outputFile.WriteLine(XmlManifest);
        }
        AssetDatabase.Refresh();
    }

    static void SDKUninstall(SDK_Types Type)
    {        
        if(Type == SDK_Types.WAVESDK)
        {
            RemoveDefinition("WAVE_SDK");
            RRequest = Client.Remove("com.htc.upm.wave.native");
            EditorApplication.update += RemoveWaveFollowUp;
        }
        else if (Type == SDK_Types.DISSONANCE)
        {
            RemoveDefinition("DISSONANCE");
            FileUtil.DeleteFileOrDirectory(Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\Dissonance");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "Dissonance was uninstalled successfully.", "OK");
        }
        else if(Type == SDK_Types.FINALIK)
        {
            RemoveDefinition("FINAL_IK");
            FileUtil.DeleteFileOrDirectory(Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\InverseKinematics");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "Final IK was uninstalled successfully.", "OK");
        }
    }

    public static void RemoveDefinition(string defineSymbol)
    {
        foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
        {
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

            if (group == BuildTargetGroup.Unknown)
            {
                continue;
            }

            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();

            if (defineSymbols.Contains(defineSymbol))
            {
                defineSymbols.Remove(defineSymbol);

                try
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defineSymbols.ToArray()));
                }
                catch (Exception e)
                {
                    Debug.LogError("Third Party Manager " + e.StackTrace);
                }
            }
        }
    }

    static void RemoveWaveFollowUp()
    {
        if (RRequest.IsCompleted)
        {
            Client.Remove("com.htc.upm.wave.xrsdk");
            EditorUtility.DisplayDialog("Success", "Wave SDK was uninstalled successfully.", "OK");
            EditorApplication.update -= RemoveWaveFollowUp;
        }
    }

}
