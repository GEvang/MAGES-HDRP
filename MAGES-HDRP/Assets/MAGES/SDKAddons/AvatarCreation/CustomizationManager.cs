using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ovidVR.GameController;
using ovidVR.OperationAnalytics;
using ovidVR.UIManagement;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomizationManager : MonoBehaviour
{
    [HideInInspector]
    public bool resetAvatar = false;
    
    public enum CustomizationState { Start, Gender, Skin, Suit, Final}
    private List<Action> initializeStates = new List<Action>();
    private List<Action> performState = new List<Action>();
    private List<Action> undoState = new List<Action>();
    private static CustomizationManager _singleton;
    public CustomizationState currentState = CustomizationState.Start;
    public AvatarManager.CustomizationData currentCreation;
    private AvatarManager.AvatarSkeletonReference currentAvatarReference;

    public GameObject currentAvatar;

    private bool fileExists;

    [HideInInspector]
    public GameObject _customizationUI;
    [HideInInspector]
    public GameObject _selectionsUI;
    [HideInInspector]
    public GameObject _descriptionUI;
    [HideInInspector]
    public GameObject _prevButtonUI;
    [HideInInspector]
    public GameObject _nextButtonUI;

    private List<GameObject> _selections;
    // Use this for initialization
    IEnumerator Start () {

        //Check if customization is needed. if yes destroy everything
        currentCreation = new AvatarManager.CustomizationData();
        
        _selectionsUI = transform.Find("Selections").gameObject;
        _prevButtonUI = transform.Find("LeftOption").gameObject;
        _nextButtonUI = transform.Find("RightOption").gameObject;
        _descriptionUI = transform.Find("Description").gameObject;


        CustomizationActions customActions = GetComponent<CustomizationActions>();
        //Wait for initialization
        while (!customActions.initialized)
        {
            yield return new WaitForEndOfFrame();
        }

        if (!customActions)
        {
            Debug.LogError("No custom actions script found on customization manager.");
            Destroy(gameObject);
        }

        initializeStates.Add(customActions.InitializeStart);
        initializeStates.Add(customActions.InitializeGender);
        initializeStates.Add(customActions.InitializeSkin);
        initializeStates.Add(customActions.InitializeSuit);
        initializeStates.Add(customActions.InitializeFinal);

        performState.Add(customActions.PerformStart);
        performState.Add(customActions.PerformGender);
        performState.Add(customActions.PerformSkin);
        performState.Add(customActions.PerformSuit);
        performState.Add(customActions.PerformFinal);

        undoState.Add(customActions.UndoStart);
        undoState.Add(customActions.UndoGender);
        undoState.Add(customActions.UndoSkin);
        undoState.Add(customActions.UndoSuit);
        undoState.Add(customActions.UndoFinal);

        currentCreation = LoadCurrentFile();
        currentAvatar = Instantiate(Resources.Load("MAGESres/General Models/Avatar_" + currentCreation.genderIdx), GameObject.Find("Avatar").transform) as GameObject;
        Material suitMat = Resources.Load("MAGESres/AvatarCustomization/Selections/Materials/Suits/Suit" + currentCreation.suitIdx) as Material;
        if (currentCreation.genderIdx == 0)
        {
            Material skinMat = Resources.Load("MAGESres/AvatarCustomization/Selections/Materials/Avatar" + currentCreation.genderIdx + "/Skin" + currentCreation.skinIdx) as Material;
            currentAvatar.transform.Find("group1")
                .transform.Find("group 1")
                .transform.Find("Man_All_GRP")
                .transform.Find("Man_GEO_GRP")
                .transform.Find("Body_GEO_GRP")
                .transform.Find("Body_GEO")
                .GetComponent<SkinnedMeshRenderer>().material = skinMat;
        }
        else
        {
            Material faceMat = Resources.Load("MAGESres/AvatarCustomization/Selections/Materials/Avatar" + currentCreation.genderIdx + "/Skin" + currentCreation.skinIdx+"0") as Material;
            Material skinMat = Resources.Load("MAGESres/AvatarCustomization/Selections/Materials/Avatar" + currentCreation.genderIdx + "/Skin" + currentCreation.skinIdx + "1") as Material;

            Material[] mats = new Material[2];
            mats[0] = faceMat;
            mats[1] = skinMat;

            currentAvatar.transform.Find("group1")
                .transform.Find("group 1")
                .transform.Find("Woman_GRP")
                .transform.Find("Woman_GEO_GRP")
                .transform.Find("body_geo")
                .transform.Find("Body_GEO2")
                .GetComponent<SkinnedMeshRenderer>().materials = mats;
            
            
        }
        
        
        if (currentCreation.genderIdx == 0)
        {
            currentAvatar.transform.Find("male_nurse_suit").GetComponent<SkinnedMeshRenderer>().material = suitMat;
        }
        else
        {
            currentAvatar.transform.Find("group1")
                .transform.Find("group 1")
                .transform.Find("Woman_GRP")
                .transform.Find("Woman_GEO_GRP")
                .transform.Find("body_geo")
                .transform.Find("female_nurse_suit")
                .GetComponent<SkinnedMeshRenderer>().material = suitMat;
        }
        
        //change skin in hands
        Material handMat = Resources.Load("MAGESres/AvatarCustomization/Selections/Materials/Hands/Hand" +
                                          currentCreation.skinIdx) as Material;
        OvidVRControllerClass.Get.leftHand.transform.Find("HandRenderer").
            GetComponent<SkinnedMeshRenderer>().material = handMat;

        OvidVRControllerClass.Get.rightHand.transform.Find("HandRenderer").
            GetComponent<SkinnedMeshRenderer>().material = handMat;
        yield return 0;

        
        if (fileExists)
        {
            SkipAllActions();
            
            yield break;
        }

        initializeStates[(int)currentState].Invoke();
    }

    public void ChangeDescription(string description)
    {
        _descriptionUI.GetComponent<Text>().text = description;
    }

    #region FileFunctions
    public void SaveFile()
    {
        string json = JsonConvert.SerializeObject(currentCreation, Formatting.Indented);

#if UNITY_STANDALONE_WIN
        string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/ORamaVR/Profile/" +
                               Configuration.ProductCode + "/";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        string jsonFullPath = directoryPath + UserAccountManager.Get.GetUsername()+".json";

#elif UNITY_EDITOR
       string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/ORamaVR/Profile/" +
                               Configuration.ProductCode + "/";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        string jsonFullPath = directoryPath + UserAccountManager.Get.GetUsername()+".json";
#elif UNITY_STANDALONE_OSX
    string directoryPath = "/Users/" + Environment.UserName + "/Documents/ORamaVR/Profile/" + Configuration.ProductCode + "/";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        string jsonFullPath = directoryPath + UserAccountManager.Get.GetUsername()+".json";

#elif UNITY_ANDROID
        //Find how to write in android
        //string jsonFullPath = Environment.CurrentDirectory +"Assets"+unityJsonPath+ "/" + UserAccountManager.Get.GetUsername() + ".json";
        System.IO.File.WriteAllText(Application.persistentDataPath  + "/" + UserAccountManager.Get.GetUsername() + ".json", json);

#endif
#if !UNITY_ANDROID
        if (File.Exists(jsonFullPath))
        {
            File.Delete(jsonFullPath);
        }

        File.WriteAllText(jsonFullPath, json);
#endif
    }

    IEnumerator UploadProfile(string path)
    {
        if (Application.isEditor)
        {
            yield return 0;
        }

        // Check if filepath exists
        if (!(File.Exists(path)))
        {
            Debug.LogError("[AvatarCustomization.ProfileUpload] Filepath doesn't exist.");
            yield return 0;
        }
        // Now read s into a byte buffer with a little padding.
        byte[] bytesProfile = File.ReadAllBytes(path);
        WWWForm form = new WWWForm();
        form.AddField("Username", UserAccountManager.Get.GetUsername());
        form.AddField("Password", UserAccountManager.Get.GetPassword());
        form.AddBinaryData("file", bytesProfile, Path.GetFileName(path));

        UnityWebRequest www = UnityWebRequest.Post("http://elearn-oramavr.azurewebsites.net/analytics/Analytics/UploadProfile", form);

        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Profile upload complete!");
        }
    }

    AvatarManager.CustomizationData LoadCurrentFile()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        string jsonFullPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/ORamaVR/Profile/" + Configuration.ProductCode + "/" + UserAccountManager.Get.GetUsername() + ".json";
#elif UNITY_STANDALONE_OSX
        string jsonFullPath = "/Users/" + Environment.UserName + "/Documents/ORamaVR/Profile/" + Configuration.ProductCode + "/" + UserAccountManager.Get.GetUsername() + ".json";
#else
        string jsonFullPath;

        jsonFullPath = Application.persistentDataPath + "/" + UserAccountManager.Get.GetUsername() + ".json";
        
#endif
        AvatarManager.CustomizationData data = new AvatarManager.CustomizationData();

        
        if (File.Exists(jsonFullPath))
        {
            if (resetAvatar) 
            {
                File.Delete(jsonFullPath);
                return new AvatarManager.CustomizationData();
            }

            using (StreamReader r = new StreamReader(jsonFullPath))
            {
                string json = r.ReadToEnd();
                data = JsonConvert.DeserializeObject<AvatarManager.CustomizationData>(json);
            }
        }
        else
        {
            return new AvatarManager.CustomizationData();
        }

        fileExists = true;
        return data;
    }


#endregion


    #region UI_Functions

    public void SelectOption(int selection)
    {
        int i = 0;
        foreach (Transform t in _selectionsUI.transform)
        {
            if (i != selection)
            {
                t.gameObject.GetComponent<ButtonBehavior>().ResetButton();
            }

            i++;
        }
        switch (currentState)
        {
            case CustomizationState.Start:

                break;
            case CustomizationState.Gender:
                currentCreation.genderIdx = selection;
                Transform currentAvatarTransform = GameObject.Find("Avatar").transform.GetChild(0).transform;
                
                currentAvatar = Instantiate(Resources.Load("MAGESres/General Models/Avatar_" + selection), GameObject.Find("Avatar").transform) as GameObject;
                currentAvatar.transform.position = currentAvatarTransform.position;
                currentAvatar.transform.rotation = currentAvatarTransform.rotation;

                Destroy(GameObject.Find("Avatar").transform.GetChild(0).gameObject);
                break;
            case CustomizationState.Skin:
                currentCreation.skinIdx = selection;
                if (currentCreation.genderIdx == 0)
                {
                    Material skinMat = Resources.Load("MAGESres/AvatarCustomization/Selections/Materials/Avatar" + currentCreation.genderIdx + "/Skin" + selection) as Material;
                    currentAvatar.transform.Find("group1")
                        .transform.Find("group 1")
                        .transform.Find("Man_All_GRP")
                        .transform.Find("Man_GEO_GRP")
                        .transform.Find("Body_GEO_GRP")
                        .transform.Find("Body_GEO")
                        .GetComponent<SkinnedMeshRenderer>().material = skinMat;
                }
                else
                {
                    Material faceMat = Resources.Load("MAGESres/AvatarCustomization/Selections/Materials/Avatar" + currentCreation.genderIdx + "/Skin" + selection+"0") as Material;
                    Material skinMat = Resources.Load("MAGESres/AvatarCustomization/Selections/Materials/Avatar" + currentCreation.genderIdx + "/Skin" + selection + "1") as Material;
                    Material[] mats = new Material[2];
                    mats[0] = faceMat;
                    mats[1] = skinMat;

                    currentAvatar.transform.Find("group1")
                        .transform.Find("group 1")
                        .transform.Find("Woman_GRP")
                        .transform.Find("Woman_GEO_GRP")
                        .transform.Find("body_geo")
                        .transform.Find("Body_GEO2")
                        .GetComponent<SkinnedMeshRenderer>().materials = mats;
                }
                break;
            case CustomizationState.Suit:
                currentCreation.suitIdx = selection;
                Material suitMat = Resources.Load("MAGESres/AvatarCustomization/Selections/Materials/Suits/Suit" + selection) as Material;

                if (currentCreation.genderIdx == 0)
                {
                    currentAvatar.transform.Find("male_nurse_suit").GetComponent<SkinnedMeshRenderer>().material = suitMat;
                }
                else
                {
                    currentAvatar.transform.Find("group1")
                        .transform.Find("group 1")
                        .transform.Find("Woman_GRP")
                        .transform.Find("Woman_GEO_GRP")
                        .transform.Find("body_geo")
                        .transform.Find("female_nurse_suit")
                        .GetComponent<SkinnedMeshRenderer>().material = suitMat;
                }
                break;
            case CustomizationState.Final:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Restart()
    {
        // WHY??
        //var manager = GameObject.Find("CustomizationManager").GetComponent<CustomizationManager>();
        var manager = this;

        for (int i = manager.undoState.Count - 1; i > 1; i--)
        {
            manager.undoState[i].Invoke();
            manager.initializeStates[i - 1].Invoke();
            manager.currentState = (CustomizationState)i - 1;
        }
    }

    public void NextState()
    {
        var currentStateNum = (int) currentState;
        performState[currentStateNum].Invoke();
        if (currentState == CustomizationState.Final) return;
        initializeStates[currentStateNum + 1].Invoke();
        currentState = (CustomizationState)currentStateNum + 1;
    }

    public void PrevState()
    {
        
        var currentStateNum = (int)currentState;
        undoState[currentStateNum].Invoke();
        if (currentState == CustomizationState.Start) return;
        initializeStates[(int)currentStateNum - 1].Invoke();
        currentState = (CustomizationState)currentStateNum - 1;


    }
#endregion

    #region AccessAvatar

    public void InitializeAvatarReference()
    {
        currentAvatarReference.avatar = currentAvatar;

        SetAllAvatarReferences(currentAvatar.transform);
        AvatarManager.Instance.SetAvatarReference(currentAvatarReference);
    }

    private void SetAllAvatarReferences(Transform currentRoot)
    {
        foreach (Transform child in currentRoot)
        {
            
            if (child.name.Equals("Chest_M_Avatar"))
            {
                currentAvatarReference.chest = child.gameObject;
            }
            if (child.name.Equals("Neck_M_Avatar"))
            {
                currentAvatarReference.neck = child.gameObject;
            }
            if (child.name.Equals("Head_M_Avatar"))
            {
                currentAvatarReference.head = child.gameObject;
            }
            if (child.name.Equals("Root_M"))
            {
                currentAvatarReference.root = child.gameObject;
            }
            if (child.name.Equals("Spine1_M"))
            {
                currentAvatarReference.spine = child.gameObject;
            }
            
            if (child.childCount > 0) SetAllAvatarReferences(child);
        } 
    }
    


    #endregion

    public void SkipAllActions()
    {
        for (int i = 0; i < initializeStates.Count; i++)
        {
            initializeStates[i].Invoke();
            performState[i].Invoke();
            currentState = (CustomizationState)i;
        }
    }
}
