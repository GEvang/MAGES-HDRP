using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.GameController;
using System;

public class ControllerButtonTutorial : MonoBehaviour
{
    bool[] buttonPressed;

    Material[] controllerRenderedMaterials;
    Color controllerMaterialColor;

    public Color BaseHandColor;

    OvidVRControllerClass.OvidVRHand whichController;

    struct ButtonFlash
    {
        public Material buttonMat;      // button's material 
        public OvidVRControllerClass.OvidVRControllerButtons button; // button's name
        public bool allowFlash;         // if button is pressed don't allow flashing

        public ButtonFlash(Material _mat, OvidVRControllerClass.OvidVRControllerButtons _button, bool _flash)
        {
            buttonMat = _mat;
            button = _button;
            allowFlash = _flash;
        }
    }

    List<ButtonFlash> buttonFlashList, buttonPressList;

    void Awake()
    {
        // All Controller Renderers
        Renderer[] allRenderers = this.gameObject.GetComponentsInChildren<Renderer>(true);
        controllerRenderedMaterials = new Material[allRenderers.Length];

        if (allRenderers[0].materials[0].HasProperty("_BaseColor")) //Oculus OVRAvatarSelfOccludingShader configuration
        {
            for (int i = 0; i < allRenderers.Length; ++i)
            {
                controllerRenderedMaterials[i] = allRenderers[i].material;
                controllerRenderedMaterials[i].EnableKeyword("_EMISSION");

                controllerRenderedMaterials[i].EnableKeyword("_ALPHABLEND_ON");
                controllerRenderedMaterials[i].DisableKeyword("_ALPHATEST_ON");
                controllerRenderedMaterials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                controllerRenderedMaterials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                controllerRenderedMaterials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                controllerRenderedMaterials[i].SetInt("_ZWrite", 0);

                controllerRenderedMaterials[i].SetFloat("_Mode", 2f); //Opaque
            } 
        }

        // Default Color Of Controller
        if (OvidVRControllerClass.Get.rightHand.transform.Find("HandRenderer").GetComponent<SkinnedMeshRenderer>().materials[0].HasProperty("_LayerColor1"))
        {
            controllerMaterialColor = OvidVRControllerClass.Get.rightHand.transform.Find("HandRenderer").GetComponent<SkinnedMeshRenderer>().materials[0].GetColor("_LayerColor1"); 
        }

        BaseHandColor = OvidVRControllerClass.Get.rightHand.transform.Find("HandRenderer").GetComponent<SkinnedMeshRenderer>().materials[0].HasProperty("_LayerColor0") 
            ?  
              OvidVRControllerClass.Get.rightHand.transform.Find("HandRenderer").GetComponent<SkinnedMeshRenderer>().materials[0].GetColor("_LayerColor0") 
            : OvidVRControllerClass.Get.rightHand.transform.Find("HandRenderer").GetComponent<SkinnedMeshRenderer>().materials[0].GetColor("_Color");

        buttonFlashList = new List<ButtonFlash>();
        buttonPressList = new List<ButtonFlash>();

        int myEnumMemberCount = Enum.GetNames(typeof(OvidVRControllerClass.OvidVRControllerButtons)).Length;
        buttonPressed = new bool[myEnumMemberCount];
        for (int i = 0; i < sizeof(OvidVRControllerClass.OvidVRControllerButtons); ++i)
            buttonPressed[i] = false;

        // SetUp left and right Controller
        if (gameObject.tag.Equals("RightPalm"))
            whichController = OvidVRControllerClass.OvidVRHand.right;
        else if (gameObject.tag.Equals("LeftPalm"))
            whichController = OvidVRControllerClass.OvidVRHand.left;

        SetControllerTransparency(0.36f, 0.8f);
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (buttonFlashList != null)
            buttonFlashList.Clear();

        if (buttonPressList != null)
            buttonPressList.Clear();

    }

    void Update()
    {
        if (OvidVRControllerClass.Get.GetIsGrabed(whichController))
            CheckButtonPressed(OvidVRControllerClass.OvidVRControllerButtons.GripButton, true);
        else
            CheckButtonPressed(OvidVRControllerClass.OvidVRControllerButtons.GripButton, false);

        if (OvidVRControllerClass.Get.GetControllerGrabStrength(whichController) >= 0.5f)
            CheckButtonPressed(OvidVRControllerClass.OvidVRControllerButtons.TriggerButton, true);
        else
            CheckButtonPressed(OvidVRControllerClass.OvidVRControllerButtons.TriggerButton, false);

        //if (OvidVRControllerClass.DeviceController.GetTrackPadPressed(whichController))
        //    CheckButtonPressed(OvidVRControllerClass.OvidVRControllerButtons.ThumbStick, true);
        //else
        //    CheckButtonPressed(OvidVRControllerClass.OvidVRControllerButtons.ThumbStick, false);
    }

    private void CheckButtonPressed(OvidVRControllerClass.OvidVRControllerButtons _button, bool _isPressed)
    {
        if (buttonFlashList == null || buttonPressList == null)
            return;

        if (_isPressed)
        {
            if (buttonPressed[(int)_button])
                return;

            buttonPressed[(int)_button] = true;

            if (buttonFlashList.Count > 0 || buttonPressList.Count > 0 && gameObject.activeSelf)
                StopCoroutine("PlayButtonFlashAnimation");

            // Search the flashing list - if the pressed button is flashing already stop it
            for (int i = 0; i < buttonFlashList.Count; ++i)
            {
                // A button will flash when pressed ONLY if the tutorial flash animation is on
                if (buttonFlashList[i].button == _button)
                {
                    // Add to pressed List                 
                    buttonPressList.Add(buttonFlashList[i]);
                    // stop flashing from flash list
                    buttonFlashList[i] = new ButtonFlash(buttonFlashList[i].buttonMat, buttonFlashList[i].button, false);
                }
            }

            if (buttonFlashList.Count > 0 || buttonPressList.Count > 0 && gameObject.activeSelf)
                StartCoroutine("PlayButtonFlashAnimation");
        }
        else
        {
            if (!buttonPressed[(int)_button])
                return;

            buttonPressed[(int)_button] = false;

            if (buttonFlashList.Count > 0 || buttonPressList.Count > 0 && gameObject.activeSelf)
                StopCoroutine("PlayButtonFlashAnimation");

            // Remove from list for button press flash animation the released button
            foreach (ButtonFlash button in buttonPressList)
            {
                if (button.button == _button)
                {
                    button.buttonMat.SetColor("_LayerColor1", controllerMaterialColor);
                    button.buttonMat.SetColor("_LayerColor1", new Color(0, 0, 0, 1f));
                    button.buttonMat.SetColor("_LayerColor2", controllerMaterialColor);
                    button.buttonMat.SetColor("_LayerColor2", new Color(0, 0, 0, 1f));
                    button.buttonMat.SetColor("_LayerColor3", controllerMaterialColor);

                    if (button.buttonMat.HasProperty("_LayerColor0"))
                    {
                        button.buttonMat.SetColor("_LayerColor0", BaseHandColor);
                    }
                    else
                    {
                        button.buttonMat.SetColor("_Color", BaseHandColor);
                    }
                    button.buttonMat.SetInt("_LayerBlendMode0", 0);

                    buttonPressList.Remove(button);
                    break;
                }
            }

            // Restart the button flash if it was stopped while it was pressed
            for (int i = 0; i < buttonFlashList.Count; ++i)
            {
                if (buttonFlashList[i].button == _button && buttonFlashList[i].allowFlash == false)
                    buttonFlashList[i] = new ButtonFlash(buttonFlashList[i].buttonMat, buttonFlashList[i].button, true);
            }

            if (buttonFlashList.Count > 0 || buttonPressList.Count > 0 && gameObject.activeSelf)
                StartCoroutine("PlayButtonFlashAnimation");
        }
    }

    IEnumerator PlayButtonFlashAnimation()
    {
        Color emissionColor = new Color();

        while (true)
        {
            yield return null;

            foreach (ButtonFlash mat in buttonFlashList)
            {
                if (mat.allowFlash)
                {
                    emissionColor = Color.Lerp(controllerMaterialColor, (Color.yellow * 2f), /*Mathf.PingPong(Time.time * 2f, 1)*/ 0.25f);
                    mat.buttonMat.SetColor("_LayerColor1", emissionColor);
                    mat.buttonMat.SetColor("_LayerColor2", emissionColor);
                    mat.buttonMat.SetColor("_LayerColor3", emissionColor);

                    if (mat.buttonMat.HasProperty("_LayerColor0"))
                    {
                        mat.buttonMat.SetColor("_LayerColor0", BaseHandColor);
                    }
                    else
                    {
                        mat.buttonMat.SetColor("_Color", BaseHandColor);
                    }
                    mat.buttonMat.SetInt("_LayerBlendMode0", 1);


                }
            }

            foreach (ButtonFlash mat in buttonPressList)
            {
                emissionColor = Color.Lerp(controllerMaterialColor, (Color.green * 2f), /*Mathf.PingPong(Time.time * 4f, 1)*/ 0.25f);
                mat.buttonMat.SetColor("_LayerColor1", emissionColor);
                mat.buttonMat.SetColor("_LayerColor2", emissionColor);
                mat.buttonMat.SetColor("_LayerColor3", emissionColor);

                if (mat.buttonMat.HasProperty("_LayerColor0"))
                {
                    mat.buttonMat.SetColor("_LayerColor0", BaseHandColor);
                }
                else
                {
                    mat.buttonMat.SetColor("_Color", BaseHandColor);
                }
                mat.buttonMat.SetInt("_LayerBlendMode0", 1);

            }
        }
    }

    // User Accessible -------------------------------------------------

    public void SetControllerButtonNotification(bool _enable, OvidVRControllerClass.OvidVRControllerButtons _button)
    {
        if (buttonFlashList == null || buttonPressList == null)
            return;

        if (buttonFlashList.Count > 0 && gameObject.activeSelf)
            StopCoroutine("PlayButtonFlashAnimation");

        if (_enable)
        {

            //for (int i = 0; i < transform.childCount; ++i)
            //{
            if (true)
            {
                ButtonFlash aBut = new ButtonFlash();
                if (_button == OvidVRControllerClass.OvidVRControllerButtons.GripButton)
                {
                    switch (whichController)
                    {
                        case OvidVRControllerClass.OvidVRHand.left:
                            {
                                aBut = new ButtonFlash(OvidVRControllerClass.Get.leftHand.transform.Find("HandRenderer").GetComponent<SkinnedMeshRenderer>().materials[0], _button, true);
                                break;
                            }
                        case OvidVRControllerClass.OvidVRHand.right:
                            {
                                aBut = new ButtonFlash(OvidVRControllerClass.Get.rightHand.transform.Find("HandRenderer").GetComponent<SkinnedMeshRenderer>().materials[0], _button, true);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                else if (_button == OvidVRControllerClass.OvidVRControllerButtons.TriggerButton)
                {
                    switch (whichController)
                    {
                        case OvidVRControllerClass.OvidVRHand.left:
                            {
                                aBut = new ButtonFlash(OvidVRControllerClass.Get.leftHand.transform.Find("HandRenderer").GetComponent<SkinnedMeshRenderer>().materials[1], _button, true);
                                break;
                            }
                        case OvidVRControllerClass.OvidVRHand.right:
                            {
                                aBut = new ButtonFlash(OvidVRControllerClass.Get.rightHand.transform.Find("HandRenderer").GetComponent<SkinnedMeshRenderer>().materials[1], _button, true);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                buttonFlashList.Add(aBut);
            }
            //}
        }
        else
        {
            foreach (ButtonFlash button in buttonFlashList)
            {
                if (button.button == _button)
                {
                    button.buttonMat.SetColor("_LayerColor1", Color.black);
                    button.buttonMat.SetColor("_LayerColor2", Color.black);
                    button.buttonMat.SetColor("_LayerColor3", Color.black);

                    if (button.buttonMat.HasProperty("_LayerColor0"))
                    {
                        button.buttonMat.SetColor("_LayerColor0", BaseHandColor);
                    }
                    else
                    {
                        button.buttonMat.SetColor("_Color", BaseHandColor);
                    }
                    button.buttonMat.SetInt("_LayerBlendMode0", 0);

                    //button.buttonMat.SetColor("_LayerColor1", new Color(0, 0, 0, 1f));

                    buttonFlashList.Remove(button); // Not safe in foreach -> change to for(;;--i)
                    break;
                }
            }

            foreach (ButtonFlash button in buttonPressList)
            {
                if (button.button == _button)
                {
                    button.buttonMat.SetColor("_LayerColor1", Color.black);
                    button.buttonMat.SetColor("_LayerColor2", Color.black);
                    button.buttonMat.SetColor("_LayerColor3", Color.black);

                    if (button.buttonMat.HasProperty("_LayerColor0"))
                    {
                        button.buttonMat.SetColor("_LayerColor0", BaseHandColor);
                    }
                    else
                    {
                        button.buttonMat.SetColor("_Color", BaseHandColor);
                    }
                    button.buttonMat.SetInt("_LayerBlendMode0", 0);

                    //button.buttonMat.SetColor("_LayerColor1", new Color(0, 0, 0, 1f));

                    buttonPressList.Remove(button);
                    break;
                }
            }
        }

        if (buttonFlashList.Count > 0 && gameObject.activeSelf)
            StartCoroutine("PlayButtonFlashAnimation");
    }

    public void SetControllerTransparency(float _alphaColorBody, float _alphaColorButtons)
    {
        if (controllerRenderedMaterials == null)
            return;

        if (_alphaColorBody < 0f || _alphaColorBody > 1f)
            return;

        if (_alphaColorButtons < 0f || _alphaColorButtons > 1f)
            return;

        //for (int i = 0; i < controllerRenderedMaterials.Length; ++i)
        //{
        //Color alpha = controllerRenderedMaterials[i].color;

        //if (transform.GetChild(i).gameObject.name == OvidVRControllerClass.OvidVRControllerButtons.GripButton.ToString() ||
        //        transform.GetChild(i).gameObject.name == OvidVRControllerClass.OvidVRControllerButtons.TriggerButton.ToString() ||
        //            transform.GetChild(i).gameObject.name == OvidVRControllerClass.OvidVRControllerButtons.ThumbStick.ToString() ||
        //                transform.GetChild(i).gameObject.name == OvidVRControllerClass.OvidVRControllerButtons.MenuButton.ToString())
        //{
        //    alpha.a = _alphaColorButtons;
        //    controllerRenderedMaterials[i].color = alpha;
        //}
        //else
        //{
        //    alpha.a = _alphaColorBody;
        //    controllerRenderedMaterials[i].color = alpha;
        //}

        //}
    }

    //[System.Obsolete("Obsolete Method, use: SetControllerButtonNotification(bool _enable, ControllerButton _button)")]
    //public void StartControllerButtonNotification(OvidVRControllerClass.OvidVRControllerButtons _button)
    //{
    //    if (buttonFlashList == null)
    //        return;

    //    if (buttonFlashList.Count > 0 && gameObject.activeSelf)
    //        StopCoroutine("PlayButtonFlashAnimation");

    //    for (int i = 0; i < transform.childCount; ++i)
    //    {
    //        if (transform.GetChild(i).gameObject.name == _button.ToString())
    //        {
    //            ButtonFlash aBut = new ButtonFlash(controllerRenderedMaterials[i], _button, true);
    //            buttonFlashList.Add(aBut);
    //            break;
    //        }
    //    }

    //    if (buttonFlashList.Count > 0 && gameObject.activeSelf)
    //        StartCoroutine("PlayButtonFlashAnimation");
    //}

    //[System.Obsolete("Obsolete Method, use: SetControllerButtonNotification(bool _enable, ControllerButton _button)")]
    //public void StopControllerButtonNotification(OvidVRControllerClass.OvidVRControllerButtons _button)
    //{
    //    if (buttonFlashList == null || buttonPressList == null)
    //        return;

    //    if (buttonFlashList.Count > 0 && gameObject.activeSelf)
    //        StopCoroutine("PlayButtonFlashAnimation");

    //    foreach (ButtonFlash button in buttonFlashList)
    //    {
    //        if (button.button == _button)
    //        {
    //            button.buttonMat.SetColor("_LayerColor1", controllerMaterialColor);
    //            button.buttonMat.SetColor("_LayerColor1", new Color(0, 0, 0, 1f));
    //            button.buttonMat.SetColor("_LayerColor2", controllerMaterialColor);
    //            button.buttonMat.SetColor("_LayerColor2", new Color(0, 0, 0, 1f));
    //            button.buttonMat.SetColor("_LayerColor3", controllerMaterialColor);

    //            button.buttonMat.SetColor("_LayerColor0", controllerMaterialColor);

    //            button.buttonMat.SetInt("_LayerBlendMode0", 0);
    //            buttonFlashList.Remove(button);
    //            break;
    //        }
    //    }

    //    foreach (ButtonFlash button in buttonPressList)
    //    {
    //        if (button.button == _button)
    //        {
    //            button.buttonMat.SetColor("_LayerColor1", controllerMaterialColor);
    //            button.buttonMat.SetColor("_LayerColor1", new Color(0, 0, 0, 1f));
    //            button.buttonMat.SetColor("_LayerColor2", controllerMaterialColor);
    //            button.buttonMat.SetColor("_LayerColor2", new Color(0, 0, 0, 1f));
    //            button.buttonMat.SetColor("_LayerColor3", controllerMaterialColor);

    //            button.buttonMat.SetColor("_LayerColor0", controllerMaterialColor);

    //            button.buttonMat.SetInt("_LayerBlendMode0", 0);

    //            buttonPressList.Remove(button);
    //            break;
    //        }
    //    }

    //    if (buttonFlashList.Count > 0 && gameObject.activeSelf)
    //        StartCoroutine("PlayButtonFlashAnimation");
    //}

}
