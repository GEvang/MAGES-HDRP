using ovidVR.GameController;
using ovidVR.OperationAnalytics;
#if FINAL_IK
using RootMotion.FinalIK;
#endif
using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Multiplayer Avatar Script
/// This is script include functions to: 
/// 1.Change Camera position on connect
/// 2.Update Avatar Colors
/// 3.Sync Avatar Colors
/// </summary>
public class MultiplayerAvater : MonoBehaviourPunCallbacks 
{

    public bool isServer = false;

    /// <summary>
    /// displayed username
    /// </summary>
    public string user_name;

    /// <summary>
    /// displayed username 
    ///Be carefull: here is passed the variable is server
    /// </summary>
    public string gender_colorSkin_colorSuit;

    private string materialsPath = "MAGESres/AvatarCustomization/Selections/Materials/";
    
    /// <summary>
    /// Init values when local player spawn avater
    /// 1. Set avatar as child of camera
    /// 2. Disable renderers for the owner
    /// </summary>
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2);
        
        if (!GetComponent<OvidVrNetworkingId>().HasAuthority)
            ChangeLayersRecursively(transform);
        if (photonView.IsMine)
        {
            transform.parent = OvidVRControllerClass.Get.GetCameraHead().transform;

            transform.localPosition = new Vector3(0, 0, 0);
            transform.localRotation = Quaternion.identity;

            if (OvidVRControllerClass.Get.isServer)
            {
                foreach (Renderer r in GetComponentsInChildren<Renderer>())
                {
                    r.enabled = false;
                }
            }
            else
            {
                foreach (Renderer r in 
                    AvatarManager.Instance.currentAvatarSkeletonReference.avatar.GetComponentsInChildren<Renderer>())
                {
                    r.enabled = false;
                }
            }


            string profile = AvatarManager.Instance.currentCustomizationData.genderIdx + "-" +
                             AvatarManager.Instance.currentCustomizationData.skinIdx + "-" +
                             AvatarManager.Instance.currentCustomizationData.suitIdx;


            //Be carefull: here is passed the variable is server
            if (OvidVRControllerClass.Get.isServer)
            {
                profile += "-server";
            }

        
            var localPlayerCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            localPlayerCustomProperties.Add("Username", UserAccountManager.Get.GetUsername());
            localPlayerCustomProperties.Add("Profile", profile);
            PhotonNetwork.LocalPlayer.SetCustomProperties(localPlayerCustomProperties);
        }
        else
        {
            var profileValue = photonView.Owner.CustomProperties["Profile"];
            var userValue = photonView.Owner.CustomProperties["Username"];

            if (profileValue != null) ChangeProfile(profileValue.ToString());
            
            if(userValue!=null) ChangeName(userValue.ToString());

        }



        Debug.Log("Settings changed ");
    }

    /// <summary>
    /// Change VRCamera Rig position 
    /// </summary>
    /// <param name="_posID"></param>
    void OnPositionChange(int _posID)
    {
        if (!GetComponent<OvidVrNetworkingId>().HasAuthority)
            return;

        GameObject camera = OvidVRControllerClass.Get.GetCameraHead();
        Transform cameraRig = OvidVRControllerClass.Get.GetCameraRig().transform;

        Transform spawnPoint = null;
        Transform currentCameraParent = cameraRig.parent;
        cameraRig.parent = null;

        GameObject spawnPositions = GameObject.Find("SpawnPositions");
        if (spawnPositions)
        {
            if (_posID < spawnPositions.transform.childCount)
            {
                spawnPoint = spawnPositions.transform.GetChild(_posID);
            }
            else
            {
                spawnPoint = spawnPositions.transform.GetChild(spawnPositions.transform.childCount - 1);
            }
        }
        if (spawnPoint)
        {
            cameraRig.position = spawnPoint.position;
            cameraRig.rotation = spawnPoint.rotation;
        }
        cameraRig.parent = currentCameraParent;
    }

    private void ChangeLayersRecursively(Transform trans)
    {
        foreach (Transform child in trans)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Default");
            ChangeLayersRecursively(child);
        }
    }


    /// <summary>
    /// Hook, change username in current avatar
    /// </summary>
    /// <param name="_usrname"></param>
    public void ChangeName(string _usrname)
    {
        if(_usrname[_usrname.Length-1].Equals('-'))
        {
            return;
        }
        if(transform.FindDeepChild("UserNameCanvas"))
        {
            Transform usrNameGameobject = transform.FindDeepChild("UserNameCanvas");
            Text tm = usrNameGameobject.GetComponentInChildren<Text>();
            if(tm)
            {
                tm.text = _usrname;
            }
        }
        user_name = _usrname;
    }

    /// <summary>
    /// Hook, change profile in current avatar
    /// </summary>
    /// <param name="_usrname"></param>
    public void ChangeProfile(string _profile)
    {
        try
        {
            
            string[] profile_data = _profile.Split('-');
            if (profile_data.Length < 2)
            {
                Debug.LogError("Found wrong profile data for  " + user_name + " " + _profile);
            }
            if(profile_data.Length > 3 && profile_data[3].Equals("01"))
            {
                return;
            }

            
            int gender = 0; 
            int.TryParse(profile_data[0],out gender);
            int colorSkin = 0;
            int.TryParse(profile_data[1],out colorSkin);
            int colorSuitChanged = 0;
            int.TryParse(profile_data[2],out colorSuitChanged);

            try
            {
                //Get the correct gender
                Transform destroyGO = gender == 0
                    ? gameObject.transform.Find("Avatars").Find("Female")
                    : gameObject.transform.Find("Avatars").Find("Male");
                if (destroyGO)
                {
                    Destroy(destroyGO.gameObject);
                }

                if (gender == 0)
                {
                    Transform UserNameCanvas = transform.FindDeepChild("UserNameCanvas");
                    #if FINAL_IK
                    UserNameCanvas.parent =
                        gameObject.transform.Find("Avatars").Find("Male").GetComponent<LookAtIK>().solver.head
                            .transform;
                    UserNameCanvas.localRotation = Quaternion.Euler(new Vector3(-90, 90, 0));
                    #endif
                }
                else
                {
                    Transform UserNameCanvas = transform.FindDeepChild("UserNameCanvas");
                    #if FINAL_IK
                    UserNameCanvas.parent =
                        gameObject.transform.Find("Avatars").Find("Female").GetComponent<LookAtIK>().solver.head
                            .transform;
                    UserNameCanvas.localRotation = Quaternion.Euler(new Vector3(-90, 90, 0));
                    #endif
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            //Apply the rest of the profile (skin/suit)
            try
            {
                //Apply (skin to male)
                if (gender == 0)
                {
                    Transform model = gameObject.transform.Find("Avatars").Find("Male");
                    Material skin = Resources.Load(materialsPath + "Avatar0/Skin" + colorSkin) as Material;
                    model.transform.FindDeepChild("Body_GEO").GetComponent<SkinnedMeshRenderer>().material = skin;
                }
                //Apply (skin to female)
                else
                {
                    Transform model = gameObject.transform.Find("Avatars").Find("Female");
                    Material faceMat = Resources.Load(materialsPath + "Avatar1/Skin" + colorSkin + "0") as Material;
                    Material skinMat = Resources.Load(materialsPath + "Avatar1/Skin" + colorSkin + "1") as Material;
                    Material[] mats = new Material[2];
                    mats[0] = faceMat;
                    mats[1] = skinMat;
                    model.transform.FindDeepChild("Body_GEO2").GetComponent<SkinnedMeshRenderer>().materials = mats;
                    
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            try
            {
                //Apply (suit to male)
                Material suit = Resources.Load(materialsPath + "Suits/Suit" + colorSuitChanged) as Material;
                if (gender == 0)
                {
                    Transform model = gameObject.transform.Find("Avatars").Find("Male");
                    model.Find("male_nurse_suit").GetComponent<SkinnedMeshRenderer>().material = suit;
                }
                else
                {
                    //Apply (suit to female)
                    Transform model = gameObject.transform.Find("Avatars").Find("Female");
                    model.transform.FindDeepChild("female_nurse_suit").GetComponent<SkinnedMeshRenderer>().material =
                        suit;
                }
            }
            catch (Exception e)
            {

                Debug.LogError(e);
            }
            
            if (profile_data.Length == 4 && profile_data[3] == "server")
            {
                isServer = true;
                if (transform.FindDeepChild("SurgicalMaleInPlaceCoop") != null)
                    Destroy(transform.FindDeepChild("SurgicalMaleInPlaceCoop").gameObject);
                if (transform.FindDeepChild("Gown_Male_InPlace_Coop") != null)
                    Destroy(transform.FindDeepChild("Gown_Male_InPlace_Coop").gameObject);
                if (transform.FindDeepChild("SurgicalFemaleInPlaceCoop") != null)
                    Destroy(transform.FindDeepChild("SurgicalFemaleInPlaceCoop").gameObject);
                if (transform.FindDeepChild("Gown_Female_InPlace_Coop") != null)
                    Destroy(transform.FindDeepChild("Gown_Female_InPlace_Coop").gameObject);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.StackTrace);
        }
    }



}