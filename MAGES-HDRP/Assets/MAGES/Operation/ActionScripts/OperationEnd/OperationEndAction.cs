using ovidVR.sceneGraphSpace;
using UnityEngine;
using System;
using ovidVR.OperationAnalytics;
using ovidVR.UIManagement;
using ovidVR.Utilities;

public class OperationEndAction : MonoBehaviour, IAction
{

    #region Action Variables
    private GameObject exit;
    #endregion

    #region IAction Variables
    private string AName;
    private GameObject acNode;
    public string ActionName
    {
        get { return AName; }
        set { AName = value; }
    }
    public GameObject ActionNode
    {
        get { return acNode; }
        set { acNode = value; }
    }
    private int altPath = -1;
    public int AlternativePath
    {
        get { return altPath; }
        set { this.altPath = value; }
    }
    #endregion

    #region IAction Functions
    public void Initialize()
    {
        InterfaceManagement.Get.InterfaceRaycastActivation(true);


        exit = PrefabImporter.SpawnActionPrefab("OperationEnd/OperationExit");
        // Call OperationFinished to export Analytics
        AnalyticsMain.OperationFinished();
    }

    public void InitializeHolograms()
    {
    }

    public void Perform()
    {
        DestroyUtilities.RemoteDestroy(exit);
    }

    public void Undo()
    {
        DestroyUtilities.RemoteDestroy(exit);
        InterfaceManagement.Get.InterfaceRaycastActivation(false);
    }

    public void SetNextModule(Action action)
    {
        // Empty
    }

    public void DifficultyRestrictions()
    {
        // Empty
    }
    #endregion

}
