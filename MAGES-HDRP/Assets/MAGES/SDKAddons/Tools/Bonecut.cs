using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.toolManager.tool;

public class Bonecut : MonoBehaviour {

    Tool Bonesaw;
    Material cleanBlade;
    public GameObject blade;
    // Use this for initialization
    void Start()
    {
        Bonesaw = new Tool(ToolsEnum.Bonesaw.ToString(), this.gameObject, true);

        cleanBlade = Resources.Load("MAGESres\\Models\\Tools\\BonesawMaterials\\BonesawClean\\BonesawCleanMaterial", typeof(Material)) as Material;

        //Clean CleanBlade
        Bonesaw.SetDeselectAction(CleanBlade);
    }



    public void CleanBlade()
    {
        blade.GetComponent<Renderer>().material = cleanBlade;
    }
}
