using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.GameController;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using ovidVR.UIManagement;
using ovidVR.sceneGraphSpace;

public class UIButton : MonoBehaviour
{
    [Tooltip("Set if the button is enabled by default (disabled button is still visible, it just doesn't 'work')")]
    public bool isActive = true;
    [HideInInspector]
    public bool isPrimaryState = true;

    [Tooltip("If button is affected by a license, select what type of license affects it")]
    public List<UIButtonList.AffectedByLicense> licenseAffectedButton;

    [SerializeField]
    internal LanguageTranslator buttonExplanation;

    [SerializeField, Tooltip("Add a Sprite for the Button's secodanry state (e.g. On / Off). If there is no second state, leave empty")]
    private Sprite secondaryButtonState = null;

    [SerializeField, Tooltip("Set to true in a peramnent text (or image) above the button is required")]
    internal bool allowButtonPermanentExplanation = false;

    [SerializeField, Tooltip("Permanent message displayed above Button. Careful might overlap its surroundings")]
    internal LanguageTranslator permanentButtonText;

    [SerializeField, Tooltip("Permanent image displayed above Button. Careful might overlap its surroundings")]
    internal Sprite permanentButtonImage;

    [SerializeField, Tooltip("Set sound on button press, Set to false if overlaps sounds. eg Next/Previous Action")]
    internal bool allowButtonSound = true;

    // Check from Parent Spawn if these are allowed - ButtonList passes the values
    internal bool allowButtonAnimation = true;

    // Does not allow buttons to be pressed during animation play. If button has animation, it will change to false
    // from parent script UIButtonList, and return to true after animation is finished
    internal bool allowInvoke = true;

    // Store the original button explanation
    private LanguageTranslator defaultButtonExplanation;

    private Image buttonImage;
    private Image disabledButtonImage;
    private Sprite primaryButtonState;

    private GameObject secondaryExplanation;
    private Text secondaryExplanationText;
    private Image secondaryExplanationImage;

    private UIButtonList parentButtonList;

    private OvidVRControllerClass.OvidVRHand currHandCollider;
    private int handCollisionCounter;

    private Animator uiAnim;
    private bool hasInvoked;

    // used from OvidVRControllerClass to know the tag of the collided gameobject
    private string parentTag = "";

    private License.LicenseType licenseType = License.LicenseType.None;

    public UnityEvent buttonInitialization;
    public UnityEvent buttonFunction;

    // Initialization --------------------------------------

    internal void InitializeButton()
    {
        if (buttonFunction == null || transform.parent == null || transform.parent.GetComponent<UIButtonList>() == null)
        {
            Debug.LogError("Parent does NOT include a UISelectionBase " + transform.parent.gameObject.name);
            enabled = false;
            return;
        }

        licenseType = sceneGraph.GetLicenseType();

        defaultButtonExplanation = buttonExplanation;

        buttonImage = transform.Find("ButtonImageMask/ButtonImage").GetComponent<Image>();
        disabledButtonImage = transform.Find("ButtonImageMask/DisabledButtonImage").GetComponent<Image>();
        primaryButtonState = buttonImage.sprite;

        secondaryExplanation = transform.Find("SecondaryExplanation").gameObject;
        secondaryExplanationText = secondaryExplanation.GetComponentInChildren<Text>();
        secondaryExplanationImage = secondaryExplanation.transform.Find("ImageOptional").GetComponent<Image>();

#if UNITY_EDITOR
        // no need to disable button functionality in editor mode
        licenseAffectedButton = new List<UIButtonList.AffectedByLicense>();
        licenseAffectedButton.Add(UIButtonList.AffectedByLicense.None);
#endif

        // if none license is given the defualt will be None
        if (licenseAffectedButton.Count == 0)
            licenseAffectedButton.Add(UIButtonList.AffectedByLicense.None);

        if(transform.parent && transform.parent.GetComponent<UIButtonList>())
            parentButtonList = transform.parent.GetComponent<UIButtonList>();

        handCollisionCounter = 0;
        hasInvoked = false;
        parentTag = "";

        // A button can have more than one animator. The button behavior lies in the ButtonImageMask Animator
        Animator[] allBtnAnim = GetComponentsInChildren<Animator>();
        foreach(Animator a in allBtnAnim)
        {
            if(a.gameObject.name == "ButtonImageMask")
            {
                uiAnim = a;
                break;
            }
        }
        if (uiAnim == null)
            Debug.LogError("Problem with Button Animators, Please Fix");

        SetButtonActivity(isActive);

        if (buttonInitialization != null)
            buttonInitialization.Invoke();
    }

    internal void InitializeAfterButtonAnimation()
    {     
        if (allowButtonPermanentExplanation)
            SetButtonSecondaryExplanation(true, permanentButtonText, permanentButtonImage);

        // The button collider and the UI expanding collider (if exists) should not observe one another
        if (transform.parent.parent != null && transform.parent.parent.Find("ExpandingCollider"))
        {
            Collider expCollider = transform.parent.parent.Find("ExpandingCollider").GetComponent<Collider>();
            if (expCollider != null)
                Physics.IgnoreCollision(GetComponent<Collider>(), expCollider);
        }

        // since animation is finished, buttons are allowed to be triggered
        allowInvoke = true;
    }

    private void OnDisable()
    {
        ResetButton();
    }

    // Collision Triggers ----------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("UserHands"))
        {
            if (handCollisionCounter == 0)
            {
                // save what hand is hovering the button, to flash its own finger
                parentTag = OvidVRControllerClass.Get.GetHandTag(other.gameObject);

                if (parentTag.Equals("RightHand") || parentTag.Equals("RightPalm"))
                    currHandCollider = OvidVRControllerClass.OvidVRHand.right;
                else if (parentTag.Equals("LeftHand") || parentTag.Equals("LeftPalm"))
                    currHandCollider = OvidVRControllerClass.OvidVRHand.left;

                if (parentButtonList != null)
                    parentButtonList.SetExplanationText(buttonExplanation, true, licenseAffectedButton);

                if(allowButtonAnimation)
                    uiAnim.SetBool("ButtonHover", true);

                if (string.Equals(parentTag, "UnTagged", StringComparison.OrdinalIgnoreCase))
                    return;

                // Update Hand flash animation
                OvidVRControllerClass.DeviceController.SetControllerState(currHandCollider, true);
                OvidVRControllerClass.DeviceController.SetButtonFlashing(true, currHandCollider, OvidVRControllerClass.OvidVRControllerButtons.TriggerButton);
            }

            ++handCollisionCounter;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (string.Equals(parentTag, "UnTagged", StringComparison.OrdinalIgnoreCase))
            return;

        if (handCollisionCounter > 0 && allowInvoke)
        {
            if (OvidVRControllerClass.DeviceController.GetTriggerStrength(currHandCollider) > 0.6f || other.gameObject.name.Equals("HandCollider"))
            {
                if (hasInvoked)
                    return;

                hasInvoked = true;

                OvidVRControllerClass.DeviceController.ControllerHapticPulse(currHandCollider, 1f);

                if (!isActive)
                    return;

                if (uiAnim != null && allowButtonAnimation)
                {
                    if (uiAnim.GetBool("ButtonPress"))
                        return;
                    uiAnim.SetBool("ButtonPress", true);
                }

                if (allowButtonSound)
                    UIManagement.PlaySound(UISounds.ButtonPress);

                if (buttonFunction != null)
                    buttonFunction.Invoke();
            }
            else
            {
                if (hasInvoked)
                    hasInvoked = false;

                OvidVRControllerClass.DeviceController.ControllerHapticPulse(currHandCollider, 0.2f);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("UserHands"))
        {
            --handCollisionCounter;

            if (handCollisionCounter <= 0)
                ResetButton();
        }
    }

    // Public Functions ------------------------------------

    /// <summary>
    /// Set the buttons state. In case the button is a toggle switch, 
    /// this function changes its state from the primary to the secondary and back
    /// </summary>
    public void SetButtonPrimarySecondaryState(bool _isPrimaryState)
    {
        if (secondaryButtonState == null)
        {
            Debug.LogError("A button with a Secondary State has NO Sprite added for that state");
            return;
        }

        isPrimaryState = _isPrimaryState;
        if (_isPrimaryState)
            buttonImage.sprite = primaryButtonState;
        else
            buttonImage.sprite = secondaryButtonState;
    }

    /// <summary>
    /// Enables / Disables the button. NOT as a gameobject. Just for the user to appear
    /// as enabled or disabled. If Disabled new Image Color is Added, function invoke
    /// doesn't work and the press button animation doesn't play
    /// 
    /// if button is license affected, discard changes
    /// 
    /// +1 overload
    /// </summary>
    /// <param name="_active">set button activity</param>
    /// <param name="_newExplanatoryText">Set a different text than the default ones</param>
    public void SetButtonActivity(bool _active)
    {
        foreach (UIButtonList.AffectedByLicense abl in licenseAffectedButton)
        {
            switch (abl)
            {
                case UIButtonList.AffectedByLicense.Demo:
                    if (licenseType == License.LicenseType.Demo)
                    {
                        if(buttonFunction != null)
                            buttonFunction.RemoveAllListeners();
                        buttonFunction = null;
                        disabledButtonImage.enabled = true;
                        isActive = false;
                        return;
                    }
                    break;
                case UIButtonList.AffectedByLicense.Free:
                    if (licenseType == License.LicenseType.Free)
                    {
                        if (buttonFunction != null)
                            buttonFunction.RemoveAllListeners();
                        buttonFunction = null;
                        disabledButtonImage.enabled = true;
                        isActive = false;
                        return;
                    }
                    break;
            }
        }

        disabledButtonImage.enabled = !_active;
        isActive = _active;

        if (_active)
            buttonExplanation = defaultButtonExplanation;
        else
            buttonExplanation = LanguageTranslator.DisabledBtnButExp;
    }

    /// <summary>
    /// Enables / Disables the button. NOT as a gameobject. Just for the user to appear
    /// as enabled or disabled. If Disabled new Image Color is Added, function invoke
    /// doesn't work and the press button animation doesn't play
    /// 
    /// if button is license affected, discard changes
    /// </summary>
    /// <param name="_active">set button activity</param>
    /// <param name="_newExplanatoryText">Set a different text than the default ones</param>
    public void SetButtonActivity(bool _active, LanguageTranslator _newExplanatoryText)
    {
        SetButtonActivity(_active);

        buttonExplanation = _newExplanatoryText;
    }

    /// <summary>
    /// Enables disables the secondary - permanent explanation 
    /// 
    /// +2 overloads
    /// </summary>
    /// <param name="_active"></param>
    public void SetButtonSecondaryExplanation(bool _active)
    {
        secondaryExplanation.SetActive(_active);
    }

    /// <summary>
    /// If true it spawns above the button the small extra text area displaying the message given
    /// </summary>
    /// <param name="_active"></param>
    /// <param name="_image"></param>
    /// <param name="_message"></param>
    public void SetButtonSecondaryExplanation(bool _active, LanguageTranslator _message, Sprite _image = null)
    {
        secondaryExplanation.SetActive(_active);

        if (_active)
        {
            if (_image != null)
            {              
                secondaryExplanationImage.enabled = true;

                permanentButtonImage = _image;
                secondaryExplanationImage.sprite = _image;
            }
            else
                secondaryExplanationImage.enabled = false;

            permanentButtonText = _message;

            secondaryExplanationText.text = UIManagement.GetUIMessage(permanentButtonText);
        }
    }

    /// <summary>
    /// If true it spawns above the button the small extra text area displaying the message given.
    /// the message is given directly as a string and will NOT be translated
    /// 
    /// This is only available on Secondary Explanation (direclty given string messages)
    /// </summary>
    /// <param name="_active"></param>
    /// <param name="_message"></param>
    /// <param name="_image"></param>
    public void SetButtonSecondaryExplanation(bool _active, string _message, Sprite _image = null)
    {
        secondaryExplanation.SetActive(_active);

        if (_active)
        {
            if (_image != null)
            {
                secondaryExplanationImage.enabled = true;

                permanentButtonImage = _image;
                secondaryExplanationImage.sprite = _image;
            }
            else
                secondaryExplanationImage.enabled = false;

            secondaryExplanationText.text = _message;
        }
    }


    /// <summary>
    /// Resets button collision kept values (gameobject, layer, animation, counter)
    /// </summary>
    public void ResetButton()
    {
        if(parentButtonList != null)
            parentButtonList.SetExplanationText(buttonExplanation, false, licenseAffectedButton);

        if (allowButtonAnimation)
        {
            uiAnim.SetBool("ButtonHover", false);
            uiAnim.SetBool("ButtonPress", false);
        }

        if(OvidVRControllerClass.DeviceController != null)
        {
            OvidVRControllerClass.DeviceController.SetButtonFlashing(false, OvidVRControllerClass.OvidVRHand.left, OvidVRControllerClass.OvidVRControllerButtons.TriggerButton);
            OvidVRControllerClass.DeviceController.SetButtonFlashing(false, OvidVRControllerClass.OvidVRHand.right, OvidVRControllerClass.OvidVRControllerButtons.TriggerButton); OvidVRControllerClass.DeviceController.SetButtonFlashing(true, currHandCollider, OvidVRControllerClass.OvidVRControllerButtons.TriggerButton);
            OvidVRControllerClass.DeviceController.SetControllerState(currHandCollider, false);
        }
        
        hasInvoked = false;

        parentTag = "";
        handCollisionCounter = 0;
    }

    // GETTERS ----------------------------------------

    public string GetButtonExplanation()
    {
        return UIManagement.GetUIMessage(buttonExplanation);
    }

    public string GetButtonPermanentSecondaryExplanation()
    {
        return secondaryExplanationText.text;
    }
}
