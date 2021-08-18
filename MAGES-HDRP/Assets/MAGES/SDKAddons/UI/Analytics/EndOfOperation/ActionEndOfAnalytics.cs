using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using ovidVR.AnalyticsEngine;
using UnityEngine.UI;
using ovidVR.Utilities;

public class ActionEndOfAnalytics : MonoBehaviour {

    string actionName, actionNameToBeTrimmed,actionAnalyticsName;
    Transform actionParent;
    ActionsAnalytics parentScript;
    List<AnalyticsUserData> _sessionAnalytics;
    GameObject actionDetailsTemplate;

    public string _pathToPrefab = "MAGESres/UI/Analytics/EndOfOperation/ActionDetails";

    public void InstantiateActionDetails()
    {
        if (!parentScript)
        {
            if (GameObject.Find("EndOfSessionAnalytics(Clone)").GetComponent<ActionsAnalytics>())
            {
                parentScript = GameObject.Find("EndOfSessionAnalytics(Clone)").GetComponent<ActionsAnalytics>();
            }
        }
        parentScript.SetAllActionButtonsInteractivity(false);

        actionDetailsTemplate = PrefabImporter.SpawnGenericPrefab(_pathToPrefab, parentScript.gameObject);
        ActionDetailedAnalytics detailedAnalyticsScript;

        //Find the Action's Analytics Structure from the list
        foreach (AnalyticsUserData var in _sessionAnalytics) { 

            actionNameToBeTrimmed = gameObject.transform.Find("ActionName").GetComponent<Text>().text;
            actionAnalyticsName = actionNameToBeTrimmed.Replace("%", "");
            actionAnalyticsName = Regex.Replace(actionAnalyticsName, @"[\d-]", "");
            actionAnalyticsName = actionAnalyticsName.Remove(actionAnalyticsName.Length - 1);

            if (var.actionName == actionAnalyticsName)
            {
                //Name of action
                if (actionDetailsTemplate.transform.Find("MaskedContent/ActionName"))
                    actionDetailsTemplate.transform.Find("MaskedContent/ActionName").GetComponent<Text>().text = actionAnalyticsName;

                //Set it for prefab instantiated
                detailedAnalyticsScript = actionDetailsTemplate.GetComponent<ActionDetailedAnalytics>();

                detailedAnalyticsScript.SetOperationEndAnalyticsScriptReference(parentScript);

                detailedAnalyticsScript.PlotData(var.errorUserData.criticals, "criticals");
                detailedAnalyticsScript.PlotData(var.errorUserData.errors, "errors");
                detailedAnalyticsScript.PlotData(var.errorUserData.warnings, "warnings");

            }
        }
    }

    void Awake () {
        actionName = gameObject.name;
        actionParent = gameObject.transform.root;
        parentScript = actionParent.GetComponent<ActionsAnalytics>();
        _sessionAnalytics = parentScript.sessionAnalytics;
    }
}
