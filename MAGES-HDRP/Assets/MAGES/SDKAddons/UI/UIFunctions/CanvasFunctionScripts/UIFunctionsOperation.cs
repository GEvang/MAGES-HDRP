using ovidVR.AnalyticsEngine;
using ovidVR.CustomEventManager;
using ovidVR.GameController;
using ovidVR.OperationAnalytics;
using ovidVR.sceneGraphSpace;
using ovidVR.UIManagement;
using ovidVR.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIFunctionsOperation : MonoBehaviour {
    
    private enum OperationAlternativeApproaches { Uncemented, Cemented };

    private int activeLesson, totalNumberOfLessons;
    private UIButton acceptLessonButton;

    [SerializeField, Tooltip("Drag and Drop Here the Button that jumps to selected lesson")]
    private GameObject lessonAccept;

    struct VisualizeLessons
    {
        public bool accessLesson;
        public string name;
        public Sprite sprite;
    }

    List<VisualizeLessons> lessonList;

    License.LicenseType licenseType = License.LicenseType.None;

    /// <summary>
    /// Do NOT Use Awake or Enable. Only Start.
    /// Initialization should be done when the UI appears on the scene
    /// </summary>
    public void Start()
    {
        if (totalNumberOfLessons == 1)
            this.enabled = false;

        activeLesson = 0;
        
        totalNumberOfLessons = Operation.Get.GetNumberOfLessons();
        
        lessonList = new List<VisualizeLessons>();

        licenseType = sceneGraph.GetLicenseType();

        // If no Lesson Accept button is given, then there is no Timeline to this UI
        if (lessonAccept != null)
        {
            if (lessonAccept.GetComponent<UIButton>() != null)
                acceptLessonButton = lessonAccept.GetComponent<UIButton>();

            StartCoroutine("SetUpLessons");
        }        
    }
    
    // Operation Timeline ------------------------------------

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

    public void ChangeLesson(bool _next)
    {
        if (lessonList == null)
            return;

        int currLesson = activeLesson;

        if (_next && activeLesson < totalNumberOfLessons - 1)
            ++activeLesson;
        else if (!_next && activeLesson > 0)
            --activeLesson;
        else
            return;

        // MUST BE TRANSLATED
        acceptLessonButton.SetButtonSecondaryExplanation(true, lessonList[activeLesson].name, lessonList[activeLesson].sprite);

        // if Demo
        if(!lessonList[activeLesson].accessLesson && licenseType == License.LicenseType.Demo)
        {
            if (acceptLessonButton.isActive)
                acceptLessonButton.SetButtonActivity(false, LanguageTranslator.LessonNABtnExp);
        }
        else
        {
            if(!acceptLessonButton.isActive)
                acceptLessonButton.SetButtonActivity(true);
        }
    }

    public void JumpToSelectedLesson()
    {
        // extra Security
        if(lessonList[activeLesson].accessLesson || licenseType != License.LicenseType.Demo)
            new JumpLessons(activeLesson);
    }

    /// <summary>
    /// Must be used ONLY from UIOperationStart
    /// </summary>
    public void StartOperation()
    {
        if(gameObject.name == "UIOperationStart")
            Operation.Get.Perform();
    }

    /// <summary>
    /// Set Difficulty for Operation (Meduim Difficulty: Depricated)
    /// </summary>
    /// <param name="_isTraininglevel">TRUE if Training Difficulty else FALSE for Normal Difficulty</param>
    public void SetUpOperationDifficulty(bool _isTraininglevel)
    {
        if (_isTraininglevel)
            Operation.Get.SetOperationDifficulty(Difficulty.Easy);
        else
            Operation.Get.SetOperationDifficulty(Difficulty.Hard);
    }

    public void SetOperationEndScore(GameObject _exitButton)
    {
        if (_exitButton == null || _exitButton.GetComponent<UIButton>() == null)
            return;

        UIButton buttonScript = _exitButton.GetComponent<UIButton>();

        float totalScore = 0;
        List<UserAction> allUserActions = new List<UserAction>();

        allUserActions = UserPathTracer.Get.GetUserActions().Values.ToList();

        foreach (UserAction ua in allUserActions)
        {
            if (ua.name == "Operation Start" || ua.name == "Operation End") { continue; }

            totalScore += ua.score;
        }
        int meanScore = (int) totalScore/allUserActions.Count;
        buttonScript.SetButtonSecondaryExplanation(true, UIManagement.GetUIMessage(LanguageTranslator.ScoreText) + meanScore.ToString());
    }

    // Rest --------------------------------------------------

    public void ApplicationQuit()
    {
        ovidVR.Exit.ExitApplication.Exit();
    }

    public void ApplicationRestart()
    {
        ovidVR.Exit.ExitApplication.Restart();
    }

    public void SelectApproach(string _approach)
    {
        try
        {
            OperationAlternativeApproaches enumerable = (OperationAlternativeApproaches)System.Enum.Parse(typeof(OperationAlternativeApproaches), _approach);

            switch (enumerable)
            {
                case OperationAlternativeApproaches.Uncemented:
                    EventManager.TriggerEvent(ScenegraphTraverse.GetCurrentAction().name);
                    break;
                case OperationAlternativeApproaches.Cemented:
                    AlternativePath.SetAlternativePath(0);
                    AlternativePath.SetAlternativePath(1);
                    AlternativePath.SetAlternativePath(2);
                    AlternativePath.SetAlternativePath(3);
                    AlternativePath.SetAlternativePath(4);

                    if (OvidVRControllerClass.NetworkManager.GetIsInNetwork())
                    {
                        NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.changeAlternativePathCustom, ScenegraphTraverse.GetCurrentAction().name, "01234");
                        UNETChat.u.SendMessage(m);
                    }

                    EventManager.TriggerEvent(ScenegraphTraverse.GetCurrentAction().name);
                    break;
            }

        }
        catch (System.Exception)
        {
            Debug.LogError("Parse: Can't convert " + _approach + " to enum, please check the spell.");
        }

    }

    /**
     * Select Approach from UI non operation specific.
     * Has not been tested on Multi-Player.
    **/
    public void SelectApproachNonOperationSpecific(int _approach)
    {
        try
        {
            GameObject newCase;
            switch (_approach)
            {
                case -1:
                    AlternativePath.SetAlternativePath(-1);
                    newCase = new GameObject("Case0");
                    Operation.Get.Perform();
                    break;
                case 0:
                    newCase = new GameObject("Case1");
                    AlternativePath.SetAlternativePath(0);
                    AlternativePath.SetAlternativePath(1);
                    AlternativePath.SetAlternativePath(2);

                    if (OvidVRControllerClass.NetworkManager.GetIsInNetwork())
                    {
                        NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.changeAlternativePathCustom, ScenegraphTraverse.GetCurrentAction().name, "01234");
                        UNETChat.u.SendMessage(m);
                    }
                    Operation.Get.Perform();
                    break;
                default:
                    AlternativePath.SetAlternativePath(-1);
                    Operation.Get.Perform();
                    break;
            }

        }
        catch (System.Exception ThisException)
        {
            Debug.LogError(ThisException.Message);
        }
    }

    public void SetFollowingNotificationMessage(LanguageTranslator _message)
    {
        UIManagement.SpawnNotificationUI(NotificationUITypes.UINotification, _message, 7.0f, "", false);
    }

    // Enumeratiors -----------------------------------------

    private IEnumerator SetUpLessons()
    {
        // Set Up Button Image after the animation is done
        yield return new WaitForSeconds(2f);

        int lessonCounter = 0;
        Lesson[] operationLessons = Operation.Get.gameObject.GetComponentsInChildren<Lesson>();
        foreach (Lesson lesson in operationLessons)
        {
            VisualizeLessons vl = new VisualizeLessons();
            vl.accessLesson = lesson.accessLesson;
            vl.name = "L" + (lessonCounter + 1).ToString() + ": " + lesson.name;
            vl.sprite = Resources.Load("MAGESres/UI/ApplicationSpecific/LessonImages/Lesson" + lessonCounter.ToString(), typeof(Sprite)) as Sprite;
            lessonList.Add(vl);
            ++lessonCounter;
        }

        // leson name is taken from scenegraph leave as is, no need to be translated from UILanguageImporter
        acceptLessonButton.SetButtonSecondaryExplanation(true, lessonList[0].name, lessonList[0].sprite);
    }
}
