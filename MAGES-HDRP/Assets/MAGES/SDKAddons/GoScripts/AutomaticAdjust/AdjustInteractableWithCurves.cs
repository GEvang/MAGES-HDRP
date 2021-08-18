using ovidVR.GameController;
using ovidVR.rigidBodyAnimation;
using OvidVRPhysX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustInteractableWithCurves : MonoBehaviour {

    public bool UseCustomDropDistance = true;
    OvidVRInteractableItem interactable;
    public string TargetTransformName;
    public Transform OvverideTargetTransform = null;

    public string MiddleTargetTransformName;
    public Transform MiddleOvverideTargetTransform = null;

    public Transform OvverideControllerTransform = null;
    
    float totalDistance;
    Quaternion statingRoation;
    Vector3 startingPosition;

    float statingAngle;
    Quaternion startingControllerRotation;
    // Use this for initialization
    public IEnumerator Start () {       

        if(OvidVRControllerClass.Get.DOF != ControllerDOF.ThreeDOF)
        {
            Destroy(this);
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        if (OvverideTargetTransform == null)
        {
            if (GameObject.Find(TargetTransformName))
                OvverideTargetTransform = GameObject.Find(TargetTransformName).transform;
        }

        if(OvverideTargetTransform && OvverideTargetTransform.GetComponent<PrefabLerpPlacement>())
        {
            OvverideTargetTransform.GetComponent<PrefabLerpPlacement>().maxAngleDegreeDiff = 0.0f;
        }

        if (MiddleOvverideTargetTransform == null)
        {
            if (GameObject.Find(MiddleTargetTransformName))
                MiddleOvverideTargetTransform = GameObject.Find(MiddleTargetTransformName).transform;
            else
                Debug.LogError("Cannot find middle target transfrom");
        }

        if (OvverideControllerTransform == null)
        {
            if(OvidVRControllerClass.Get.rightHand.activeInHierarchy)
                OvverideControllerTransform = OvidVRControllerClass.Get.rightHand.transform;
            else
                OvverideControllerTransform = OvidVRControllerClass.Get.leftHand.transform;
        }

        if (OvverideTargetTransform == null || OvverideControllerTransform == null)
        {
            Destroy(this);
        }


        interactable = this.GetComponent<OvidVRInteractableItem>();
        interactable.OnBeginInteraction.AddListener(() => {
            try
            {
                statingRoation = interactable.PickupTransform.rotation;
                startingPosition = interactable.PickupTransform.position;

                startingControllerRotation =
                    Quaternion.LookRotation(OvverideTargetTransform.position, OvverideControllerTransform.up);
                statingAngle = Vector3.Angle(OvverideControllerTransform.forward,
                    (OvverideTargetTransform.position - OvverideControllerTransform.position));


                totalDistance = Vector3.Distance(this.OvverideTargetTransform.position,
                    interactable.PickupTransform.position);
            }
            catch(System.Exception e)
            {
                Debug.LogError(e);
            }
        });
        if(interactable && UseCustomDropDistance)
        {
            interactable.DropDistance = 100.0f;
        }


        yield return null;
    }
	
	// Update is called once per frame
	void Update () {
		if(interactable.IsAttached)
        {
            this.interactable.PickupTransform.position = SampleCurve(startingPosition, OvverideTargetTransform.position, MiddleOvverideTargetTransform.position, GetProgressPossition());
            this.interactable.PickupTransform.rotation = Quaternion.Lerp(statingRoation, OvverideTargetTransform.rotation, GetProgress()* GetProgress());
        }
	}

    float GetProgress()
    {
        return 1.3f*Mathf.Clamp(
                (1.0f - Vector3.Distance(this.OvverideTargetTransform.position, interactable.PickupTransform.position) / totalDistance)
            , 0.0f, 1.0f);
    }

    float GetProgressPossition()
    {
        return 1.1f*Mathf.Clamp(
                (1.0f - Vector3.Angle(OvverideControllerTransform.forward,
                    (OvverideTargetTransform.position - OvverideControllerTransform.position)) / statingAngle)
            , 0.0f, 1.0f);
    }

    Vector3 SampleCurve(Vector3 start, Vector3 end, Vector3 control, float t)
    {
        // Interpolate along line S0: control - start;
        Vector3 Q0 = Vector3.Lerp(start, control, t);
        // Interpolate along line S1: S1 = end - control;
        Vector3 Q1 = Vector3.Lerp(control, end, t);
        // Interpolate along line S2: Q1 - Q0
        Vector3 Q2 = Vector3.Lerp(Q0, Q1, t);
        return Q2; // Q2 is a point on the curve at time t
    }


}
