using System.Collections.Generic;
using UnityEngine;
using ovidVR.OperationAnalytics;
using ovidVR.AnalyticsEngine;
using UnityEngine.UI;


public class ActionsAnalytics : MonoBehaviour
{
    private float actionsOffset = -0.47f, prevY, firstY;
    Transform actionsSceneTransfromPrefab, actionName, warnings, errors, criticals;
    [HideInInspector]
    public List<AnalyticsUserData> sessionAnalytics = new List<AnalyticsUserData>();

    private List<ButtonBehavior> allActionButtons;

    public string _pathToPrefab= "MAGESres/UI/Analytics/EndOfOperation/Action";

    private void Start()
    {
        actionsSceneTransfromPrefab = transform.Find("ScrollingArea/ActionsContainer/Actions");

        allActionButtons = new List<ButtonBehavior>();

        //Get session Analytics user data from Analytics API
        CreatePrefabsAnalytics();
    }

    void CreatePrefabsAnalytics()
    {
        int action_No = 0;
        sessionAnalytics = AnalyticsMain.GetAllUserAnalyticsData();
        foreach (AnalyticsUserData var in sessionAnalytics)
        {
            if (var.actionName == "Operation Start")
                continue;
            if (var.actionName == "Operation End")
                break;

           action_No++;
            GameObject actionTemplate = Resources.Load(_pathToPrefab, typeof(GameObject)) as GameObject;
            actionTemplate = Instantiate(actionTemplate, actionsSceneTransfromPrefab);
            actionTemplate.name = "Action" + action_No;

            allActionButtons.Add(actionTemplate.GetComponent<ButtonBehavior>());

            string actionNameTemplate = var.actionName + " " + var.score + "%";
            actionName = actionTemplate.transform.Find("ActionName");
            actionName.GetComponent<Text>().text = actionNameTemplate;

            string actionWarningsTemplate = "Warnings: " + var.errorUserData.totalWarnings;
            warnings = actionTemplate.transform.Find("Warnings");
            warnings.GetComponent<Text>().text = actionWarningsTemplate;

            string actionErrorsTemplate = "Errors: " + var.errorUserData.totalErrors;
            errors = actionTemplate.transform.Find("Errors");
            errors.GetComponent<Text>().text = actionErrorsTemplate;

            string actionCriticalsTemplate = "Critical: " + var.errorUserData.totalCriticals;
            criticals = actionTemplate.transform.Find("Critical Errors");
            criticals.GetComponent<Text>().text = actionCriticalsTemplate;

            //Offset
            if (action_No == 1)
            {
                firstY = 0.721f;
                prevY = firstY;
            }
            if (action_No > 1)
            {
                prevY += actionsOffset;
                actionTemplate.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, prevY, 0);
            }
        }
    }

    public void SetAllActionButtonsInteractivity(bool _areInteractive)
    {
        if (allActionButtons == null || allActionButtons.Count == 0)
            return;
        
        foreach (ButtonBehavior bb in allActionButtons)
            bb.ButtonInteractivity(_areInteractive);
    }
}

