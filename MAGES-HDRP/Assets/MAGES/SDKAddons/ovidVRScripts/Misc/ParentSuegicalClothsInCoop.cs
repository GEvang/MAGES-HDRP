using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using ovidVR.Networking;
using UnityEngine;

public class ParentSuegicalClothsInCoop : MonoBehaviour {

    public Vector3 ParentLocalPosition;
    public Vector3 ParentLocalRotation;

    public string TransformInServerAvatar;

    IEnumerator Start()
    {
        if(!OvidVRControllerClass.Get.IsInNetwork)
        {
            Destroy(this);
            yield break;
        }
        if(GetComponent<OvidVrSyncTransformPhoton>())
        {
            GetComponent<OvidVrSyncTransformPhoton>().ChangeSyncMode(OvidVRSyncTransformMode.none);
        }
        yield return new WaitForSeconds(1);

        if (OvidVRControllerClass.Get.IsInNetwork && !OvidVRControllerClass.Get.isServer)
        {
            Transform serverAvatarTransform = FindServerAvatar();
            this.transform.parent = serverAvatarTransform;
            this.transform.localPosition = ParentLocalPosition;
            this.transform.localRotation = Quaternion.Euler(ParentLocalRotation);
        }
    }

    Transform FindServerAvatar()
    {
        MultiplayerAvater []avatars = FindObjectsOfType<MultiplayerAvater>();
        foreach(MultiplayerAvater avatar in avatars)
        {
            if (avatar.isServer)
                return avatar.transform.FindDeepChild(TransformInServerAvatar);
        }
        return null;
    }

}
