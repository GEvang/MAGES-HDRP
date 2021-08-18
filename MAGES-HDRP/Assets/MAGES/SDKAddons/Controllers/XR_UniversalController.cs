using ovidVR.GameController;
using ovidVR.UIManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class XR_UniversalController : DeviceController
{

    List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
    List<OvidVRControllerClass.OvidVRControllerButtons> ovidVRControllerButtonList = new List<OvidVRControllerClass.OvidVRControllerButtons>();

    public override void ControllerHapticPulse(OvidVRControllerClass.OvidVRHand _hand, float strength, float _freq = 0.5F, float _duration = 0.1F)
    {
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics
            ((_hand == OvidVRControllerClass.OvidVRHand.right)?
            UnityEngine.XR.InputDeviceCharacteristics.Right:
            UnityEngine.XR.InputDeviceCharacteristics.Left
            , devices);

        foreach (var device in devices)
        {
            UnityEngine.XR.HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    device.SendHapticImpulse(0, strength, _duration);
                }
            }
        }
    }

    public override float GetControllerGrabStrength(OvidVRControllerClass.OvidVRHand _hand)
    {
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics
            ((_hand == OvidVRControllerClass.OvidVRHand.right) ?
            UnityEngine.XR.InputDeviceCharacteristics.Right :
            UnityEngine.XR.InputDeviceCharacteristics.Left
            , devices);

        float gripValue = 0.0f;
        foreach (var device in devices)
        {
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out gripValue);           
        }
        return gripValue;
    }

    //public override bool GetGripPressed(OvidVRControllerClass.OvidVRHand _hand)
    //{
    //    UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics
    //        ((_hand == OvidVRControllerClass.OvidVRHand.right) ?
    //        UnityEngine.XR.InputDeviceCharacteristics.Right :
    //        UnityEngine.XR.InputDeviceCharacteristics.Left
    //        , devices);

    //    float gripValue = 0.0f;
    //    foreach (var device in devices)
    //    {
    //        device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out gripValue);
    //    }
    //    return gripValue;
    //}

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

    public override bool GetIsGrabed(OvidVRControllerClass.OvidVRHand _hand)
    {
        return Mathf.Max(GetControllerGrabStrength(_hand) , GetTriggerStrength(_hand)) > 0.6f;
    }

    Dictionary<OvidVRControllerClass.OvidVRControllerButtons, ButtonPressed> leftControllerButtons;
    Dictionary<OvidVRControllerClass.OvidVRControllerButtons, ButtonPressed> rightControllerButtons;

    public void Awake()
    {
        InvokeRepeating("CheckHMDAndControllersConnection", 11.0f, 7.0f);

        leftControllerButtons = new Dictionary<OvidVRControllerClass.OvidVRControllerButtons, ButtonPressed>();
        rightControllerButtons = new Dictionary<OvidVRControllerClass.OvidVRControllerButtons, ButtonPressed>();

        leftControllerButtons.Add(OvidVRControllerClass.OvidVRControllerButtons.A, new ButtonPressed());
        leftControllerButtons.Add(OvidVRControllerClass.OvidVRControllerButtons.B, new ButtonPressed());
        leftControllerButtons.Add(OvidVRControllerClass.OvidVRControllerButtons.GripButton, new ButtonPressed());
        leftControllerButtons.Add(OvidVRControllerClass.OvidVRControllerButtons.MenuButton, new ButtonPressed());
        leftControllerButtons.Add(OvidVRControllerClass.OvidVRControllerButtons.ThumbStick, new ButtonPressed());
        leftControllerButtons.Add(OvidVRControllerClass.OvidVRControllerButtons.TriggerButton, new ButtonPressed());

        rightControllerButtons.Add(OvidVRControllerClass.OvidVRControllerButtons.A, new ButtonPressed());
        rightControllerButtons.Add(OvidVRControllerClass.OvidVRControllerButtons.B, new ButtonPressed());
        rightControllerButtons.Add(OvidVRControllerClass.OvidVRControllerButtons.GripButton, new ButtonPressed());
        rightControllerButtons.Add(OvidVRControllerClass.OvidVRControllerButtons.MenuButton, new ButtonPressed());
        rightControllerButtons.Add(OvidVRControllerClass.OvidVRControllerButtons.ThumbStick, new ButtonPressed());
        rightControllerButtons.Add(OvidVRControllerClass.OvidVRControllerButtons.TriggerButton, new ButtonPressed());
    }

    public void Update()
    {
        UpdateGetPressedButtons(OvidVRControllerClass.OvidVRHand.left);
        UpdateGetPressedButtons(OvidVRControllerClass.OvidVRHand.right);
    }

    public override bool GetButtonPressed(OvidVRControllerClass.OvidVRHand _hand, OvidVRControllerClass.OvidVRControllerButtons button)
    {
        if(_hand == OvidVRControllerClass.OvidVRHand.left)
            return leftControllerButtons[button].IsPressed;
        else
            return rightControllerButtons[button].IsPressed;
    }


    public override OvidVRControllerClass.OvidVRControllerButtons[] GetPressedButtons(OvidVRControllerClass.OvidVRHand _hand)
    {
        ovidVRControllerButtonList.Clear();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics
             ((_hand == OvidVRControllerClass.OvidVRHand.right) ?
             UnityEngine.XR.InputDeviceCharacteristics.Right :
             UnityEngine.XR.InputDeviceCharacteristics.Left
             , devices);
        foreach (var device in devices)
        {
            bool triggerButton, gripButton, menuButton, primaryButton, secondaryButton, primary2DAxisClick;
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton,out triggerButton);
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton,out gripButton);
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton,out menuButton);
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton,out primaryButton);
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton,out secondaryButton);
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick,out primary2DAxisClick);

            if (triggerButton)
                ovidVRControllerButtonList.Add(OvidVRControllerClass.OvidVRControllerButtons.TriggerButton);
            if (gripButton)
                ovidVRControllerButtonList.Add(OvidVRControllerClass.OvidVRControllerButtons.GripButton);
            if (menuButton)
                ovidVRControllerButtonList.Add(OvidVRControllerClass.OvidVRControllerButtons.MenuButton);
            if (primaryButton)
                ovidVRControllerButtonList.Add(OvidVRControllerClass.OvidVRControllerButtons.A);
            if (secondaryButton)
                ovidVRControllerButtonList.Add(OvidVRControllerClass.OvidVRControllerButtons.B);
            if (primary2DAxisClick)
                ovidVRControllerButtonList.Add(OvidVRControllerClass.OvidVRControllerButtons.ThumbStick);
        }

        return ovidVRControllerButtonList.Count > 0 ? ovidVRControllerButtonList.ToArray() : null;
    }
    public void UpdateGetPressedButtons(OvidVRControllerClass.OvidVRHand _hand)
    {
        ovidVRControllerButtonList.Clear();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics
             ((_hand == OvidVRControllerClass.OvidVRHand.right) ?
             UnityEngine.XR.InputDeviceCharacteristics.Right :
             UnityEngine.XR.InputDeviceCharacteristics.Left
             , devices);
        foreach (var device in devices)
        {
            bool triggerButton, gripButton, menuButton, primaryButton, secondaryButton, primary2DAxisClick;

            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerButton);
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripButton);
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out menuButton);
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out primaryButton);
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out secondaryButton);
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out primary2DAxisClick);

            if(_hand == OvidVRControllerClass.OvidVRHand.left)
            {
                leftControllerButtons[OvidVRControllerClass.OvidVRControllerButtons.TriggerButton].updateValue(triggerButton);
                leftControllerButtons[OvidVRControllerClass.OvidVRControllerButtons.GripButton].updateValue(gripButton);
                leftControllerButtons[OvidVRControllerClass.OvidVRControllerButtons.MenuButton].updateValue(menuButton);
                leftControllerButtons[OvidVRControllerClass.OvidVRControllerButtons.A].updateValue(primaryButton);
                leftControllerButtons[OvidVRControllerClass.OvidVRControllerButtons.B].updateValue(secondaryButton);
                leftControllerButtons[OvidVRControllerClass.OvidVRControllerButtons.ThumbStick].updateValue(primary2DAxisClick);
            }
            else
            {
                rightControllerButtons[OvidVRControllerClass.OvidVRControllerButtons.TriggerButton].updateValue(triggerButton);
                rightControllerButtons[OvidVRControllerClass.OvidVRControllerButtons.GripButton].updateValue(gripButton);
                rightControllerButtons[OvidVRControllerClass.OvidVRControllerButtons.MenuButton].updateValue(menuButton);
                rightControllerButtons[OvidVRControllerClass.OvidVRControllerButtons.A].updateValue(primaryButton);
                rightControllerButtons[OvidVRControllerClass.OvidVRControllerButtons.B].updateValue(secondaryButton);
                rightControllerButtons[OvidVRControllerClass.OvidVRControllerButtons.ThumbStick].updateValue(primary2DAxisClick);
            }
        }

    }

    public override Vector2 GetThumpPosOnController(OvidVRControllerClass.OvidVRHand _hand)
    {
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics
              ((_hand == OvidVRControllerClass.OvidVRHand.right) ?
              UnityEngine.XR.InputDeviceCharacteristics.Right :
              UnityEngine.XR.InputDeviceCharacteristics.Left
              , devices);

        Vector2 trackPadValue = Vector2.zero;
        foreach (var device in devices)
        {
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out trackPadValue);
        }
        return trackPadValue;
    }

    public override float GetTriggerStrength(OvidVRControllerClass.OvidVRHand _hand)
    {
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics
            ((_hand == OvidVRControllerClass.OvidVRHand.right) ?
            UnityEngine.XR.InputDeviceCharacteristics.Right :
            UnityEngine.XR.InputDeviceCharacteristics.Left
            , devices);

        float indexValue = 0.0f;
        foreach (var device in devices)
        {
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out indexValue);
        }
        return indexValue;
    }

    public override void ResetHandsColor()
    {
        //throw new System.NotImplementedException();
    }

    public override void SetButtonFlashing(bool _enabled, OvidVRControllerClass.OvidVRHand _hand, params OvidVRControllerClass.OvidVRControllerButtons[] _buttons)
    {
        //throw new System.NotImplementedException();
    }

    public override void SetControllerState(OvidVRControllerClass.OvidVRHand _hand, bool state)
    {
        //if (leftController == null || rightController == null)
        //    return;
        //bool handState = false;
        //if (state == true/* && ToolsManager.GetSelectedTool() == null*/)
        //    handState = true;

        //// Set Tutorial Controller Buttons
        //if (!handState)
        //{
        //    SetButtonFlashing(false, _hand, OvidVRControllerClass.OvidVRControllerButtons.GripButton,
        //                                    OvidVRControllerClass.OvidVRControllerButtons.MenuButton,
        //                                    OvidVRControllerClass.OvidVRControllerButtons.ThumbStick,
        //                                    OvidVRControllerClass.OvidVRControllerButtons.TriggerButton);
        //}
    }

    public override void SetDefaultHandsColor(Color _color)
    {
        //throw new System.NotImplementedException();
    }

    public override void SetHandColor(Color _handColor, float _flashSpeed, OvidVRControllerClass.OvidVRHand _hand)
    {
        //throw new System.NotImplementedException();
    }

    public override void SetHandTransparency(float _alpha, OvidVRControllerClass.OvidVRHand _hand)
    {
        //throw new System.NotImplementedException();
    }

    private bool isDeviceConnected(UnityEngine.XR.InputDeviceCharacteristics device)
    {
        
        List<UnityEngine.XR.InputDevice> current_devices = new List<UnityEngine.XR.InputDevice>();

        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(device, current_devices);

        if (current_devices == null || current_devices.Count == 0)
            return false;

        //foreach (var curr_device in current_devices)
        //{
        //    if (!curr_device.isValid)
        //        return false;            
        //}
        return true;
    }

    private void CheckHMDAndControllersConnection()
    {
        string ErrorMessage = "";
        bool lControllerConnected = isDeviceConnected(UnityEngine.XR.InputDeviceCharacteristics.Left);
        bool rControllerConnected = isDeviceConnected(UnityEngine.XR.InputDeviceCharacteristics.Right);
        bool HMDConnected = isDeviceConnected(UnityEngine.XR.InputDeviceCharacteristics.HeadMounted);
        if (!lControllerConnected || !rControllerConnected)
            ErrorMessage = "Check " +
                ((lControllerConnected)? "" : "Left ") +
                ((rControllerConnected) ? "" : "Right ") +
                "controller connection\n";

        if(!HMDConnected)
            ErrorMessage += "HMD controller is not connected\n";

        if (!string.IsNullOrEmpty(ErrorMessage))
            InterfaceManagement.Get.SpawnDynamicNotificationUI(NotificationUITypes.UICriticalError, ErrorMessage, 4.0f);
    }


}

public class ButtonPressed
{
    bool current_value = false;
    bool ret_value = false;
    bool value_changed = false;
    public bool IsPressed;
    public void updateValue(bool val)
    {
        current_value = val;
        IsPressed = CheckPressed();
    }

    private bool CheckPressed()
    {
        if (value_changed == current_value)
            return false;

        value_changed = current_value;
        ret_value = ret_value == false && current_value == true;

        return ret_value;
    }
}