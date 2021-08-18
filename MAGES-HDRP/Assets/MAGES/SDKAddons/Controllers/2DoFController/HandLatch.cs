using ovidVR.GameController;
using UnityEngine;

public class HandLatch : MonoBehaviour
{
    internal OvidVRControllerClass.OvidVRHand handType;
    public Vector3 Offset { get; set; }
    public Vector3 DefaultOffset { get; set; }
    public Quaternion LocalRotation { get { return transform.localRotation; } set { transform.localRotation = value; } }
    public GameObject visGrids = null;
    public GameObject visGridXY = null;

    public GameObject visSphere;
    public static HandLatch Install(GameObject hand, OvidVRControllerClass.OvidVRHand type, GameObject vis, GameObject visSphere)
    {
        HandLatch result = hand.AddComponent<HandLatch>();
        result.handType = type;
        result.visGrids = Instantiate(vis);
        result.visGridXY = result.visGrids.transform.GetChild(0).gameObject;
        result.visSphere = Instantiate(visSphere);
        return result;
    }

    private void Awake()
    {
        var go = new GameObject("ConnectedPoint");
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localScale = Vector3.one;
    }

    public Vector3 GlobalPosition {
        get
        {
            return transform.position;
        }

        set
        {
            transform.position = value;
        }
    }

    public Quaternion GlobalRotation
    {
        get
        {
            return transform.rotation;
        }

        set
        {
            transform.rotation = value;
        }
    }
}
