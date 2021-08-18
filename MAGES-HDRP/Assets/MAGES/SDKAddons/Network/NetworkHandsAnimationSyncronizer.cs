using System;
using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class NetworkHandsAnimationSyncronizer : MonoBehaviour
{
    public bool isServer = false;
    
    private Animator anim;
    private PhotonAnimatorView NetAnim;

    public OvidVRControllerClass.OvidVRHand hand;

    public int colorToReplace;
    // Use this for initialization
    void Start()
    {
        
        hand = this.tag == "LeftPalm" ? OvidVRControllerClass.OvidVRHand.left : OvidVRControllerClass.OvidVRHand.right;
        this.anim = this.GetComponent<Animator>();
        NetAnim = GetComponent<PhotonAnimatorView>();

        if (GetComponent<PhotonView>().IsMine)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                //r.enabled = false;
                Destroy(r);
            }

            if (hand == OvidVRControllerClass.OvidVRHand.left)
            {
                this.transform.parent = OvidVRControllerClass.Get.leftHand.transform;
                this.transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                // OvidVRControllerClass.Get.leftHand.GetComponent<Animator>();
                
            }
            else
            {
                this.transform.parent = OvidVRControllerClass.Get.rightHand.transform;
                this.transform.localPosition = Vector3.zero;
                this.transform.localRotation = Quaternion.identity;
                //NetAnim.animator = OvidVRControllerClass.Get.rightHand.GetComponent<Animator>();
                
            }

            string msgToSend = AvatarManager.Instance.currentCustomizationData.skinIdx.ToString();
            if (OvidVRControllerClass.Get.isServer)
            {
                msgToSend += "-server";
            }
            
            
            var localPlayerCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            if (!localPlayerCustomProperties.ContainsKey("Hands"))
            {
                localPlayerCustomProperties.Add("Hands", msgToSend);
                PhotonNetwork.LocalPlayer.SetCustomProperties(localPlayerCustomProperties);
            }

            
        }
        else
        {
            var handsValue = GetComponent<PhotonView>().Owner.CustomProperties["Hands"];
            
            if (handsValue != null) ColorChange(handsValue.ToString());

            Destroy(GetComponent<AnimationController>());
        }
    }
    
    public void ColorChange(string stringToChangeTo)
    {
        try
        {

            Material handMat;
            string[] splitted = stringToChangeTo.Split('-');
            int valueToChangeTo = int.Parse(splitted[0]);
            if (splitted.Length > 1)
            {
                isServer = true;
            }

            handMat = Resources.Load(
                    "MAGESres/AvatarCustomization/Selections/Materials/Hands/Hand" +
                    valueToChangeTo) as Material;

            if (valueToChangeTo < 0) return;
            colorToReplace = valueToChangeTo;
                
            SkinnedMeshRenderer r = gameObject.transform.Find("vr_glove_right_slim").GetComponent<SkinnedMeshRenderer>();
            if (r)
            {
                r.material = handMat;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

  
    
}
