using UnityEngine;
using System.Collections;
using ovidVR.Networking;

public class SetIKTarget : MonoBehaviour {
    IKManager manager;
    public GameObject newIKTarget;
    public string IKObjectName;
    void Start () {

        if (!ovidVR.GameController.OvidVRControllerClass.NetworkManager.GetIsInNetwork())
        {
            Destroy(this.gameObject);
        }
        else
        {
            GameObject IKObject = GameObject.Find(IKObjectName);
            manager = IKObject.transform.Find("IKManager").GetComponent<IKManager>();

            transform.parent = IKObject.transform;
            Destroy(manager.target.parent.gameObject);
            manager.target = newIKTarget.transform;

            manager.SetIsEnabledFalse();
            StartCoroutine(WaitForPlayer());
            
        }

	}

    IEnumerator WaitForPlayer()
    {
        yield return new WaitForSeconds(2.0f);
        manager.SetIsEnabledTrue();

        yield return new WaitForSeconds(1.0f);
        manager.SetIsEnabledFalse();

        INetworkID netID = GetComponent<OvidVrNetworkingId>();
        GetComponent<OvidVRPhysX.OvidVRInteractableItem>().OnBeginInteraction.AddListener(() => {
            if (ovidVR.GameController.OvidVRControllerClass.NetworkManager.GetIsServer())
            {
                manager.SetIsEnabledTrue();
            }
            NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.IKLampEnable, netID.NetID);
            UNETChat.u.SendMessage(m);
        });

        GetComponent<OvidVRPhysX.OvidVRInteractableItem>().OnEndInteraction.AddListener(() => {
            if (ovidVR.GameController.OvidVRControllerClass.NetworkManager.GetIsServer())
            {
                manager.SetIsEnabledFalse();
            }
            NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.IKLampDisable, netID.NetID);
            UNETChat.u.SendMessage(m);
        });
        GetComponent<OvidVRPhysX.OvidVRInteractableItem>().OnEndInteraction.AddListener(() => {
            if (ovidVR.GameController.OvidVRControllerClass.NetworkManager.GetIsServer())
            {
                manager.ResetTargetPosition();
            }
            NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.IKLampRetarget, netID.NetID);
            UNETChat.u.SendMessage(m);
        });

        Destroy(this);
    }
    
	
}
