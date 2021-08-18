using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.Utilities;
using ovidVR.Utilities.prefabSpawnManager;
using ovidVR.toolManager;
public class SpawnNetworkTools : MonoBehaviour {


    public object[] Toolset;
	// Use this for initialization
	void Start () {
        Destroy(GameObject.Find("ToolSet"));
        //ToolsManager.ToolsInstance.Clear();
        Toolset = Resources.LoadAll("MAGESres\\NetworkR\\Tools\\");
        if (OvidVRControllerClass.Get.isClient && !OvidVRControllerClass.Get.isServer)
            Invoke("D", .5f);
        else
            D();


    }

    private void D()
    {

        
        //ToolsManager.ToolsInstance.Clear();
        GameObject t;
        foreach (object g in Toolset)
        {
           
            try
            {
                t = PrefabImporter.SpawnGenericPrefab("MAGESres\\NetworkR\\Tools\\" + ((GameObject)g).name);
            }
            catch (System.Exception)
            {
                //break;
            }
        }
    }
}
