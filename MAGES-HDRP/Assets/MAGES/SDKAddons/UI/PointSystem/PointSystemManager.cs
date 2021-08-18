using ovidVR.AnalyticsEngine;
using ovidVR.OperationAnalytics;
using ovidVR.sceneGraphSpace;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointSystemManager : MonoBehaviour
{

    private Color sliderGoodColor, sliderMediumColor, sliderBadColor;

    class PointGraphs
    {
        public Slider psSlider;
        public Image psImage;
        public Text psText;
    }

    private PointGraphs progress, errors, actions;

    private Text actionTimer, currActionName, prevActionName, currLessonName;

    private Animation currActionImageAnim;

    private int currActionIndex = 0;

    private License.LicenseType licenseType = License.LicenseType.None;

    private bool isCoroutineRunning;

    public void StartPointSystem()
    {
        sliderGoodColor = new Color(0f, 0.823f, 0.721f); // blue-ish green oramaVR's color pallet
        sliderMediumColor = Color.yellow * 0.8f;
        sliderBadColor = Color.red * 0.9f;
        try
        {
            if (progress == null || errors == null || actions == null)
            {
                progress = new PointGraphs();
                Transform slide = transform.Find("Graphs/Progress");
                if (slide != null)
                {
                    progress.psSlider = slide.Find("Slider").gameObject.GetComponent<Slider>();
                    progress.psImage = slide.Find("Slider/Fill Area/Fill").gameObject.GetComponent<Image>();
                    progress.psText = slide.Find("Text").gameObject.GetComponent<Text>();
                }

                errors = new PointGraphs();
                slide = transform.Find("Graphs/Errors");
                if (slide != null)
                {
                    errors.psSlider = slide.Find("Slider").gameObject.GetComponent<Slider>();
                    errors.psImage = slide.Find("Slider/Fill Area/Fill").gameObject.GetComponent<Image>();
                    errors.psText = slide.Find("Text").gameObject.GetComponent<Text>();
                }

                actions = new PointGraphs();
                slide = transform.Find("Graphs/TotalActions");
                if (slide != null)
                {
                    actions.psSlider = slide.Find("Slider").gameObject.GetComponent<Slider>();
                    actions.psImage = slide.Find("Slider/Fill Area/Fill").gameObject.GetComponent<Image>();
                    actions.psText = slide.Find("Text").gameObject.GetComponent<Text>();
                }
            }

            if (currActionImageAnim == null)
                currActionImageAnim = transform.Find("VisualElem/CurrActionImage").gameObject.GetComponent<Animation>();

            actionTimer = transform.Find("DynamicTexts/ActionTimer").gameObject.GetComponent<Text>();
            prevActionName = transform.Find("DynamicTexts/PrevAction").gameObject.GetComponent<Text>();
            currActionName = transform.Find("DynamicTexts/CurrAction").gameObject.GetComponent<Text>();
            currLessonName = transform.Find("DynamicTexts/CurrLesson").gameObject.GetComponent<Text>();

            actionTimer.text = "0 Sec";
            prevActionName.text = "Previous Action Name";
            currActionName.text = "CurrentActionName";
            currLessonName.text = "Lessons";

            licenseType = sceneGraph.GetLicenseType();

            isCoroutineRunning = false;

            StartCoroutine(LateStart(0.5f));
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Operation.Get.RemoveActionOnPerform(UpdateCanvasPerform);
        Operation.Get.RemoveActionOnUndo(UpdateCanvasUndo);
    }

    void UpdateCanvasPerform()
    {
        if (this.gameObject.activeSelf)
        {
            ++currActionIndex;

            // Bad implementation - temp fix till Analytics + Gamification refactor
            if (isCoroutineRunning)
                StopAllCoroutines();
            StartCoroutine(LateUpdateScore(0.5f, true));
        }
    }

    void UpdateCanvasUndo()
    {
        if (this.gameObject.activeSelf)
        {
            --currActionIndex;

            if (isCoroutineRunning)
                StopAllCoroutines();
            StartCoroutine(LateUpdateScore(0.5f, false));
        }
    }

    // Enumerators -------------------------------------

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        currActionName.text = ScenegraphTraverse.GetCurrentAction().name;

        Operation.Get.AddActionOnPerform(UpdateCanvasPerform);
        Operation.Get.AddActionOnUndo(UpdateCanvasUndo);
    }

    IEnumerator LateUpdateScore(float waitTime, bool _performCall)
    {
        if (!currActionImageAnim.isPlaying)
            currActionImageAnim.Play();

        isCoroutineRunning = true;

        yield return new WaitForSeconds(waitTime);

        isCoroutineRunning = false;

        GameObject prevAction = ScenegraphTraverse.GetPreviousAction();

        if (prevAction == null)
        {
            // if Undo till Operation Start - Initialize values
            GameObject cuurAction = ScenegraphTraverse.GetCurrentAction();
            if (cuurAction != null && cuurAction.name == "Operation Start")
            {
                prevActionName.text = "Previous Action Name"; currActionName.text = "Operation Start"; currLessonName.text = "Lessons";
                actionTimer.text = "0 Sec"; progress.psSlider.value = 0; progress.psImage.color = Color.white;
                errors.psText.text = "0"; errors.psSlider.value = 0; errors.psImage.color = Color.white;
            }
            yield break;
        }

        int action = prevAction.transform.GetSiblingIndex();
        int stage = prevAction.transform.parent.GetSiblingIndex();
        int lesson = prevAction.transform.parent.parent.GetSiblingIndex();

        UserAction prevUserAction = UserPathTracer.Get.GetSpecificActionStats(lesson, stage, action);

        if (prevUserAction != null)
        {
            // SCORE ------------------------------------------
            float percent = 0;
            
            percent = prevUserAction.score;
            if (percent < 0.1f)
                progress.psSlider.value = 0.1f;
            else
                progress.psSlider.value = percent;

            if (percent < 0.5)
                progress.psImage.color = sliderBadColor;
            else if (percent < 0.8)
                progress.psImage.color = sliderMediumColor;
            else
                progress.psImage.color = sliderGoodColor;

            progress.psText.text = ((int)(percent )).ToString() + "%";

            // ERRORS (total acceptable errors: 20) -----------
            int totalErrors = prevUserAction.errors;

            float errorNormalized = totalErrors / 20f;
            if (errorNormalized > 1f)
                errorNormalized = 1f;

            errors.psSlider.value = errorNormalized;

            if (totalErrors < 2)
                errors.psImage.color = sliderGoodColor;
            else if (totalErrors < 8)
                errors.psImage.color = sliderMediumColor;
            else
                errors.psImage.color = sliderBadColor;

            errors.psText.text = totalErrors.ToString();

            // ACTONS --------------------------------------
            int totalActionIndex = ScenegraphTraverse.GetNumberOfActions();

            int currActionFixed = currActionIndex;
            if (currActionFixed > totalActionIndex)
                currActionFixed = totalActionIndex;

            actions.psText.text = "Actions: " + currActionFixed.ToString() + " / " + totalActionIndex.ToString();

            if (totalActionIndex != 0)
                actions.psSlider.value = (currActionIndex / (float)totalActionIndex);

            // DYNAMIC TEXTS -------------------------------

            currLessonName.text = "Lesson " + (Operation.Get.GetLessonID() + 1).ToString() + ": " + ScenegraphTraverse.GetCurrentLesson().name;
            currActionName.text = ScenegraphTraverse.GetCurrentAction().name;
      
            // Demo License: 10
            if (licenseType == License.LicenseType.Demo && prevAction.GetComponent<ActionProperties>().isDemoApplicable == "n")
            {
                prevActionName.text = "DEMO Action Skipped";
                actionTimer.text = " Time N/A";
            }
            else
            {
                prevActionName.text = prevUserAction.name;
                actionTimer.text = prevUserAction.time.ToString("0.00") + " Sec";
            }


        }
    }
}
