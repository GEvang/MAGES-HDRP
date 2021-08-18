using ovidVR.sceneGraphSpace;
using ovidVR.UIManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarManager : MonoBehaviour {

    private Image slider;
    private Transform endOfProgressAnimation;
    public bool instantProgress=false,animateBar=true;

    //Developper has to set these vars to wanted outcome
    public int seconds = 1;
    private float setProgress = 0.0f;

    private float percentage;
    public GameObject progressBarPercentage;
    public GameObject baranimation;
    private IEnumerator coroutine;

    private int numberOfActions;
    private float actionStep;
    private int actionStepPercentage;

    // Actions
    public GameObject currentAction;
    public GameObject previousAction;
    public GameObject nextAction;
    public GameObject timerAction;
    float actionTime = 0f;

    bool isCoroutineActionNameRunning = false;

    int timerFirst = 0;

    // Match ENG Action nanes to their respective Keys to be displayed in all languages
    // by default scenegraph stores it in ENG
    private Dictionary<string, LanguageTranslator> actionNamesTranslated;

    void Start()
    {
        if (!currentAction || !previousAction || !nextAction || !timerAction)
        {
            Debug.LogError("One of Four Action Gameobjects are Empty! Disabling Script");
            this.enabled = false;
            return;
        }

        // On Operation Start disable them - too much info on screen
        currentAction.GetComponent<Text>().text = "";
        previousAction.GetComponentInChildren<Text>().text = "";
        nextAction.GetComponentInChildren<Text>().text = "";
        currentAction.SetActive(false);
        previousAction.SetActive(false);
        nextAction.SetActive(false);

        slider = gameObject.GetComponent<Image>();
        endOfProgressAnimation = baranimation.transform.Find("FillCompleteAnimation");
        StartCoroutine(DelayScenegraph());
    }

    IEnumerator DelayScenegraph()
    {

        yield return new WaitUntil(() => ScenegraphTraverse.GetNumberOfActions() > 1);

        numberOfActions = ScenegraphTraverse.GetNumberOfActions();
        actionStep = (1 / (float)numberOfActions);
        actionStepPercentage = Mathf.RoundToInt(actionStep * 100);

        Operation.Get.AddActionOnPerform(PerformAction);
        Operation.Get.AddActionOnUndo(UndoAction);

        // Initialize all ActionNames MANUALLY
        actionNamesTranslated = new Dictionary<string, LanguageTranslator>();
        actionNamesTranslated.Add("Write patient's Name on swab Label", LanguageTranslator.WritePatientName);
        actionNamesTranslated.Add("Hand Disinfection", LanguageTranslator.HandDisinfection);
        actionNamesTranslated.Add("Wear Gown", LanguageTranslator.WearGown);
        actionNamesTranslated.Add("Gown Question", LanguageTranslator.GownQuestion);
        actionNamesTranslated.Add("Select and wear your Mask", LanguageTranslator.SelectWearMask);
        actionNamesTranslated.Add("Mask Placement Question", LanguageTranslator.MaskPlacementQuestion);
        actionNamesTranslated.Add("Wear Goggles", LanguageTranslator.WearGoggles);
        actionNamesTranslated.Add("Gloves and Gown Question", LanguageTranslator.GlovesGownQuestion);
        actionNamesTranslated.Add("Wear Plastic Gloves", LanguageTranslator.WearGloves);
        actionNamesTranslated.Add("Instruct Patient to Blow her Nose", LanguageTranslator.PatientBlowNose);
        actionNamesTranslated.Add("Tilt Patient's head backwards", LanguageTranslator.TiltHeadBack);
        actionNamesTranslated.Add("Obtain Nasopharyngeal Swab Specimen", LanguageTranslator.ObtainNasoSpecimen);
        actionNamesTranslated.Add("Place Swab Into Container", LanguageTranslator.SwabIntoContainer);
        actionNamesTranslated.Add("Peel off Left Glove", LanguageTranslator.OffLeftGlove);
        actionNamesTranslated.Add("Remove Right Glove", LanguageTranslator.OffRightGlove);
        actionNamesTranslated.Add("Dispose Gloves to bin", LanguageTranslator.DisposeGloves);
        actionNamesTranslated.Add("Pull Away Gown", LanguageTranslator.PullAwayGown);
        actionNamesTranslated.Add("Discard Gown to bin", LanguageTranslator.DiscardGown);
        actionNamesTranslated.Add("Discard Mask to bin", LanguageTranslator.DiscardMask);
        actionNamesTranslated.Add("Discard Goggles to bin", LanguageTranslator.DiscardGogglesAction);
        actionNamesTranslated.Add("Repeat Action", LanguageTranslator.RepeatActionAction);
        actionNamesTranslated.Add("Remove Goggles", LanguageTranslator.RemoveGogglesAction);
        actionNamesTranslated.Add("Operation Start", LanguageTranslator.OperationStartAction);
        actionNamesTranslated.Add("Operation End", LanguageTranslator.OperationEndAction);

    }

    void PerformAction()
    {
        setProgress += actionStep;
        SetProgressAnimation(seconds, setProgress);

        // Action names ----------

        if (!currentAction.activeSelf)
        {
            currentAction.SetActive(true);
            previousAction.SetActive(true);
            nextAction.SetActive(true);
        }

        if (isCoroutineActionNameRunning)
            return;
        else
            StartCoroutine("UpdateActionNames");  
    }

    void UndoAction()
    {
        setProgress -= actionStep;
        SetProgressAnimation(seconds, setProgress);

        // Action names ----------

        if (!currentAction.activeSelf)
        {
            currentAction.SetActive(true);
            previousAction.SetActive(true);
            nextAction.SetActive(true);
        }

        if (isCoroutineActionNameRunning)
            return;
        else
            StartCoroutine("UpdateActionNames");
    }

    private IEnumerator UpdateActionNames()
    {
        isCoroutineActionNameRunning = true;

        string prev, next, curr;

        float timeActionTook = Time.time - actionTime;
        actionTime = Time.time;

        currentAction.GetComponent<Animation>().Play();
        previousAction.GetComponentInChildren<Animation>().Play();
        nextAction.GetComponentInChildren<Animation>().Play();

        if (timerFirst == 0 || timerFirst == 1)
        {
            timerFirst++;
        }
        if (timerFirst > 1)
        {
            timerAction.GetComponentInChildren<Animation>().Play();
            timerAction.GetComponentInChildren<Text>().text = ((int)timeActionTook).ToString() + " Sec";
        }

        yield return new WaitForSeconds(0.4f);

        // Disable on Operation Start OR End Action exactly during the animation is at fade timeline
        if (ScenegraphTraverse.GetCurrentAction().name == "Operation End" ||
            ScenegraphTraverse.GetCurrentAction().name == "Operation Start")
        {
            currentAction.SetActive(false);
            previousAction.SetActive(false);
            nextAction.SetActive(false);

            yield break;
        }

        if (ScenegraphTraverse.GetPreviousAction())
            prev = ScenegraphTraverse.GetPreviousAction().name;
        else
            prev = "";

        if (ScenegraphTraverse.GetNextAction())
            next = ScenegraphTraverse.GetNextAction().name;
        else
            next = "";

        curr = ScenegraphTraverse.GetCurrentAction().name;

        // The lazy and surely working way :D
        int parse = 0;

        foreach (KeyValuePair<string, LanguageTranslator> actionName in actionNamesTranslated)
        {
            string noSpaces = prev.Replace(" ", "");

            if (String.Equals(prev.Replace(" ", ""), actionName.Key.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
            {
                prev = InterfaceManagement.Get.GetUIMessage(actionName.Value);
                ++parse;
            }

            if (String.Equals(next.Replace(" ", ""), actionName.Key.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
            {
                next = InterfaceManagement.Get.GetUIMessage(actionName.Value);
                ++parse;
            }

            if (String.Equals(curr.Replace(" ", ""), actionName.Key.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
            {
                curr = InterfaceManagement.Get.GetUIMessage(actionName.Value);
                ++parse;
            }

            if (parse == 3)
                break;
        }

        previousAction.GetComponentInChildren<Text>().text = prev;
        nextAction.GetComponentInChildren<Text>().text = next;

        currentAction.GetComponent<Text>().text = curr;

        isCoroutineActionNameRunning = false;
    }

    void Update()
    {
        if (slider.fillAmount == 1)
        {
            if (endOfProgressAnimation)
            {
                if ( animateBar == true)
                {
                    endOfProgressAnimation.gameObject.SetActive(true);
                }
                else
                {
                    endOfProgressAnimation.gameObject.SetActive(false);
                }
            }
        }
        if (slider.fillAmount != 1)
        {
            if (endOfProgressAnimation)
            {
                if (animateBar == true)
                {
                    endOfProgressAnimation.gameObject.SetActive(false);
                }
            }
        }
    }

    public float GetProgress()
    {
        return Mathf.Round(slider.fillAmount * 100)/100;
    }

    public IEnumerator SliderOverTime(float seconds,float targetFrom, float targetMax)
    {
        float animationTime = 0.0f;

        if (instantProgress)
        {
            slider.fillAmount = targetMax;
            yield break;
        }

        if (targetMax == 0.0f)
        {
            slider.fillAmount = 0.0f;
            yield break;
        }
        while (animationTime < seconds)
        {
            animationTime += Time.deltaTime;
            float fillTime = animationTime / seconds;
            slider.fillAmount = Mathf.Lerp(targetFrom, targetMax, fillTime);
            yield return null;
        }
    }

    /// <summary>
    /// Input must be normalized
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="to"></param>
    void SetProgressAnimation(int seconds, float to)
    {
        float from = GetProgress();
        coroutine = SliderOverTime(seconds, from, to);
        StartCoroutine(coroutine);
        percentage = to * 100;

        if (to == setProgress)
        {
            if (from < to)
            {
                baranimation.transform.Find("PercentageUp").gameObject.SetActive(true);

                //Move parent of animation
                baranimation.GetComponent<RectTransform>().localPosition = new Vector3(2 * (GetComponent<ProgressBarManager>().GetProgress() + 0.35f), baranimation.GetComponent<RectTransform>().localPosition.y, baranimation.GetComponent<RectTransform>().localPosition.z);

                baranimation.transform.Find("PercentageUp").GetComponent<Text>().text = "+" + actionStepPercentage.ToString() + "%";

                StartCoroutine(CheckAnimationTime(baranimation.transform.Find("PercentageUp").GetComponent<Animator>(), baranimation.transform.Find("PercentageUp").gameObject));

            }
            else
            {
                baranimation.transform.Find("PercentageDown").gameObject.SetActive(true);

                //Move parent of animation
                baranimation.GetComponent<RectTransform>().localPosition = new Vector3(2 * (GetComponent<ProgressBarManager>().GetProgress() + 0.2f), baranimation.GetComponent<RectTransform>().localPosition.y, baranimation.GetComponent<RectTransform>().localPosition.z);

                baranimation.transform.Find("PercentageDown").GetComponent<Text>().text = "-" + actionStepPercentage.ToString() + "%";

                StartCoroutine(CheckAnimationTime(baranimation.transform.Find("PercentageDown").GetComponent<Animator>(), baranimation.transform.Find("PercentageDown").gameObject));
            }

            if (percentage > 100)
            {
                percentage = 100;
            }
            else if (percentage < 0)
            {
                percentage = 0;
            }
            //Update Static Percentage
            progressBarPercentage.transform.Find("Text").GetComponent<Text>().text = ((int)percentage).ToString() + "%";

        }

    }

    IEnumerator CheckAnimationTime(Animator anim, GameObject go)
    {
        while (anim.gameObject.activeSelf && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.99f)
        {
            yield return null;
        }
        go.SetActive(false);
        yield break;
    }
}
