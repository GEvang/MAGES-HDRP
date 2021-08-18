using License.Models;
using ovidVR.OperationAnalytics;
using ovidVR.sceneGraphSpace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ovidVR.UIManagement;
using UnityEngine;
using UnityEngine.Android;

public enum Region
{
    UnitedStates,
    Europe,
    Singapore,
    Auto
};

public enum QualityConfig
{
    High = 0,
    Medium = 1,
    Low = 2
};

public class Configuration : MonoBehaviour
{
    #region Singleton
    private static Configuration configuration;

    public static Configuration Get
    {
        get
        {
            if (!configuration)
            {
                configuration = FindObjectOfType(typeof(Configuration)) as Configuration;

                if (!configuration) { Debug.LogError("Error, No Configuration was found"); }
            }

            return configuration;
        }
    }

    #endregion

    #region ExternalConfigurations
#if UNITY_ANDROID
    [DllImport("StoryBoardDataLicensed")]
    public static extern void SetAppName(StringBuilder _appName);
#endif
    [DllImport("StoryBoardDataLicensed")]
    private static extern void SetXMLNames(StringBuilder _main, StringBuilder _alternativeLessons, StringBuilder _alternativeStages, StringBuilder _alternativeActions);
    [DllImport("StoryBoardDataLicensed")]
    private static extern int SetVersion(StringBuilder _version);
    [DllImport("StoryBoardDataLicensed")]
    private static extern void SetPath(StringBuilder _path);
    
    #endregion

    #region ApplicationConfiguration
    public static string ProductCode { get; set; } // do not change on Unity.Editor

    public bool UserLogin;

    public string productCode;

    public string LanguageTranslationJsonPath;

    public GameObject LoginUI;

    public GameObject VerificationCodeUI;

    public GameObject OperationStartUI;

    public GameObject AnalyticsViewUI;

    public GameObject CustomizationCanvasUI;

#if UNITY_ANDROID
    public static string PackageName {get; set;}
#endif

    public static string Version { get; set; } = "0.0";

    public static QualityConfig Quality { get; set; } = QualityConfig.High;

    public static Region Region { get; set; } = Region.Auto;

    public static UserAccountManager.Difficulty Difficulty { get; set; } = UserAccountManager.Difficulty.Easy;

    public static string EditorPathToXml { get; set; }

#if UNITY_ANDROID
    public static string PathToXml { get; set; } = "/data/data/" + PackageName;
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
    public static string PathToXml { get; set; } = "";
#endif
    #endregion

    #region StoryBoard
    public static string OperationXML = null;
    public static string AlternativeLessonsXML = null;
    public static string AlternativeStagesXML = null;
    public static string AlternativeActionsXML = null;
    #endregion

    #region UserManagement
    public static ApplicationUser User { get; set; } = null;

    public static int Session { get; set; } = 0;

    public static string UserPassword { get; set; }
    #endregion

    #region Analytics
    public static string EditorPathToAnalytics { get; set; }

    public static string OverrideLocalWindowsPath { get; set; }

    public static string OverrideLocalAndroidPath { get; set; }

    public static string OnlineURL { get; set; }

    public static List<AnalyticsExporter.FormField> FormFields { get; set; }

    public static List<AnalyticsExporter.HeaderKey> HeaderKeys { get; set; }

#if UNITY_ANDROID
    public static string PathToAnalytics { get; set; } = "/data/data/" + PackageName + "/Analytics/";
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
    public static string PathToAnalytics { get; set; } = "";
#endif
    #endregion
    public bool enableMicrophone = true;


    private void Awake()
    {
        EditorPathToXml = Application.dataPath + "/Resources/StoryBoard/";
        EditorPathToAnalytics = Application.dataPath + "/../" + "\\Analytics\\";
#if UNITY_ANDROID
        PackageName = Application.identifier;
#endif
#if UNITY_ANDROID
        if(enableMicrophone){
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
        }
#endif
//#if UNITY_STANDALONE_OSX
//        Material HoloMat = Resources.Load("MAGESres/HolographicPrefabs/Materials/HoloMaterial") as Material;
//        Shader HoloShader = Shader.Find("Custom/HolographicShaderMac");
//        HoloMat.shader = HoloShader;
//        Color HoloColor = new Color(0.28235f, 1.0f, 0.0f, 0.15f);
//        HoloMat.color = HoloColor;
//#endif
    }

    public static void SetXmlNames(string main, string alternativeLessons, string alternativestages, string alternativeActions)
    {
        OperationXML = main;
        AlternativeLessonsXML = alternativeLessons;
        AlternativeStagesXML = alternativestages;
        AlternativeActionsXML = alternativeActions;
    }

    public static void ConfigurePreInitialization()
    {
        // 1. Initialize Product Code/Name => This will be appended to the XML path!
        Operation.Get.SetProductName(ProductCode);

        // 2. If Android/Windows specify AppName and XML paths
#if UNITY_ANDROID
        SetAppName(new StringBuilder(PackageName));

        TextAsset xmlAsset;
        String xmlContent;

        if(!String.IsNullOrEmpty(OperationXML) && !System.IO.File.Exists("/data/data/" + PackageName + "/" + OperationXML)){
            xmlAsset = Resources.Load("StoryBoard/platform/" + OperationXML.Substring(0,OperationXML.Length - 4)) as TextAsset;
            if (xmlAsset!=null)
            {
                xmlContent = xmlAsset.text;
                System.IO.File.WriteAllText("/data/data/" + PackageName + "/" + OperationXML, xmlContent); 
            }
        }  
        if(!String.IsNullOrEmpty(AlternativeLessonsXML) && !System.IO.File.Exists("/data/data/" + PackageName + "/" + AlternativeLessonsXML)){
            xmlAsset = Resources.Load("StoryBoard/platform/" + AlternativeLessonsXML.Substring(0, AlternativeLessonsXML.Length - 4)) as TextAsset;
            if (xmlAsset != null)
            {
                xmlContent = xmlAsset.text;
                System.IO.File.WriteAllText("/data/data/" + PackageName + "/" + AlternativeLessonsXML, xmlContent); 
            }
        }
        if (!String.IsNullOrEmpty(AlternativeStagesXML) && !System.IO.File.Exists("/data/data/" + PackageName + "/" + AlternativeStagesXML))
        {
            xmlAsset = Resources.Load("StoryBoard/platform/" + AlternativeStagesXML.Substring(0, AlternativeStagesXML.Length - 4)) as TextAsset;
            if (xmlAsset != null)
            {
                xmlContent = xmlAsset.text;
                System.IO.File.WriteAllText("/data/data/" + PackageName + "/" + AlternativeStagesXML, xmlContent); 
            }
        }
        if (!String.IsNullOrEmpty(AlternativeActionsXML) && !System.IO.File.Exists("/data/data/" + PackageName + "/" + AlternativeActionsXML))
        {
            xmlAsset = Resources.Load("StoryBoard/platform/" + AlternativeActionsXML.Substring(0, AlternativeActionsXML.Length - 4)) as TextAsset;
            if (xmlAsset != null)
            {
                xmlContent = xmlAsset.text;
                System.IO.File.WriteAllText("/data/data/" + PackageName + "/" + AlternativeActionsXML, xmlContent); 
            }
        }
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        PathToXml = "";
        PathToAnalytics = "";
        //PathToXml = Application.dataPath + "\\";
        //PathToAnalytics = Application.dataPath + "\\Analytics\\";
#endif

        // 3. Finally, utilize different paths based on Editor/Runtime
#if UNITY_EDITOR
        SetPath(new StringBuilder(EditorPathToXml));
#else
        SetPath(new StringBuilder(PathToXml));
#endif
        // 4. Set XML names
        SetXMLNames(new StringBuilder(OperationXML),
            new StringBuilder(AlternativeLessonsXML),
            new StringBuilder(AlternativeStagesXML),
            new StringBuilder(AlternativeActionsXML));

        // 5. Set Application Quality
        QualitySettings.SetQualityLevel((int)Quality, true);

        // 6. Set version
        SetVersion(new StringBuilder(Version));

        // 7. Set Region for Coop
        switch (Region)
        {
            case Region.Auto:
                ChangeRegion("");
                break;
            case Region.Europe:
                ChangeRegion("eu");
                break;
            case Region.UnitedStates:
                ChangeRegion("us");
                break;
            case Region.Singapore:
                ChangeRegion("asia");
                break;
            default:
                ChangeRegion("");
                break;
        }
        
        // 8. Set Language
        InterfaceManagement.Get.applicationLanguage = ApplicationLanguages.ENG;
    }

    public static void ConfigurePostInitialization()
    {
        // 1. Initialize UserAccountManager
        // Here you can initialize your user on build.
        // e.g. 
        if (User != null)
        {
            UserAccountManager.Get.InitializeUserAccountManager(User);
        }
        if (!string.IsNullOrEmpty(UserPassword))
        {
            UserAccountManager.Get.SetPassword(UserPassword);
        }
        UserAccountManager.Get.SetOperation(ProductCode);

        // 2. Set AnalyticsExporter configurations
        AnalyticsExporter.overrideLocalWindowsPath = OverrideLocalWindowsPath;
        AnalyticsExporter.overrideLocalAndroidPath = OverrideLocalAndroidPath;
        AnalyticsExporter.Get.onlineURL = OnlineURL;
        AnalyticsExporter.Get.headerKeys = HeaderKeys;
        AnalyticsExporter.Get.formFields = FormFields;
    }

    private static void ChangeRegion(String reg)
    {
        if (string.IsNullOrEmpty(reg) || reg == " ") return;
        NetworkControllerPhoton.Instance.Region = reg;
    }
}
