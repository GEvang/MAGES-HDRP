using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.toolManager;
using ovidVR.toolManager.tool;
using OvidVRPhysX;
using System;
using ovidVR.GameController;

public class CauterizerGestureHands : GestureHands
{
    [SerializeField]
    ParticleSystem[] psystem;
    public override void SetUpTool(Tool _tool)
    {
        base.pulseValue = 0.25f;

        base.SetUpTool(_tool);
    }

    public override void ChangeToolLayer(bool _isToolActive)
    {
        if (!_isToolActive)
        {
            psystem[0].Stop();
            psystem[1].Stop();
        }
        base.ChangeToolLayer(_isToolActive);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (!grabStrength)
            return;

        if (collider.name.StartsWith("BloodSpot"))
        {
            psystem[1].Play();
            psystem[0].Play();
        }
        else
            psystem[0].Play();
    }

    void OnTriggerExit(Collider collider)
    {
        if (!grabStrength)
            return;

        if (collider.name.StartsWith("BloodSpot"))
        {
            psystem[1].Stop();
            psystem[0].Stop();
        }
        else
            psystem[0].Stop();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (RemoteActivation) DeactivateTool(this);
            else ActivateTool(this);
        }
    }

    public void StopAudio()
    {
        //audioSource.Stop();

        //psystem[0].Stop();
        //psystem[1].Stop();

        ovidVR.GameController.OvidVRControllerClass.Get.ControllerHapticPulse(tool.ToolRightAttachment ? OvidVRControllerClass.OvidVRHand.right : OvidVRControllerClass.OvidVRHand.left, 0);

        //colliderObject = null;
    }
}
