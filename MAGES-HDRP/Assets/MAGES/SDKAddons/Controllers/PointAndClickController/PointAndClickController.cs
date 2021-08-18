using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAndClickController : DeviceController
{

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Start()
    {
        //Vector3 fixCamPos = new Vector3(-9.142f, -0.376f, -0.634f);

        OvidVRControllerClass.DeviceController = this;

    }

    private void Update()
    {

    }

    public override void SetHandColor(Color _handColor, float _flashSpeed, OvidVRControllerClass.OvidVRHand _hand)
    {
        //throw new System.NotImplementedException();
    }

    public override void ResetHandsColor()
    {
        //throw new System.NotImplementedException();
    }

    public override void SetDefaultHandsColor(Color _color)
    {
        //throw new System.NotImplementedException();
    }

    public override void SetControllerState(OvidVRControllerClass.OvidVRHand _hand, bool state)
    {
        //throw new System.NotImplementedException();
    }

    public override void SetButtonFlashing(bool _enabled, OvidVRControllerClass.OvidVRHand _hand, params OvidVRControllerClass.OvidVRControllerButtons[] _buttons)
    {
        //throw new System.NotImplementedException();
    }

    public override void SetHandTransparency(float _alpha, OvidVRControllerClass.OvidVRHand _hand)
    {
        //throw new System.NotImplementedException();
    }

    public override bool GetButtonPressed(OvidVRControllerClass.OvidVRHand _hand, OvidVRControllerClass.OvidVRControllerButtons button)
    {
        return false;
    }

    public override float GetTriggerStrength(OvidVRControllerClass.OvidVRHand _hand)
    {
        return PointAndClickCameraController.Instance.Trigger?1.0f:0.0f;
    }

    public override float GetControllerGrabStrength(OvidVRControllerClass.OvidVRHand _hand)
    {
        return 0.0f;
    }

    public override string GetHandTag(GameObject child)
    {
        if (child == null)
            return "UnTagged";
        else if (child.transform.parent == null)
            return "UnTagged";
        else if (child.transform.parent.parent == null)
            return "UnTagged";

        return child.tag;
    }

    public override Transform GetHandTransform(GameObject child)
    {
        return child.transform;
    }

    public override void ControllerHapticPulse(OvidVRControllerClass.OvidVRHand _hand, float strength, float freq, float duration)
    {
        //throw new System.NotImplementedException();
    }

    public override bool GetIsGrabed(OvidVRControllerClass.OvidVRHand _hand)
    {

        return false;
    }

    public override Vector2 GetThumpPosOnController(OvidVRControllerClass.OvidVRHand _hand)
    {
        return Vector2.zero;
    }

    public override OvidVRControllerClass.OvidVRControllerButtons[] GetPressedButtons(OvidVRControllerClass.OvidVRHand _hand)
    {
        throw new System.NotImplementedException();
    }
}
