using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ovidVR.UIManagement;

public class UIAnimSlider : MonoBehaviour {

    // when user leaves the button in the desired position
    private bool isSelectedValueFinalized = false; 

    // Translation occurs Y: 0.105 -> -0.164
    private float maxY = -0.164f;
    private float minY = 0.105f;

    // User Given min max values depending on their value offset
    private float userMaxY, userMinY;

    // Within this range the UI will be marked as completed (user can change them)
    private float minAccRange = 0.95f;
    private float maxAccRange = 1f;

    // Arrow Path Animation values
    private List<Image> pathArrows;
    private int listIndex = -1;
    private float colorChangeInterval;

    // Image Slider Transform values
    private RectTransform sliderImgTransform;
    private Text sliderPercentText;

    // Image Slider mask contains as childern the default , correct and wrong image
    // depending where the user leaves the slider (0: defalt, 1:correct, 2:wrong)
    private List<Image> sliderImageStates;

    // Store header text
    private Text headerText;

    private float percentage, percentageNorm;

    // Slider collision, animation & transform values
    private Animator sliderAnim;
    private OvidVRControllerClass.OvidVRHand currHandCollider;
    private Transform currHandTransform;
    private float currHandStartingY;
    private float sliderStartingY;
    private int handCollisionCounter;
    public bool isSliderInUse;

    // multiplier for translation to help "lesser" DOF modes
    private float translationMul = 1f;

    void Start () {
        Transform allpathArrowTrs = transform.Find("ButtonPath");
        pathArrows = new List<Image>();
        foreach(Transform t in allpathArrowTrs)
            pathArrows.Add(t.GetComponent<Image>());

        sliderImgTransform = transform.Find("ButtonSlider").GetComponent<RectTransform>();
        sliderPercentText = sliderImgTransform.GetComponentInChildren<Text>();

        Transform sliderImageMask = transform.Find("ButtonSlider/ButtonImageMask");
        sliderImageStates = new List<Image>();
        // second and third image must be disabled
        int index = 0;
        foreach(Transform t in sliderImageMask)
        {    
            sliderImageStates.Add(t.GetComponent<Image>());
            if (index > 0)
                t.GetComponent<Image>().enabled = false;
            ++index;
        }

        headerText = transform.Find("Header").GetComponentInChildren<Text>();

        sliderAnim = transform.Find("ButtonSlider").GetComponentInChildren<Animator>();
        isSliderInUse = false;

        if (OvidVRControllerClass.Get.DOF != ControllerDOF.SixDOF)
            translationMul = 1.6f;
    }
	
	void FixedUpdate () {       
        UpdateSliderPercentage();
        ArrowsColorAnimation();
        UpdateTriggerState();
    }

    void ArrowsColorAnimation()
    { 
        colorChangeInterval += Time.deltaTime;

        if(colorChangeInterval >= 0.06f)
        {
            colorChangeInterval = 0f;

            // First: disable all arrows that the slider has passed through them
            //        slider pos normalize 0 -> 10, per value disable 2 arrows
            int sliderPosNorm = (int)(10f * percentageNorm) * 2;

            if (sliderPosNorm > pathArrows.Count)
                sliderPosNorm = pathArrows.Count;

            for (int i = 0; i < pathArrows.Count; ++i)
            {
                if (i <= sliderPosNorm)
                    pathArrows[i].gameObject.SetActive(false);
                else
                    pathArrows[i].gameObject.SetActive(true);
            }

            // Secondl: update List Index (Circle List)
            ++listIndex;
            listIndex %= pathArrows.Count;

            // Third: reset all arrows back to white
            foreach (Image i in pathArrows)
                i.color = Color.white;

            // Fourth: Add black and grey to the animated at this frame arrows
            if (listIndex > 0)
                pathArrows[listIndex - 1].color = new Color32(56, 56, 56, 255);
            if(listIndex < pathArrows.Count - 1)
                pathArrows[listIndex + 1].color = new Color32(56, 56, 56, 255);

            pathArrows[listIndex].color = Color.black;
        }
    }

    void UpdateSliderPercentage()
    {
        // normalization between 0 -> 1 of where the slider is (it's own Y values)
        percentageNorm = (sliderImgTransform.localPosition.y - minY) / (maxY - minY);
        // this is reversed normalization (0 -> 1 => minGiven -> maxGiven)
        percentage = percentageNorm * (userMaxY - userMinY) + userMinY;

        sliderPercentText.text = ((int)(100f * percentageNorm)).ToString() + "%";
    }

    void UpdateTriggerState()
    {
        if (!isSliderInUse)
            return;

        isSliderInUse = GetIsTriggerPressed(currHandCollider);
        if (!isSliderInUse)
        {
            SetSliderImageState();
            ResetCollisionSlider();
            return;
        }

        float newY = Mathf.Clamp(sliderStartingY + (translationMul * (currHandTransform.position.y - currHandStartingY)), maxY, minY);

        sliderImgTransform.localPosition = new Vector3(sliderImgTransform.localPosition.x,  newY, sliderImgTransform.localPosition.z);       
    }

    void SetSliderImageState(bool _reset = false)
    {
        if(_reset)
        {
            isSelectedValueFinalized = false;

            sliderImageStates[0].enabled = true;
            sliderImageStates[1].enabled = false;
            sliderImageStates[2].enabled = false;

            return;
        }

        isSelectedValueFinalized = true;

        if (percentageNorm >= minAccRange && percentageNorm <= maxAccRange)
        {
            sliderImageStates[0].enabled = false;
            sliderImageStates[1].enabled = true;
            sliderImageStates[2].enabled = false;
        }
        else
        {
            sliderImageStates[0].enabled = false;
            sliderImageStates[1].enabled = false;
            sliderImageStates[2].enabled = true;
        }
    }

    // Collision Detection ---------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("UserHands"))
        {
            if (handCollisionCounter == 0)
            {
                // save what hand is hovering the button, to flash its own finger
                 string parentTag = OvidVRControllerClass.Get.GetHandTag(other.gameObject);

                if (parentTag.Equals("RightHand") || parentTag.Equals("RightPalm"))
                {
                    currHandCollider = OvidVRControllerClass.OvidVRHand.right;
                    currHandTransform = OvidVRControllerClass.Get.rightHand.transform;
                }
                else if (parentTag.Equals("LeftHand") || parentTag.Equals("LeftPalm"))
                {
                    currHandCollider = OvidVRControllerClass.OvidVRHand.left;
                    currHandTransform = OvidVRControllerClass.Get.leftHand.transform;
                }  

                sliderAnim.SetBool("ButtonHover", true);

                if (string.Equals(parentTag, "UnTagged", System.StringComparison.OrdinalIgnoreCase))
                    return;

                // Update Hand flash animation
                if (OvidVRControllerClass.DeviceController != null)
                {
                    OvidVRControllerClass.DeviceController.SetControllerState(currHandCollider, true);
                    OvidVRControllerClass.DeviceController.SetButtonFlashing(true, currHandCollider, OvidVRControllerClass.OvidVRControllerButtons.TriggerButton);
                }                   
            }

            ++handCollisionCounter;
        }
    }

    private void OnTriggerStay()
    {
        if (handCollisionCounter <= 0)
            return;

        // Start observing the Y changes in hand
        if (GetIsTriggerPressed(currHandCollider))
        {
            SetSliderImageState(true);
            isSliderInUse = true;

            currHandStartingY = currHandTransform.position.y;
            sliderStartingY = sliderImgTransform.localPosition.y;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("UserHands"))
        {
            --handCollisionCounter;

            if (handCollisionCounter <= 0)
                ResetCollisionSlider();
        }
    }

    private void ResetCollisionSlider()
    {
        handCollisionCounter = 0;

        if (OvidVRControllerClass.DeviceController != null)
        {
            OvidVRControllerClass.DeviceController.SetButtonFlashing(false, OvidVRControllerClass.OvidVRHand.left, OvidVRControllerClass.OvidVRControllerButtons.TriggerButton);
            OvidVRControllerClass.DeviceController.SetButtonFlashing(false, OvidVRControllerClass.OvidVRHand.right, OvidVRControllerClass.OvidVRControllerButtons.TriggerButton); OvidVRControllerClass.DeviceController.SetButtonFlashing(true, currHandCollider, OvidVRControllerClass.OvidVRControllerButtons.TriggerButton);
            OvidVRControllerClass.DeviceController.SetControllerState(currHandCollider, false);
        }

        if (!isSliderInUse)
        {
            sliderAnim.SetBool("ButtonHover", false);
            currHandTransform = null;
            currHandStartingY = 0f;
        }
    }

    private bool GetIsTriggerPressed(OvidVRControllerClass.OvidVRHand _hand)
    {
        if (OvidVRControllerClass.DeviceController.GetControllerGrabStrength(_hand) > 0.5f)
            return true;

        return false;
    }

    // Public Values ---------------------------------------------

    /// <summary>
    /// It return where the slider is between the given min and max values
    /// </summary>
    /// <param name="_normalized">if true, it returns the normalized value (0->1)</param>
    /// <returns></returns>
    public float GetCurrentSliderValue(bool _normalized = false)
    {
        if (_normalized)
            return percentageNorm;
        else
            return percentage;
    }

    /// <summary>
    /// It returns false as long as the user is using the slider
    /// and true if the slider is left to a specific position. Only if it is true
    /// the value is considered as selected
    /// </summary>
    /// <returns></returns>
    public bool GetIsSliderValueFinalized()
    {
        return isSelectedValueFinalized;
    }

    /// <summary>
    /// Set the minimum and maximum values that are going to be mapped
    /// as 0 and 1 for the normalized slider
    /// </summary>
    /// <param name="_minVal"></param>
    /// <param name="_maxVal"></param>
    public void SetSliderValueRange(float _minVal, float _maxVal)
    {
        userMinY = _minVal;
        userMaxY = _maxVal;
    }

    /// <summary>
    /// Set the range where is the slider is left it will be acceptable
    /// If _normalizedRange is TRUE, the values must be between 0 -> 1
    /// If _normalizedRange is FALSE, the values should be between the min and max value ranges
    /// 
    /// By defalut the acceptable range is [0.95 , 1]
    /// </summary>
    /// <param name="_minRange"></param>
    /// <param name="_maxRange"></param>
    /// <param name="_normalizedRange"></param>
    public void SetNormalizedAcceptableRange(float _minRange, float _maxRange, bool _normalizedRange)
    {
        if (_normalizedRange)
        {
            minAccRange = _minRange;
            maxAccRange = _maxRange;
        }
        else
        {
            minAccRange = (_minRange - userMinY) / (userMaxY - userMinY);
            maxAccRange = (_maxRange - userMinY) / (userMaxY - userMinY);
        }
    }

    /// <summary>
    /// Set the message that it will be displayed on the UIs header
    /// </summary>
    /// <param name="_message"></param>
    public void SetHeaderMessage(LanguageTranslator _message)
    {
        string messageStr = UIManagement.GetUIMessage(_message);

        headerText.text = messageStr;
    }

    /// <summary>
    /// Updated the position of the slider manually
    /// slider can change only its Y values
    /// </summary>
    /// <param name="_y"></param>
    /// <param name="isNormalized"></param>
    public void UpdateManuallySliderHeight(float _y, bool isNormalized = true)
    {
        float newY;

        if (isNormalized)
            newY = Mathf.Clamp(_y, 0, 1) * (maxY - minY) + minY;
        else
            newY = Mathf.Clamp(_y, minY, maxY);

        sliderImgTransform.localPosition = new Vector3(sliderImgTransform.localPosition.x, newY, sliderImgTransform.localPosition.z);
    }
}
