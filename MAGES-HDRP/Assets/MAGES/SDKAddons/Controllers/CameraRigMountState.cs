using UnityEngine;
#if STEAMVR_SDK && !UNITY_ANDROID
using Valve.VR;
#endif

public class CameraRigMountState : MonoBehaviour {

    GameObject loadPrefab;

    // When application is started the headset state is already off
    // and the standby prefab spawns immediatelly. Only for application start change
    // the timer in order to give the user time to put on the headset
    bool hasStarted;

    void Start ()
    {
        loadPrefab = Resources.Load("MAGESres/Cameras/NonVR/Steam_HMD_StandBy", typeof(GameObject)) as GameObject;

        if(loadPrefab == null)
        {
            Debug.LogError("No Steam_HMD_StandBy was founded in Resources to be loaded");
            enabled = false;
        }

        hasStarted = false;
    }

#if !UNITY_EDITOR && STEAMVR_SDK && !UNITY_ANDROID
    void Update () {
        timer += Time.deltaTime;

        if(timer >= checkHMDStatusInterval)
        {
            timer = 0;

            if (!hasStarted)
            {
                hasStarted = true;
                checkHMDStatusInterval -= 6f;
            }

            if(OpenVR.System.GetTrackedDeviceActivityLevel(0) == EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction)
            {
                if(standBy != null)
                {
                    Destroy(standBy);
                    standBy = null;
                }
            }
            else
            {
                if (standBy == null)
                    standBy = Instantiate(loadPrefab);
            }
        }
    }
#endif
}
