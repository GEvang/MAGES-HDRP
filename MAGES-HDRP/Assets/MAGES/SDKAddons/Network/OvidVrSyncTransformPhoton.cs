using UnityEngine;
using ovidVR.Networking;
using ovidVR.GameController;
using ovidVR.rigidBodyAnimation;
using Photon.Pun;

[RequireComponent(typeof(OvidVrNetworkingId))]
[RequireComponent(typeof(PhotonView))]

public class OvidVrSyncTransformPhoton : MonoBehaviour,IOvidVRNetTransform, IPunObservable
{

    public enum TransformFlags
    {
        None = 1, //0001
        Rotation = 2, //0010
        Position = 4  //0100
        //4, 8, 16, 32, 64, 128, 256,
    }

    private RigidbodyMoveAndRotateDualQuat _interpolator;
    private OvidVrNetworkingId _netId;
    private PhotonView _pView;

    
    public Transform T { get; set; }
    public int SendTimesPerSecond
    {
        get
        {
            return sendTimesPerSecond;
        }

        set
        {
            SendInterval = 1.0f / SendTimesPerSecond;
            sendTimesPerSecond = value;
        }
    }

    [Range(1, 30),SerializeField]
    private int sendTimesPerSecond = 15;
    
    public float SendInterval { get; set; }
    
    public float MovementThreshold
    {
        get
        {
            return movementThreshold;
        }

        set
        {
            movementThreshold = value;
        }
    }
    [SerializeField]
    private float movementThreshold = 0.005f;
    
    public float RotationThreshold
    {
        get
        {
            return rotationThreshold;
        }

        set
        {
            rotationThreshold = value;
        }
    }
    [SerializeField]
    private float rotationThreshold = 1.0f;

    public Vector3 LastPos { get; set; }
    public Quaternion LastRot { get; set; }
    public float LastSendTime { get; set; }
    
    public OvidVRSyncTransformMode SyncTransformMode
    {
        get
        {
            return syncTransformMode;
        }

        set
        {
            syncTransformMode = value;
        }
    }
    [SerializeField] 
    private OvidVRSyncTransformMode syncTransformMode = OvidVRSyncTransformMode.all;
    
    private void Awake()
    {
        try
        {
            Initialise();
        }
        catch (System.Exception e)
        {
            Debug.LogError("" + e.StackTrace);
        }
    }

    public void Initialise()
    {
        T = transform;
        LastPos = T.position;
        LastRot = T.rotation;
        SendInterval = 1.0f / SendTimesPerSecond;
        _interpolator = new RigidbodyMoveAndRotateDualQuat(Vector3.zero, Quaternion.identity, this.gameObject, 1.0f / ((float)SendTimesPerSecond));
        RigidbodyAnimationController.addRigidbodyAnimation(_interpolator);
        _netId = GetComponent<OvidVrNetworkingId>();
        _pView = PhotonView.Get(this);

        if (_pView.ObservedComponents == null)
            _pView.ObservedComponents = new System.Collections.Generic.List<Component>();
        _pView.ObservedComponents.Add(this);
    }

    private void SyncTransform()
    {
        _interpolator.Restart(LastPos, LastRot);
    }

    public void ChangeSendRate(int sendrate)
    {
        SendTimesPerSecond = sendrate;
        SendInterval = 1.0f / SendTimesPerSecond;
        _interpolator.speed = 1.0f / (SendTimesPerSecond );
    }

    public void ChangeSyncMode(OvidVRSyncTransformMode ovidVrSyncTransformMode)
    {
        SyncTransformMode = ovidVrSyncTransformMode;
    }
           
    public void Tick()
    {
        
    }

    #region network commands

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            if (SyncTransformMode == OvidVRSyncTransformMode.none)
            {
                stream.SendNext((int)TransformFlags.None);
                return;
            };

            //Compute time from last tick
            float currentTime = Time.time;
            float passedTime = currentTime - LastSendTime;

            if (passedTime < SendInterval)
            {
                stream.SendNext((int)TransformFlags.None);
                return;
            }

            LastSendTime = currentTime;


            TransformFlags transfromFlags = TransformFlags.None;

            if (Vector3.Distance(T.position, LastPos) > MovementThreshold)
            {
                transfromFlags = transfromFlags | TransformFlags.Position;
            }

            if (Quaternion.Angle(T.rotation, LastRot) > RotationThreshold)
            {
                transfromFlags = transfromFlags | TransformFlags.Rotation;
            }

            stream.SendNext((int)transfromFlags);
            if (transfromFlags != TransformFlags.None)
            {

                if ((transfromFlags & TransformFlags.Position) != 0)
                {
                    var position = T.position;
                    LastPos = position;
                    stream.SendNext(position);
                }

                if ((transfromFlags & TransformFlags.Rotation) != 0)
                {
                    var rotation = T.rotation;
                    LastRot = rotation;
                    stream.SendNext(rotation);
                }
            }
            
            PhotonNetworkMetrics.Instance.IncrementTransformation(transfromFlags,true);
        }
        else
        {
            TransformFlags transfromFlags = (TransformFlags)stream.ReceiveNext();
            if (transfromFlags != TransformFlags.None)
            {
                if ((transfromFlags & TransformFlags.Position) != 0)
                {
                    LastPos = (Vector3)stream.ReceiveNext();

                }

                if ((transfromFlags & TransformFlags.Rotation) != 0)
                {
                    LastRot = (Quaternion)stream.ReceiveNext();
                }
                SyncTransform();
                PhotonNetworkMetrics.Instance.IncrementTransformation(transfromFlags,true);
            }
        }
    }


    #endregion
}
