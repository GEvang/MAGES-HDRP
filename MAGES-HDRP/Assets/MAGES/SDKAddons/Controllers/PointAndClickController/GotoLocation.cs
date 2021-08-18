using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GotoLocation : MonoBehaviour
{
    [SerializeField]
    public Vector3 targetLocation;
    [SerializeField]
    public Vector2 minMaxYaw = new Vector2(0,360);
    [SerializeField]
    public Vector2 minMaxPitch = new Vector2(0, 360);
    [SerializeField]
    public List<string> disableOnClickNames;
    [SerializeField]
    public List<string> enableOnClickNames;
    [SerializeField]
    public bool startDisabled = false;
    [SerializeField]
    public bool noInterpolation = false;
    [SerializeField]
    public bool isBackOption = false;

    public Vector3 backRelativeLocation = new Vector3(0, 0, 0);

    public GameObject handle;

    private UnityEvent eventOnActivate = new UnityEvent();

    private List<GotoLocation> disableOnClick;
    private List<GotoLocation> enableOnClick;

    public void SetOnClickAction(UnityAction action)
    {
        eventOnActivate.AddListener(action);
    }

    private void Awake()
    {
        handle = transform.Find("Handle").gameObject;
        gameObject.SetActive(true);
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        yield return new WaitForFrames(1);

        var buttonBehavior = GetComponentInChildren<ButtonBehavior>();
        buttonBehavior.buttonFunction.AddListener(OnButtonPress);

        disableOnClick = new List<GotoLocation>();
        enableOnClick  = new List<GotoLocation>();

        var gotoObjects = FindObjectsOfType<GotoLocation>();

        foreach (var gtlString in enableOnClickNames)
        {
            foreach (var gotoObject in gotoObjects)
            {
                if (gtlString == gotoObject.gameObject.name)
                {
                    enableOnClick.Add(gotoObject);
                    break;
                }
            }
            
        }

        foreach (var gtlString in disableOnClickNames)
        {
            foreach (var gotoObject in gotoObjects)
            {
                if (gtlString == (gotoObject.gameObject.name))
                {
                    disableOnClick.Add(gotoObject);
                }
            }
        }

        //
        foreach (var gtl in disableOnClick)
        {
            if (gtl == this)
            {
                disableOnClick.Remove(gtl);
            }
        }

        foreach (var gtl in enableOnClick)
        {
            if (gtl == this)
            {
                disableOnClick.Remove(gtl);
            }
        }

        Initialize();
        yield return new WaitForFrames(2);
        SetActive(!startDisabled);
    }

    private void Initialize()
    {
        if (isBackOption)
        {
            Vector3 tmpHandlePos = handle.transform.position;
            Quaternion tmpHandleRot = handle.transform.rotation;

            handle.transform.SetParent(null, true);
            transform.SetParent(GameObject.Find("Cameras/VRCamera/[CameraRig]").transform, false);

            handle.transform.localPosition = tmpHandlePos;
            handle.transform.localRotation = tmpHandleRot;
        }
    }

    private void Start()
    {
        Initialize();
    }

    private IEnumerator Unparent()
    {
        if (isBackOption)
        {

            GameObject parent = null;

            while (true)
            {
                parent = GameObject.Find("Cameras/VRCamera/[CameraRig]");
                if (parent != null) break;

                yield return new WaitForEndOfFrame();
            }
            transform.SetParent(parent.transform, false);
        }
    }

    public void Update()
    {
        if (isBackOption)
        {
            var filmBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(
                backRelativeLocation.x * (1.0f / Camera.main.aspect),
                backRelativeLocation.y * (Camera.main.aspect),
                backRelativeLocation.z
                ));
            
            filmBottomLeft = Camera.main.transform.InverseTransformPoint(filmBottomLeft);

            var newLocation = filmBottomLeft;
            transform.localPosition = newLocation;
            transform.localRotation = Quaternion.identity;
        }
    }

    private void SetActive(bool active)
    {
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = active;
        }
        gameObject.SetActive(active);
    }

    public void GotoImmediate()
    {
        eventOnActivate.Invoke();

        if (!isBackOption)
            PointAndClickCameraController.Instance.GotoLocation(handle.transform.position, handle.transform.rotation, true);
        else
            PointAndClickCameraController.Instance.GotoLocation(handle.transform.position, handle.transform.rotation, true);


        gameObject.SetActive(false);
        foreach (var gtl in disableOnClick)
        {
            gtl.gameObject.GetComponent<GotoLocation>().SetActive(false);
        }

        foreach (var gtl in enableOnClick)
        {
            gtl.gameObject.GetComponent<GotoLocation>().SetActive(true);
        }
    }

    public void OnButtonPress()
    {
        eventOnActivate.Invoke();

        if (!isBackOption)
            PointAndClickCameraController.Instance.GotoLocation(handle.transform.position, handle.transform.rotation, noInterpolation);
        else
            PointAndClickCameraController.Instance.GotoLocation(handle.transform.position, handle.transform.rotation, noInterpolation);

        gameObject.SetActive(false);
        foreach (var gtl in disableOnClick)
        {
            gtl.gameObject.GetComponent<GotoLocation>().SetActive(false);
        }

        foreach (var gtl in enableOnClick)
        {
            gtl.gameObject.GetComponent<GotoLocation>().SetActive(true);
        }
    }


    private void OnDrawGizmos()
    {
        Color prevColor = Gizmos.color;

        Gizmos.color = new Color(0.2f, 0.5f, 0.5f);
        Gizmos.DrawSphere(handle.transform.position, 0.12f);

        Gizmos.color = new Color(0.6f,0.4f, 0.9f);
        Gizmos.DrawLine(transform.position, handle.transform.position);

        Gizmos.color = new Color(0.7f, 0.7f, 0.2f);
        Gizmos.DrawRay(handle.transform.position, handle.transform.TransformDirection(Vector3.forward)*5);

        Gizmos.color = prevColor;

    }

}

#if UNITY_EDITOR

[CustomEditor(typeof(GotoLocation)), CanEditMultipleObjects]
public class GotoLocationEditor : Editor
{
    private void Awake()
    {
        GotoLocation script = (GotoLocation)target;

        if (script.handle == null) script.handle = script.transform.Find("Handle").gameObject;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GotoLocation script = (GotoLocation)target;

        EditorGUILayout.BeginVertical();
        script.handle.transform.position = EditorGUILayout.Vector3Field("Position", script.handle.transform.position);
        script.handle.transform.eulerAngles = EditorGUILayout.Vector3Field("Rotation", script.handle.transform.eulerAngles);
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Position Camera Here"))
        {
            script.OnButtonPress();
        }
    }

    private void OnSceneGUI()
    {
        GotoLocation script = (GotoLocation)target;

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.left;
        Vector3 up = Vector3.up;

        // yaw constraint
        right = Quaternion.Euler(0.0f, script.minMaxYaw.x, 0.0f) * forward;
        Handles.DrawWireArc(script.handle.transform.position, up, right, script.minMaxYaw.y - script.minMaxYaw.x, 0.5f);

        // pitch constraint
        up = Vector3.back;
        right = Vector3.Normalize(Quaternion.Euler(0.0f, 0.0f, script.minMaxPitch.y) * Vector3.left);
        Handles.DrawWireArc(script.handle.transform.position, up, right, script.minMaxPitch.y - script.minMaxPitch.x, 0.5f);

        var prev = Handles.color;
        Handles.color = Color.red;
        Handles.DrawLine(script.handle.transform.position, script.handle.transform.position + Vector3.forward * 5);
        Handles.color = prev;

        EditorGUI.BeginChangeCheck();
        Vector3 newTargetLocation = Handles.PositionHandle(script.handle.transform.position, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(script, "Change Look At Target Position");
            script.handle.transform.position = newTargetLocation;
            script.Update();
        }

    }

}

#endif