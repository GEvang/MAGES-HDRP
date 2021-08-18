using ovidVR.AnalyticsEngine;
using ovidVR.sceneGraphSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.UIManagement;
using ovidVR.OperationAnalytics;
using ovidVR.CustomEventManager;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;

public class InterfaceFunctionTriggers : MonoBehaviour {

    private bool isRecording;
    void Awake()
    {
        isRecording = false;
    }

    // BUTTON INITIALIZE FUNCTIONS ------------------------

    public void SetOperationEndScore(GameObject _ui)
    {
        float totalScore = 0;
         
        if (!_ui)
        {
            Debug.LogError("Bo button given");
            return;
        }

        if (AnalyticsManager.OperationScoringSystem == ScoringSystem.Skills)
        {
            //Hiding score
            /*
            float skillScore=0,totalScorePoints=0;
            int skill1_total_points = 3, skill2_total_points = 5, skill3_total_points = 4, skill4_total_points = 3, skill5_total_points = 4, total_skills_points = 19;

            Dictionary<string, float> allSkillsPoints = AnalyticsManager.results;
            foreach (KeyValuePair<string, float> attachStat in allSkillsPoints)
            {
                switch (attachStat.Key)
                {
                    case "Skill 7070.16.6.1":
                        skillScore = (attachStat.Value / skill1_total_points);
                        break;
                    case "Skill 7070.16.6.2":
                        skillScore = (attachStat.Value / skill2_total_points);
                        break;
                    case "Skill 7070.16.6.3":
                        skillScore = (attachStat.Value / skill3_total_points);
                        break;
                    case "Skill 7070.16.6.4":
                        skillScore = (attachStat.Value / skill4_total_points);
                        break;
                    case "Skill 7070.16.6.5":
                        skillScore = (attachStat.Value / skill5_total_points);
                        break;
                    default:
                        Debug.Log("Skill name not found. Probably misspelled when was assigned.");
                        break;
                }
                //totalScorePoints += skillScore;
                totalScorePoints += attachStat.Value;
            }
            totalScore= totalScorePoints/total_skills_points;
            totalScore = Mathf.Clamp(totalScore, 0, 100)*100;*/

            //Not using Languge Translator because in WGU module, we use only ENG
            _ui?.GetComponent<ButtonBehavior>().UpdateButtonText("ANALYTICS");
        }
        else
        {
            List<UserAction> allUserActions = new List<UserAction>();

            allUserActions = UserPathTracer.Get.GetUserActions().Values.ToList();

            foreach (UserAction ua in allUserActions)
            {
                if (ua.name == "Operation Start" || ua.name == "Operation End") { continue; }

                totalScore += ua.score;
            }
            int meanScore = (int)totalScore / allUserActions.Count;
            meanScore = Mathf.Clamp(meanScore, 0, 100);

            _ui?.GetComponent<ButtonBehavior>().UpdateButtonText(InterfaceManagement.Get.GetUIMessage(LanguageTranslator.AnalyticsScore) + ": " + meanScore.ToString() + "%");
        }
    }

    // MODULE ---------------------------------------------

    public void StartOperation()
    {

        if (gameObject.name.Contains("OperationStart"))
            Operation.Get.Perform();
    }

    public void NextAction()
    {
        if (ScenegraphTraverse.GetCurrentAction().name == "Operation End")
            return;

        Operation.Get.SkipCurrentAction();
    }

    public void PreviousAction()
    {
        if (ScenegraphTraverse.GetCurrentAction().name == "Operation Start")
            return;

        Operation.Get.Undo();
    }

    public void SetUpOperationDifficulty(bool _isTraininglevel)
    {
        if (_isTraininglevel)
            Operation.Get.SetOperationDifficulty(Difficulty.Easy);
        else
            Operation.Get.SetOperationDifficulty(Difficulty.Hard);
    } 

    public void ApplicationQuit(GameObject _ui)
    {
        if (!_ui)
        {
            Debug.LogError("Bo button given");
            return;
        }

        Sprite acceptSprite = Resources.Load("MAGESres/UI/ApplicationGeneric/Sprites/AcceptWhite", typeof(Sprite)) as Sprite;

        if (acceptSprite)
            _ui?.GetComponent<ButtonBehavior>().UpdateButtonSprite(acceptSprite);
        _ui?.GetComponent<ButtonBehavior>().UpdateButtonText(InterfaceManagement.Get.GetUIMessage(LanguageTranslator.AreYouSureButton));
        _ui?.GetComponent<ButtonBehavior>().UpdateButtonFunctionTrigger(() => 
        {
            DestroyThis();
            ovidVR.Exit.ExitApplication.Exit();
        });
    }

    public void ApplicationRestart(GameObject _ui)
    {
        if (!_ui)
        {
            Debug.LogError("Bo button given");
            return;
        }

        Sprite acceptSprite = Resources.Load("MAGESres/UI/ApplicationGeneric/Sprites/AcceptWhite", typeof(Sprite)) as Sprite;

        if (acceptSprite)
            _ui?.GetComponent<ButtonBehavior>().UpdateButtonSprite(acceptSprite);
        _ui?.GetComponent<ButtonBehavior>().UpdateButtonText(InterfaceManagement.Get.GetUIMessage(LanguageTranslator.AreYouSureButton));
        _ui?.GetComponent<ButtonBehavior>().UpdateButtonFunctionTrigger(() =>
        {
            DestroyThis();
            ovidVR.Exit.ExitApplication.Restart();
        });
    }

    // SPAWN ----------------------------------------------

    public void DestroyThis()
    {
        if (this.gameObject.GetComponent<InteractiveInterfaceSpawnAnimation>())
            this.gameObject.GetComponent<InteractiveInterfaceSpawnAnimation>().DestroyInterface();
        else
            Destroy(this.gameObject);
    }

    public void SpawnGameObject(GameObject _obj)
    {
        if (_obj != null)
            Instantiate(_obj);
    }

    public void SpawnNotificationMessage(LanguageTranslator _message)
    {
        InterfaceManagement.Get.SpawnDynamicNotificationUI(NotificationUITypes.UINotification, _message, 7.0f, "SpawnFromInterface");
    }

    public void SpawnWarningMessage(LanguageTranslator _message)
    {
        InterfaceManagement.Get.SpawnDynamicNotificationUI(NotificationUITypes.UIWarning, _message, 7.0f, "SpawnFromInterface");
    }

    public void SpawnErrorMessage(LanguageTranslator _message)
    {
        InterfaceManagement.Get.SpawnDynamicNotificationUI(NotificationUITypes.UIError, _message, 7.0f, "SpawnFromInterface");
    }

    public void SpawnOperationEndAnalytics(GameObject _ui)
    {
        _ui?.GetComponent<ButtonBehavior>().ButtonActivation(false);

        gameObject?.GetComponent<Animator>().SetBool("Reposition", true);

        Configuration SceneManagement = GameObject.Find("SCENE_MANAGEMENT").GetComponent<Configuration>();

        GameObject analyticsPrefab = SceneManagement.AnalyticsViewUI;

        if (analyticsPrefab == null)
            analyticsPrefab = (GameObject)Resources.Load("MAGESres/UI/Analytics/EndOfOperation/EndOfSessionAnalytics", typeof(GameObject));

        if (analyticsPrefab)
            Instantiate(analyticsPrefab);
    }

    public void SpawnOperationEndAnalyticsCustom(string _pathToPrefab)
    {
        gameObject.transform.Find("InterfaceContent/ViewAnalytics").GetComponent<ButtonBehavior>().ButtonActivation(false);
        gameObject?.GetComponent<Animator>().SetBool("Reposition", true);

        GameObject analyticsPrefab = (GameObject)Resources.Load(_pathToPrefab, typeof(GameObject));
        if (analyticsPrefab)
            Instantiate(analyticsPrefab);
    }

    public void SpawnCustomizationCanvas()
    {
        if (GameObject.Find("CharacterCustomizationCanvas(Clone)"))
            return;

        Destroy(this.gameObject);

        InterfaceManagement.Get.InterfaceRaycastActivation(true);

        Configuration SceneManagement = GameObject.Find("SCENE_MANAGEMENT").GetComponent<Configuration>();

        GameObject customizationCanvas = SceneManagement.CustomizationCanvasUI;

        if (customizationCanvas == null)
            customizationCanvas = Resources.Load("MAGESres/UI/OperationPrefabs/CharacterCustomizationCanvas", typeof(GameObject)) as GameObject;
        
        customizationCanvas = Instantiate(customizationCanvas);
        customizationCanvas.GetComponent<CustomizationManager>().resetAvatar = true;
    }

    public void SpawnOperationStart()
    {
        GameObject operationStart=InterfaceManagement.Get.SpawnUI("OperationStart");
    }

    // CUSTOM CODE FOR COOP ------------------------------

    public void DestroyGameObject(GameObject go)
    {
        GameObject gameObject = GameObject.Find(go.name+"(Clone)");

        if (gameObject)
            Destroy(gameObject);
    }

    public void DestroyAvailableSessions()
    {
        GameObject AvailableSessions = GameObject.Find("NetworkingUI(Clone)");
        if (AvailableSessions)
            Destroy(AvailableSessions);
    }

    public void DestroyCreateSession()
    {
        GameObject createSession = GameObject.Find("UICreateSession(Clone)");
        if (createSession)
            Destroy(createSession);
    }

    // EVENT MANAGER -------------------------------------

    public void EventManagerTrigger(string _key)
    {
        EventManager.TriggerEvent(_key);
    }
}
