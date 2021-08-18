using ovidVR.GameController;
using ovidVR.Networking;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace ovidVR.Networking
{
    [RequireComponent(typeof(OvidVrNetworkingId))]
    [RequireComponent(typeof(PhotonView))]

    public class NetMultiTransformPhoton : NetworkMultiTransformSync, IPunObservable
    {

        public enum TransformFlags
        {
            None = 1, //0001
            Rotation = 2, //0010
            Position = 4  //0100
                          //4, 8, 16, 32, 64, 128, 256,
        }

        private struct TransformSendData
        {
            public TransformFlags flags;
            public int ID;
            public Vector3 pos;
            public Quaternion rot;
        }

        private OvidVrNetworkingId netID;
        private PhotonView pView;

        public void Start()
        {
            Initialise();
        }

        private void Update()
        {
            if (OvidVRControllerClass.NetworkManager.GetIsInNetwork())
            {
                Tick();
                if (!netID.HasAuthority)
                {
                    for (int i = 0; i < transformSyncCount; ++i)
                    {
                        float lerp_percentage_pos =
                            Mathf.Clamp01((Time.time - StartTime_pos[i]) / (1.0f / (SendTimesPerSecond)));
                        float lerp_percentage_rot =
                            Mathf.Clamp01((Time.time - StartTime_rot[i]) / (1.0f / (SendTimesPerSecond)));

                        T[i].localPosition = Vector3.Lerp(StartPosition[i], LastPos[i], lerp_percentage_pos);
                        T[i].localRotation = Quaternion.Lerp(StartRoation[i], LastRot[i], lerp_percentage_rot);
                    }
                }
            }
        }

        #region network commands

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                List<TransformSendData> sendData = new List<TransformSendData>();
                float currentTime = Time.time;

                for (int i = 0; i < transformSyncCount; ++i)
                {
                    float passedTime = currentTime - LastSendTime[i];

                    if (passedTime < sendInterval)
                    {
                        stream.SendNext(0);
                        return;
                    }

                    LastSendTime[i] = currentTime;

                    TransformFlags transfromFlags = TransformFlags.None;

                    if (Vector3.Distance(T[i].localPosition, LastPos[i]) > MovementThreshold)
                    {
                        transfromFlags = transfromFlags | TransformFlags.Position;
                    }

                    if (Quaternion.Angle(T[i].localRotation, LastRot[i]) > RotationThreshold)
                    {
                        transfromFlags = transfromFlags | TransformFlags.Rotation;
                    }

                    if (transfromFlags != TransformFlags.None)
                    {
                        if ((transfromFlags & TransformFlags.Position) != 0)
                        {
                            var position = T[i].localPosition;
                            LastPos[i] = T[i].localPosition;
                        }

                        if ((transfromFlags & TransformFlags.Rotation) != 0)
                        {
                            var rotation = T[i].localRotation;
                            LastRot[i] = T[i].localRotation;
                        }

                        sendData.Add(new TransformSendData()
                            {flags = transfromFlags, ID = i, pos = LastPos[i], rot = LastRot[i]});
                    }
                    
                }

                stream.SendNext(sendData.Count);
                if (sendData.Count > 0)
                {
                    foreach (TransformSendData sendValues in sendData)
                    {
                        if (sendValues.flags != TransformFlags.None)
                        {
                            stream.SendNext((int) sendValues.flags);
                            stream.SendNext((int) sendValues.ID);


                            if ((sendValues.flags & TransformFlags.Position) != 0)
                            {
                                stream.SendNext(sendValues.pos);
                            }

                            if ((sendValues.flags & TransformFlags.Rotation) != 0)
                            {
                                stream.SendNext(sendValues.rot);
                            }
                        }
                        PhotonNetworkMetrics.Instance.IncrementTransformation(sendValues.flags,true);
                    }
                }
            }
            else
            {
                int transformsCount = (int) stream.ReceiveNext();
                for (int i = 0; i < transformsCount; i++)
                {
                    TransformFlags transfromFlags = (TransformFlags) stream.ReceiveNext();
                    int currentID = (int) stream.ReceiveNext();
                    if (transfromFlags != TransformFlags.None)
                    {
                        if ((transfromFlags & TransformFlags.Position) != 0)
                        {
                            LastPos[currentID] = (Vector3) stream.ReceiveNext();
                            StartTime_pos[currentID] = Time.time;
                            StartPosition[currentID] = T[currentID].localPosition;

                        }

                        if ((transfromFlags & TransformFlags.Rotation) != 0)
                        {
                            LastRot[currentID] = (Quaternion) stream.ReceiveNext();
                            StartTime_rot[currentID] = Time.time;
                            StartRoation[currentID] = T[currentID].localRotation;
                        }
                    }
                    PhotonNetworkMetrics.Instance.IncrementTransformation(transfromFlags,false);
                }
            }
        }

        #endregion
        public override void ChangeSyncMode(OvidVRSyncTransformMode _ovidVRSyncTransformMode)
        {
            throw new System.NotImplementedException();
        }

        public override void Initialise()
        {
            if (!OvidVRControllerClass.Get.IsInNetwork)
            {
                Destroy(this);
            }

            if (T == null || T.Length == 0)
            {
                T = new Transform[transformSyncCount];
            }

            transformSyncCount = T.Length;
            LastPos = new Vector3[transformSyncCount];
            LastRot = new Quaternion[transformSyncCount];

            StartTime_pos = new float[transformSyncCount];
            StartPosition = new Vector3[transformSyncCount];

            StartTime_rot = new float[transformSyncCount];
            StartRoation = new Quaternion[transformSyncCount];

            LastSendTime = new float[transformSyncCount];

            for (int i = 0; i < transformSyncCount; ++i)
            {
                //t[i] = transform.GetChild(i);
                LastPos[i] = T[i].localPosition;
                LastRot[i] = T[i].localRotation;
            }


            sendInterval = 1.0f / SendTimesPerSecond;
            netID = GetComponent<OvidVrNetworkingId>();
            pView = PhotonView.Get(this);
            if (pView.ObservedComponents == null)
                pView.ObservedComponents = new System.Collections.Generic.List<Component>();
            pView.ObservedComponents.Add(this);

        }

        public override void Tick()
        {
           
        }

        public override void ChangeSendRate(int _sendrate)
        {
            throw new System.NotImplementedException();
        }
    }
}
