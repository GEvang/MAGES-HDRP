#if FINAL_IK

using System.Collections;
using System.Collections.Generic;
using ovidVR.GameController;
using RootMotion.FinalIK;
using UnityEngine;

public class LookAtIKTargetController : MonoBehaviour
{
    public LookAtController lookAtController;
    LookAtIK lookAtIK;

    IEnumerator Start () {

        Transform lookAtTarget = OvidVRControllerClass.Get.GetCameraHead().transform;

        lookAtController = GetComponent<LookAtController>();

        GetComponent<LookAtController>().target = lookAtTarget;
		GetComponent<LookAtIK>().solver.target = lookAtTarget;
        yield return null;
    }
}
#endif