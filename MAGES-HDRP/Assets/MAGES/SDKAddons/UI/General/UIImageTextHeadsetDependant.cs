using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ovidVR.UIManagement;
using System;


public class UIImageTextHeadsetDependant : MonoBehaviour {

    [Serializable]
    private class HeadsetImageText
    {
        public Sprite headsetImage;
        public LanguageTranslator headsetText;
    }

    [SerializeField, Header("Set Oculus Rift / S / Quest")]
    private HeadsetImageText oculusHeadset;

    [SerializeField, Header("Set Vive / WMR")]
    private HeadsetImageText viveHeadset;

    [SerializeField, Header("Add the Image-Child if required")]
    Image uiImage;
    [SerializeField, Header("Add the Text-Child if required")]
    Text uiText; 

    IEnumerator Start()
    {
        // Wait at least two frames for the SteamVR Controller to set up the type of controller
        yield return 0;
        yield return 0;

        if(!uiImage && !uiText)
        {
            Destroy(this);
            yield break;
        }   

        // Usually all other controllers are just like the Oculus
        if (OvidVRControllerClass.Get.controllerType == OvidVRControllerClass.ControllerTypes.HTCViveController ||
            OvidVRControllerClass.Get.controllerType == OvidVRControllerClass.ControllerTypes.WindowsMixedRealityController)
        {
            uiText.text = UIManagement.GetUIMessage(viveHeadset.headsetText);
            if (viveHeadset.headsetImage)
                uiImage.sprite = viveHeadset.headsetImage;
        }
        else
        {
            uiText.text = UIManagement.GetUIMessage(oculusHeadset.headsetText);
            if (oculusHeadset.headsetImage)
                uiImage.sprite = oculusHeadset.headsetImage;
        }
    }

}
