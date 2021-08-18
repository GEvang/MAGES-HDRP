using System;
using System.Collections;
using ovidVR.GameController;
using ovidVR.Networking;
using Photon.Pun;
using UnityEngine;

public enum NetworkingApi {PhotonNetwork }

public class OvidVrNetworkingId :  MonoBehaviour,INetworkID
{
	private NetworkingApi currentNetwork;
	private PhotonView thisPV;	
	
	public ushort NetID { get { return (ushort)ThisPV.ViewID; } set { } } 
	public bool HasAuthority
    {
        get
        {
            return ThisPV.IsMine;
        }

        set {; }
    }

    public PhotonView ThisPV
    {
        get
        {
            if(thisPV == null)
                thisPV = PhotonView.Get(this);
            return thisPV;
        }

        set
        {
            thisPV = value;
        }
    }


    public void OnDestroy()
    {
        if(OvidVRControllerClass.Get.IsInNetwork && OvidVRControllerClass.Get.isServer)
            NetworkFunctions.DestroyObject(gameObject);
    }
}
