using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;


/// <summary>
/// Point And Click Controller
///
/// SETUP
///     - Replace the current camera prefab with one in this folder
///     - Replace the current DeviceController in SCENE_MANAGEMENT/OvidVRDeviceController with
///       PointAndClickController
/// </summary>
public class WaitForFrames : CustomYieldInstruction
{
    private int targetCount;

    public WaitForFrames(int frameCount)
    {
        targetCount = Time.frameCount + frameCount;
    }

    public override bool keepWaiting
    {
        get
        {
            return Time.frameCount < targetCount;
        }
    }
}

public class PointAndClickCameraController : MonoBehaviour
{
    public static PointAndClickCameraController Instance { get; private set; } = null;

    [Tooltip("Is the user allowed to move?")]
    public bool freeMove = false;
    [Tooltip("Is the user allowed to rotate the camera?")]
    public bool freeRotate = true;
    [Tooltip("Lock the mouse always")]
    public bool lockAlways = true;

    public float mouseSpeed = 1.0f;
    public Vector2 constraintYaw = new Vector2(0, 360);
    public Vector2 constraintPitch = new Vector2(0, 360);

    public float mouseHoldDurationTheshold = 1.0f;
    private float mouseHoldDuration = 0.0f;

    private float yaw = 0.0f, pitch = 0.0f;
    private Quaternion previousOrientation;
    private Quaternion orientation;

    private bool mouseIsLocked = false;
    private Camera pointCamera;

    private GameObject raycast;
    private GameObject raycastEndCollider;
    private DrawRay drawRay;

    public bool Trigger { get; private set; } = false;

    [Tooltip("A GameObject with the Gotos script attached to it")]
    public GameObject gotoPrefab = null;

    [HideInInspector]
    public RawImage cursorImage = null;
    [HideInInspector]
    public Canvas cursorCanvas = null;

    public void SetCursor(bool enabled)
    {
        transform.Find("Canvas/RawImage").gameObject.SetActive(enabled);
        Cursor.visible = !enabled;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        Instance = this;
        XRSettings.enabled = false;

        transform.parent.parent.parent.gameObject.name = "VRCamera";


        if (!freeMove && !freeRotate)   // Goto Mode
        {
            var gotoList = gotoPrefab;

            gotoInstances = null;
            if (gotoList != null && gotoList.GetComponent<Gotos>() != null)
            {
                GameObject[] gotoPrefabs = gotoList.GetComponent<Gotos>()?.gotoPrefabs;

                if (gotoPrefabs == null)
                {
                    Debug.LogError("Goto prefabs are null!");
                }
                else
                {
                    gotoInstances = new GameObject[gotoPrefabs.Length];

                    for (int i = 0; i < gotoPrefabs.Length; ++i)
                    {
                        GameObject prefab = gotoPrefabs[i];
                        var instance = Instantiate(prefab);

                        instance.name = prefab.name;
                        gotoInstances[i] = instance;

                    }

                }
            }
        }

        // var gotoList = Resources.Load("PointAndClick/Gotos") as GameObject;
        cursorImage = transform.Find("Canvas/RawImage").GetComponent<RawImage>();
        cursorCanvas = transform.Find("Canvas").GetComponent<Canvas>();


        // SetCursor(true);
    }

    // Goto location smoothing
    bool gotoInProgress = false;
    private Vector3 startLocation = Vector3.zero;
    private Vector3 endLocation = Vector3.zero;

    private Quaternion startRotation = Quaternion.identity;
    private Quaternion endRotation = Quaternion.identity;

    private float gotoTimer = 0.0f;
    public float GotoSmoothDuration = 0.35f;

    [HideInInspector]
    public GameObject[] gotoInstances;
    // Start is called before the first frame update
    void Start()
    {
        orientation = transform.rotation;
        var euler = orientation.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;
        pointCamera = GetComponent<Camera>();
        raycast = GetComponentInChildren<DrawRay>().gameObject;

        raycastEndCollider = new GameObject();
        var sphere = raycastEndCollider.AddComponent<SphereCollider>();
        sphere.radius = 0.25f;
        sphere.isTrigger = true;
        raycastEndCollider.transform.parent = transform;

        var renderers = OvidVRControllerClass.Get.rightHand.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers) renderer.enabled = false;

        renderers = OvidVRControllerClass.Get.leftHand.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers) renderer.enabled = false;

        drawRay = raycast.GetComponent<DrawRay>();

        if (gotoInstances != null) StartCoroutine(SetLocation(gotoInstances[0].GetComponent<GotoLocation>()));
        StartCoroutine(DisableVRHands());
    }

    IEnumerator DisableVRHands()
    {
        yield return null; // Wait one frame, so Awake and Start methods get invoked
        GameObject.Find("UserHands").SetActive(false);
    }

    public GotoLocation GetGotoByName(string name)
    {
        foreach (var instance in gotoInstances)
        {
            if (instance.name == name)
            {
                return instance.GetComponent<GotoLocation>();
            }
        }
        return null;
    }

    public void GotoLocationWithName(string name, bool immediate = false)
    {
        foreach (var go in gotoInstances)
        {
            if (name == go.name)
            {
                StartCoroutine(SetLocation(go.GetComponent<GotoLocation>(), immediate));
                break;
            }
        }
    }

    IEnumerator SetLocation(GotoLocation location, bool immediate = false)
    {
        yield return new WaitForFrames(3);

        if (immediate)
        {
            location.GotoImmediate();
        }
        else
        {
            location.OnButtonPress();
        }
    }

    private Vector3 Position
    {
        get
        {
            //return transform.position;
            // [CameraRig] = transform.parent(head).parent(Rig)
            return transform.parent.parent.position;
        }
        set
        {
            //transform.position = value;
            transform.parent.parent.position = value;
        }
    }

    private Quaternion Rotation
    {
        get
        {
            return transform.parent.parent.rotation;
        }

        set
        {
            transform.parent.parent.rotation = value;
        }
    }

    private bool CanRotate()
    {
        if (!freeRotate) return false;      // Rotation isn't available
        if (lockAlways) return true;        // We're always rotating
        return Input.GetMouseButton(1);     // We only rotate with right mouse button
    }

    private void Update()
    {
        bool mouseShouldBeLocked = false;
        if (CanRotate())
        {
            mouseShouldBeLocked = true;
            yaw   += Input.GetAxisRaw("Mouse X") * mouseSpeed;
            pitch -= Input.GetAxisRaw("Mouse Y") * mouseSpeed;


            if (constraintYaw.x == 0.0f && constraintYaw.y == 360.0f)
            {
                if (yaw < 0.0f) yaw += 360.0f;
                if (yaw > 360.0f) yaw -= 360.0f;
            }
            yaw = Mathf.Clamp(yaw, constraintYaw.x, constraintYaw.y);

            if (constraintPitch.x == 0.0f && constraintPitch.y == 360.0f)
            {
                if (pitch < 0.0f) pitch += 360.0f;
                if (pitch > 360.0f) pitch -= 360.0f;
            }
            pitch = Mathf.Clamp(pitch, constraintPitch.x, constraintPitch.y);


            previousOrientation = transform.rotation;
            orientation = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);
        }
        
        if (freeRotate)
        {
            if (mouseIsLocked != mouseShouldBeLocked) ToggleMouseLock();

            transform.rotation = Quaternion.Slerp(previousOrientation, orientation, 1.0f / 3.0f);
        }


        var ray = pointCamera.ScreenPointToRay(Input.mousePosition);
        raycast.transform.LookAt(ray.GetPoint(100.0f));


        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(cursorCanvas.transform as RectTransform, Input.mousePosition, cursorCanvas.worldCamera, out pos);
        cursorImage.transform.position = cursorCanvas.transform.TransformPoint(pos);

        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(Click());
        }

        if (gotoInProgress)
        {
            gotoTimer += Time.deltaTime;
            if (gotoTimer >= GotoSmoothDuration)
            {
                gotoTimer = 0.0f;
                Position = endLocation;
                Rotation = endRotation;
                gotoInProgress = false;
            }
            else
            {

                Vector3 newLocation = new Vector3();
                newLocation.x = Mathf.SmoothStep(startLocation.x, endLocation.x, gotoTimer / GotoSmoothDuration);
                newLocation.y = Mathf.SmoothStep(startLocation.y, endLocation.y, gotoTimer / GotoSmoothDuration);
                newLocation.z = Mathf.SmoothStep(startLocation.z, endLocation.z, gotoTimer / GotoSmoothDuration);
                Position = newLocation;

                Quaternion newRotation = Quaternion.Slerp(startRotation, endRotation, gotoTimer / GotoSmoothDuration);
                Rotation = newRotation;
            }
        }

        if (freeMove)
        {

            Vector3? teleportTarget = null;
            Collider[] overlapColliders = Physics.OverlapSphere(drawRay.RaycastEnd.transform.position, 0.25f);
            if (overlapColliders != null && overlapColliders.Length != 0)
            {
                foreach (var collider in overlapColliders)
                {
                    if (collider.gameObject.name.ToLower().Contains("floor"))
                    {
                        Debug.DrawLine(drawRay.RaycastEnd.transform.position, drawRay.RaycastEnd.transform.position + Vector3.up * 5, Color.green);
                        teleportTarget = drawRay.RaycastEnd.transform.position;
                    }
                }
            }

            if (Input.GetMouseButton(0))
            {
                mouseHoldDuration += Time.deltaTime;
            }
            else
            {
                mouseHoldDuration = 0.0f;
            }

            if (teleportTarget.HasValue)
            {
                if (mouseHoldDuration > mouseHoldDurationTheshold)
                {
                    mouseHoldDuration = 0.0f;
                    Vector3 resultPosition = teleportTarget.Value;
                    resultPosition.y = gameObject.transform.position.y;
                    gameObject.transform.position = resultPosition;
                    mouseHoldDuration = 0.0f;
                }
            }
        }

    }

    public void GotoLocation(Vector3 location, Quaternion rotation, bool noInterpolation)
    {
        gotoInProgress = true;
        startRotation = Rotation;
        endRotation = rotation;

        startLocation = Position;
        endLocation = location;

        if (noInterpolation)
        {
            gotoInProgress = false;
            Position = endLocation;
            Rotation = rotation;
        }

        // constraintYaw = yawConstraint;
        // constraintPitch = pitchConstraint;
    }

    internal IEnumerator Click()
    {
        Trigger = true;
        raycastEndCollider.transform.position = drawRay.RaycastEnd.transform.position;
        yield return new WaitForFrames(3);
        raycastEndCollider.transform.localPosition = Vector3.zero;
        Trigger = false;
    }


    internal void ToggleMouseLock()
    {
        mouseIsLocked = !mouseIsLocked;
        Cursor.lockState = mouseIsLocked
            ? CursorLockMode.Locked
            : CursorLockMode.None;
        SetCursor(!mouseIsLocked);

    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(PointAndClickCameraController))]
public class TwoDoFControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var script = (PointAndClickCameraController)target;
    }
}
#endif
