using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.toolManager.tool;

public class Drill : MonoBehaviour {

    Tool drill;
    Material cleanDrillTip;
    public GameObject drillTip;
    // Use this for initialization
    void Start()
    {
        drill = new Tool(ToolsEnum.Drill.ToString(), this.gameObject, true);

        cleanDrillTip = Resources.Load("MAGESres\\Models\\Tools\\CleanMaterial\\CleanTipMaterial", typeof(Material)) as Material;

        //Clean Drill Tip
        drill.SetDeselectAction(CleanDrillTip);
    }

    public void CleanDrillTip()
    {
        drillTip.GetComponent<Renderer>().material = cleanDrillTip;
    }
}
