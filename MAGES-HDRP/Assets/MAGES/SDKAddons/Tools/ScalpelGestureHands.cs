using ovidVR.toolManager.tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ovidVR.toolManager;

public class ScalpelGestureHands : GestureHands
{

    public Material[] materials;
    public GameObject[] gameObjectsToChange;

    public override void EndToolGesture()
    {
        // Reset Scalpel Material
        ChangeScalpelMaterial(0);
        //base.EndToolGesture();
    }
    
    // Use this for initialization
    public override void SetUpTool(Tool _tool)
    {
        if (materials.Length == 0)
        {
            Debug.LogError(gameObject.name + ": No Materials Found!");
        }
        else
        {
            if (materials[0] == null)
            {
                Debug.LogError(gameObject.name + ": No Default Material Found!");
            }
            if (materials[1] == null)
            {
                Debug.LogError(gameObject.name + ": No Blood Material Found!");
            }
        }

        if (gameObjectsToChange.Length == 0)
        {
            Debug.LogError(gameObject.name + ": No GameObjects to Change Material Found!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ActionTriggerCollider")
        {
            ChangeScalpelMaterial(1);
        }
    }

    public void ChangeScalpelMaterial(int _materialNumber)
    {
        ToolsManager.GetTool("Scalpel").ReinitializeToolMaterials();

        foreach(GameObject gO in gameObjectsToChange)
        {
            gO.GetComponent<Renderer>().material = materials[_materialNumber];
        }
    }
}
