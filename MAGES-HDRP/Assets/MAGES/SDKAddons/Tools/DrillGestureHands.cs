using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.toolManager.tool;
using ovidVR.toolManager;
using System;
using OvidVRPhysX;

public class DrillGestureHands : GestureHands {

    public Material[] materials;
    public GameObject[] gameObjectsToChange;

    private OvidVRInteractableItem toolInteractable;

    public void SetRotatingParts(params GameObject[] GO)
    {
        rotatedParts.AddRange(GO);
    }

    public void removeRotatedParts(params GameObject[] GO)
    {
        foreach (GameObject item in GO)
        {
            rotatedParts.Remove(item);
        }
    }

    public override void EndToolGesture()
    {
        GetComponent<Renderer>().material = materials[0];

        base.EndToolGesture();
    }

    private void Start()
    {
        fullRotation = true;
        SetRotatingParts(gameObject);

        toolInteractable = tool.toolGameobject.GetComponent<OvidVRInteractableItem>();
        if (toolInteractable == null)
            Debug.LogError("No OvidVRInteractableItem found in tool: " + gameObject.name);
    }

    public void SetPulse(float setPulse)
    {
        pulseValue = setPulse;
    }

    #region User Functions
    public void ChangeToolMaterial(int _materialNumber)
    {
        // If tool is not grabbed and called to change to bloody material - ignore
        if (!toolInteractable.IsAttached && _materialNumber == 1)
            return;

        if (gameObjectsToChange.Length != 0 || materials.Length != 0)
        {
            foreach (GameObject gO in gameObjectsToChange)
            {
                gO.GetComponent<Renderer>().material = materials[_materialNumber];
            }
        }
        else
        {
            Debug.LogError(gameObject.name + ": No GameObjects or Materials to Change Found!");
        }
    }
    #endregion
}
