using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonVRController : DeviceController
{
    Transform CameraTransform;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void SetHandColor(Color _handColor, float _flashSpeed, OvidVRControllerClass.OvidVRHand _hand)
    {
    }

    public override void ResetHandsColor()
    {
    }

    public override void SetDefaultHandsColor(Color _color)
    {
    }

    public override void SetControllerState(OvidVRControllerClass.OvidVRHand _hand, bool state)
    {
    }

    public override void SetButtonFlashing(bool _enabled, OvidVRControllerClass.OvidVRHand _hand, params OvidVRControllerClass.OvidVRControllerButtons[] _buttons)
    {
    }

    public override void SetHandTransparency(float _alpha, OvidVRControllerClass.OvidVRHand _hand)
    {
    }    

    public override float GetTriggerStrength(OvidVRControllerClass.OvidVRHand _hand)
    {

        return _hand == OvidVRControllerClass.OvidVRHand.left
            ? TwoDoFController.Instance.leftHand.Trigger?1.0f:0.0f
            : TwoDoFController.Instance.rightHand.Trigger ? 1.0f : 0.0f;
    }

    public override float GetControllerGrabStrength(OvidVRControllerClass.OvidVRHand _hand)
    {
        return _hand == OvidVRControllerClass.OvidVRHand.left
            ? TwoDoFController.Instance.leftHand.Trigger  ? 1.0f : 0.0f
            : TwoDoFController.Instance.rightHand.Trigger ? 1.0f : 0.0f;
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

    public override void ControllerHapticPulse(OvidVRControllerClass.OvidVRHand _hand, float strength, float _freq = 0.5F, float _duration = 0.7F)
    {
        
    }

    public override bool GetIsGrabed(OvidVRControllerClass.OvidVRHand _hand)
    {
        return _hand == OvidVRControllerClass.OvidVRHand.left
            ? TwoDoFController.Instance.leftHand.Trigger || TwoDoFController.Instance.leftHand.Grip
            : TwoDoFController.Instance.rightHand.Trigger || TwoDoFController.Instance.rightHand.Grip;
    }


    public override Vector2 GetThumpPosOnController(OvidVRControllerClass.OvidVRHand _hand)
    {
        return Vector2.zero;
    }

    public override OvidVRControllerClass.OvidVRControllerButtons[] GetPressedButtons(OvidVRControllerClass.OvidVRHand _hand)
    {
        return null;
    }

    public override bool GetButtonPressed(OvidVRControllerClass.OvidVRHand _hand, OvidVRControllerClass.OvidVRControllerButtons button)
    {
        return false;
    }
}
