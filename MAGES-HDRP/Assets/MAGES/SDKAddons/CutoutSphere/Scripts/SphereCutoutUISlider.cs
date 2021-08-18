using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ovidVR.UIManagement;

public class SphereCutoutUISlider : MonoBehaviour
{
    [SerializeField, Range(0f, 1f), Header("Max distance hand from UI to enable the Slider")]
    private float maxHandDistance = 0.15f;

    [SerializeField, Range(0f, 2f), Header("Time intervals to check distance between hands and UI")]
    private float checkHandDistanceInterval = 0.6f;
    private float timer;

    // Translation occurs Y: 0.105 -> -0.164
    private float maxY = -0.164f;
    private float minY = 0.105f;

    // User Given min max values depending on their value offset
    private float userMaxY, userMinY;

    // Image Slider Transform values
    private RectTransform sliderImgTransform;
    private Text sliderPercentText;

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
    private float translationMul = 1.6f;

    private Canvas thisCanvas;
    private Collider buttonCollider;
    private Transform cutoutSphere;
    private Transform buttonTransform;
    private Transform leftController;
    private Transform rightController;

    // UI slider will initially have a small animation explaining what this is and
    // why it disappears. Coroutine waits as long as the animation lasts (manual input)
    private bool isInitialAnimOver = false;

    void Start()
    {
        sliderImgTransform = transform.Find("ButtonSlider").GetComponent<RectTransform>();
        sliderPercentText = sliderImgTransform.GetComponentInChildren<Text>();

        Transform sliderImageMask = transform.Find("ButtonSlider/ButtonImageMask");

        sliderAnim = transform.Find("ButtonSlider").GetComponentInChildren<Animator>();
        isSliderInUse = false;

        thisCanvas = GetComponent<Canvas>();
        buttonCollider = GetComponentInChildren<Collider>();

        cutoutSphere = Resources.FindObjectsOfTypeAll<SphereCutout>()[0].transform;
        buttonTransform = transform.Find("ButtonSlider");
        leftController = OvidVRControllerClass.Get.leftHand.transform;
        rightController = OvidVRControllerClass.Get.rightHand.transform;

        StartCoroutine("InitialAnimationWait");
    }

    IEnumerator InitialAnimationWait()
    {
        yield return new WaitForSeconds(6.5f);

        SetSliderState(false);
        isInitialAnimOver = true;
    }

    void Update()
    {
        if (!isInitialAnimOver)
            return;

        timer += Time.deltaTime;
        if(timer >= checkHandDistanceInterval)
        {
            timer = 0f;
            float leftDist = Vector3.Distance(buttonTransform.position, leftController.position);
            float rightDist = Vector3.Distance(buttonTransform.position, rightController.position);

            if (leftDist <= maxHandDistance || rightDist <= maxHandDistance)
                SetSliderState(true);
            else
                SetSliderState(false);
        }
    }

    void FixedUpdate()
    {
        UpdateSliderPercentage();
        UpdateTriggerState();
        UpdateCutOutSphereScale();
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
            ResetCollisionSlider();
            return;
        }

        float newY = Mathf.Clamp(sliderStartingY + (translationMul * (currHandTransform.position.y - currHandStartingY)), maxY, minY);

        sliderImgTransform.localPosition = new Vector3(sliderImgTransform.localPosition.x, newY, sliderImgTransform.localPosition.z);
    }

    void UpdateCutOutSphereScale()
    {
        //denormalized = (normalized)*(max(x)-min(x))+min(x)
        float scale = GetCurrentSliderValue(true) * (0.4f - 0.1f) + 0.1f;

        cutoutSphere.localScale = new Vector3(scale, scale, scale);
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

    public float GetCurrentSliderValue(bool _normalized = false)
    {
        if (_normalized)
            return percentageNorm;
        else
            return percentage;
    }

    public void SetSliderState(bool _isActive)
    {
        thisCanvas.enabled = _isActive;
        buttonCollider.enabled = _isActive;
    }
}
