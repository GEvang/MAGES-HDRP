using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.toolManager;
using ovidVR.toolManager.tool;
using OvidVRPhysX;
using System;

public class BonesawGestureHands : GestureHands
{
    [SerializeField] private GameObject m_RotatableObject;
    [SerializeField] private bool m_UseFullRotation;

    private OvidVRInteractableItem toolInteractable;

    public override void SetUpTool(Tool _tool)
    {
        base.SetUpTool(_tool);

        rotatedParts.Add(m_RotatableObject);
        fullRotation = m_UseFullRotation;
        RotationAxis = Vector3.back;
        
    }
}
