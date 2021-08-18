using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActionDetailedAnalytics : MonoBehaviour {

    Transform allTexts,wTexts,cTexts,eTexts;
    public float prevY=0,offset=-121;
    public static int wFirst, eFirst, cFirst;

    ActionsAnalytics parentScript;
    ButtonBehavior buttonBehaviorScript;

    List<string> _warnings=new List<string>(), _errors = new List<string>(), _criticals = new List<string>();

    public string _pathToError= "MAGESres/UI/Analytics/EndOfOperation/ErrorText", _pathtoWarning= "MAGESres/UI/Analytics/EndOfOperation/WarningText", _pathToCritical= "MAGESres/UI/Analytics/EndOfOperation/CriticalText";

    void Awake()
    {   
        allTexts = gameObject.transform.Find("ScrollingArea/ActionsContainer/ActionDetails/AllTexts");
        wTexts = allTexts.Find("WTexts");
        cTexts = allTexts.Find("CTexts");
        eTexts = allTexts.Find("ETexts");
        
    }

    void OnDisable()
    {
        if (!parentScript)
        {
            if (GameObject.Find("EndOfSessionAnalytics(Clone)").GetComponent<ActionsAnalytics>()) {
                parentScript=GameObject.Find("EndOfSessionAnalytics(Clone)").GetComponent<ActionsAnalytics>();
            }
        }
        parentScript.SetAllActionButtonsInteractivity(true);
    }

    /// <summary>
    /// Set the script reference from the OperationEnd Analytics. It is a separate gameObject
    /// </summary>
    /// <param name="_parentScript"></param>
    public void SetOperationEndAnalyticsScriptReference(ActionsAnalytics _parentScript)
    {
        parentScript = _parentScript;
    }
    public void SetButtonBehaviorScriptReference(ButtonBehavior _buttonBehaviorScript)
    {
        buttonBehaviorScript = _buttonBehaviorScript;
    }

    public void PlotData(Dictionary<string, int> _action,string type)
    {
        if (type == "criticals")
        {
            cFirst = 0;
            foreach (string var in _action.Keys)
            {
                if (_criticals.Contains(var))
                {
                    if (GameObject.Find("CriticalText(Clone)"))
                    {
                        GameObject.Find("CriticalText(Clone)").GetComponent<Text>().text = "x" + _action[var] + " " + var;
                    }
                }
                if (!_criticals.Contains(var))
                {
                    GameObject cText = Resources.Load(_pathToCritical, typeof(GameObject)) as GameObject;
                    _criticals.Add(var);
                    cText = Instantiate(cText, cTexts);
                    if (cFirst == 0)
                    {
                        cFirst++;
                    }
                    else
                    {
                        prevY = -120;
                        prevY += offset;
                    }
                    cText.GetComponent<RectTransform>().anchoredPosition = new Vector3(47, prevY, 0);
                    cText.GetComponent<Text>().text = "x" + _action[var] + " " + var;
                }

            }                          
        }
        if (type == "errors")
        {
            eFirst = 0;
            foreach (string var in _action.Keys)
            {
                if (_errors.Contains(var))
                {
                    if (GameObject.Find("ErrorText(Clone)"))
                    {
                        GameObject.Find("ErrorText(Clone)").GetComponent<Text>().text = "x" + _action[var] + " " + var;
                    }
                }

                if (!_errors.Contains(var))
                {
                    GameObject eText = Resources.Load(_pathToError, typeof(GameObject)) as GameObject;
                    _errors.Add(var);
                    eText = Instantiate(eText, eTexts);
                    if (eFirst == 0)
                    {
                        eFirst++;
                    }
                    else
                    {
                        prevY = -120;
                        prevY += offset;
                    }
                    eText.GetComponent<RectTransform>().anchoredPosition = new Vector3(47, prevY, 0);
                    eText.GetComponent<Text>().text = "x" + _action[var] + " " + var;
                }
            }               
        }
        if (type == "warnings")
        {
            wFirst = 0;

            foreach (string var in _action.Keys)
            {
                if (_warnings.Contains(var))
                {
                    if (GameObject.Find("WarningText(Clone)"))
                    {
                        GameObject.Find("WarningText(Clone)").GetComponent<Text>().text = "x" + _action[var] + " " + var;
                    }
                }
                if (!_warnings.Contains(var))
                {
                    GameObject wText = Resources.Load(_pathtoWarning, typeof(GameObject)) as GameObject;
                    _warnings.Add(var);
                    wText = Instantiate(wText, wTexts);
                    if (wFirst == 0)
                    {
                        wFirst++;
                    }
                    else
                    {
                        prevY = -120;
                        prevY += offset;
                    }
                    wText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, prevY, 0);
                    wText.GetComponent<Text>().text = "x" + _action[var] + " " + var;

                }
            }               
        }
    }
}
