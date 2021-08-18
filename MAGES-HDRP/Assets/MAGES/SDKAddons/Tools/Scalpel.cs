using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.toolManager.tool;

public class Scalpel : MonoBehaviour {


    Tool scalpel;
    // Use this for initialization
    void Start () {
        scalpel = new Tool(ToolsEnum.Scalpel.ToString(), this.gameObject,true);
    }

    // Update is called once per frame
    void Update () {

    }
    

}
