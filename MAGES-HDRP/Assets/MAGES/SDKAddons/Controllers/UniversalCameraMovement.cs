using ovidVR.GameController;
using UnityEngine;
using ovidVR.UIManagement;
using static ovidVR.GameController.OvidVRControllerClass;

public class UniversalCameraMovement : MonoBehaviour
{
    [Range(0.5f, 2f), Tooltip("Set Translation Speed")]
    public float translationSpeed = 1.2f;

    public bool allowTranslation = true;
    public bool allowRotation = true;
    public bool allowVROptions = true;

    private CharacterController controller;

    private bool toggleTranslation = false;
    private bool toggleRotation = false;
    private bool toggleVROptions = false;

    private GameObject vrOptions, vrNotificationOptions;
    private Transform head;

    [Range(1, 10)]
    private int quartileSteps = 4;


    private void Awake()
    {
        head = transform.Find("head");

        InitializeMovement();
    }

    private void Update()
    {
        CameraTranslation();

        VROptions();
    }

    /// <summary>
    /// This function is called at the end of Start() in ViveController
    /// </summary>
    public void InitializeMovement()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("No CharacterController found");
            this.enabled = false;
        }

        controller.detectCollisions = true;
    }

    private void CameraTranslation()
    {
        if (!allowTranslation) return;

        if (!toggleTranslation && OvidVRControllerClass.DeviceController.GetButtonPressed(OvidVRHand.left, OvidVRControllerButtons.ThumbStick))
        {
            toggleTranslation = true;
        }
        else if (toggleTranslation && OvidVRControllerClass.DeviceController.GetButtonPressed(OvidVRHand.left, OvidVRControllerButtons.ThumbStick))
        {
            toggleTranslation = false;
        }

        if (toggleTranslation) return;

        // Body Movement
        Vector3 moveDirection;
        Vector3 front = head.forward;
        Vector3 right = Vector3.Cross(front, Vector3.up);

        //Maintain movement on a single plane
        front.y = 0;
        right.y = 0;

        //Up-Down
        Vector3 upvector = new Vector3();
        float height = OvidVRControllerClass.DeviceController.GetThumpPosOnController(OvidVRHand.right).y;

        //Threshold
        if (height < 0.3f && height > -0.3)
            height = 0f;

        upvector.y += translationSpeed * 0.7f * height * Time.deltaTime;

        moveDirection = front * (translationSpeed * OvidVRControllerClass.DeviceController.GetThumpPosOnController(OvidVRHand.left).y * Time.deltaTime) +
                        right * (translationSpeed * -OvidVRControllerClass.DeviceController.GetThumpPosOnController(OvidVRHand.left).x * Time.deltaTime) +
                        upvector;

        controller.Move(moveDirection);

        //Rotation
        CameraRotation();
    }

    private void CameraRotation()
    {

        if (!allowRotation) return;

        //Fade out

        float rotationValue = OvidVRControllerClass.DeviceController.GetThumpPosOnController(OvidVRHand.right).x;

        if (!toggleRotation && rotationValue > 0.5f)
        {
            toggleRotation = true;
            transform.Rotate(new Vector3(0, 1, 0), (90f / (float)quartileSteps), Space.Self);
        }
        else if (!toggleRotation && rotationValue < -0.5f)
        {
            toggleRotation = true;
            transform.Rotate(new Vector3(0, 1, 0), -(90f / (float)quartileSteps), Space.Self);
        }
        else if(rotationValue > -0.5f && rotationValue < 0.5f)
        {
            toggleRotation = false;
        }

        //Fade in
    }

    private void VROptions()
    {

        if (!allowVROptions) return;

        if(!toggleVROptions && OvidVRControllerClass.DeviceController.GetButtonPressed(OvidVRHand.right, OvidVRControllerButtons.ThumbStick)){
            if (vrOptions == null || vrOptions.GetComponent<InteractiveInterfaceSpawnAnimation>().GetIsInterfaceCurrentlyDestroyed())
            {
                if (!InterfaceManagement.Get.GetUserSpawnedUIAllowance())
                    return;

#if UNITY_ANDROID
            vrOptions = InterfaceManagement.Get.SpawnUI("OptionsMobile");
#elif UNITY_STANDALONE_WIN
                vrOptions = InterfaceManagement.Get.SpawnUI("Options");
#else
            vrOptions = InterfaceManagement.Get.SpawnUI("Options");
#endif

                float preserveX = vrOptions.transform.rotation.x;
                float preserveZ = vrOptions.transform.rotation.z;
                float preserveHeight = vrOptions.transform.position.y + 1;

                InterfaceManagement.Get.FacingUserInterface(vrOptions, false);

                Quaternion newRot = vrOptions.transform.rotation;
                newRot.x = preserveX;
                newRot.z = preserveZ;
                vrOptions.transform.rotation = newRot;

                Vector3 newPos = vrOptions.transform.position;
                newPos.y = preserveHeight;
                vrOptions.transform.position = newPos;

                vrNotificationOptions = InterfaceManagement.Get.SpawnExtraExplanationNotification(InterfaceManagement.Get.GetUIMessage(LanguageTranslator.OptionsTitle), OvidVRControllerClass.Get.rightHand.transform, true);
                InterfaceManagement.Get.FollowHandInterface(vrNotificationOptions, false);
                vrNotificationOptions.transform.Translate(-0.6f * Vector3.right, Space.Self);

                toggleVROptions = true;
            }
        }
        else if (toggleVROptions && OvidVRControllerClass.DeviceController.GetButtonPressed(OvidVRHand.right, OvidVRControllerButtons.ThumbStick))
        {
            if (vrOptions != null)
            {
                vrOptions.GetComponent<InteractiveInterfaceSpawnAnimation>().DestroyInterface();
                vrNotificationOptions.GetComponent<ExtraExpNotificationUI>().DestroyThis();

                vrOptions = null;

                toggleVROptions = false;
            }
        }

    }
}