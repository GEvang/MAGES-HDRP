using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ovidVR.UIManagement;
using UnityEngine.Events;
using System;

public class DecisionInterface : MonoBehaviour
{
    [Header("Set to true to show first the Question Header and then the Buttons"), Tooltip("Make Sure to have the header Child Enabled")]
    public bool allowTextHeader = false;
    [Header("If Text Header is enabled, Set Text lifetime. Set to 0 for automate lifetime based on word count"), Range(0f, 100f)]
    public float textHeaderLifetime = 10f;

    //TODO: Add function name to tooltip.
    [Header("Automatically proceed to answers.") ,
     Tooltip("Set to true to automatically disable header and continue. Set to false to show the header until...."),]
    public bool automaticContinue = true;
    
    [Header("Set true if user should selects only one option")]
    public bool isSingleSelection = false;

    [Header("Set to true to set choice order")]
    public bool answersWithOrder = false;

    [Header("Set to true if the question UI is a dialogue")]
    public bool isDialogue = false;

    [Header("Assign dialogue box gameobject")]
    public GameObject dialogueBox;
    
    public bool RevealCorrectAnswers;

    private GameObject headerText;
    [HideInInspector]
    public List<GameObject> allButtons;
    [HideInInspector]
    public List<bool> userAnswers;

    [HideInInspector]
    public GameObject submitButton;
    
    private List<Image> allBtnImages;
    private List<Text> allBtnTexts;

    public UnityEvent correctAnswerTrigger;
    public UnityEvent wrongAnswerTrigger;

    [HideInInspector]
    public int answersCounter = 1;

    public bool result = false;

    [HideInInspector]
    public GameObject dialogueBoxPrefab;

    [HideInInspector]
    public GameObject contentBox;

    private int correctAnswersCounter = 0;
    private int wrongAnswersCounter = 0;
    private int correctSelectedAnswers = 0;
    private bool questionResult = false;

    public GameObject showHideHeader = null;
    private Image showHideHeaderIcon = null;

    private bool hideHeader = true;

    void Start()
    {
        Transform content = transform.Find("InterfaceContent");
        if (!content)
        {
            Debug.LogError("No child InterfaceContent' found in Decision Interface: " + gameObject.name);
            Destroy(this.gameObject);
            return;
        }

        headerText = content.Find("HeaderText").gameObject;

        if (allowTextHeader)
            headerText.gameObject.SetActive(true);
        else
            headerText.gameObject.SetActive(false);

        allBtnImages = new List<Image>();
        allBtnTexts = new List<Text>();

        allButtons = new List<GameObject>();
        for (int i = 0; i < content.childCount; ++i)
        {
            GameObject g = content.GetChild(i).gameObject;
            if (g.name != "HeaderText" && g.name != "SubmitButton" && g.name != "ShowHideHeader")
                allButtons.Add(g);

            if (g.name == "SubmitButton")
            {
                submitButton = g;
            }

            if (g.name == "ShowHideHeader")
            {
                showHideHeader = g;
            }
        }
        if (allowTextHeader)
        {
            if (textHeaderLifetime == 0)
            {
                textHeaderLifetime = GetComponent<HeaderAnimation>().textHeaderTime;
            }

            foreach (GameObject ui in allButtons)
            {
                allBtnImages.AddRange(ui.GetComponentsInChildren<Image>());
                allBtnTexts.AddRange(ui.GetComponentsInChildren<Text>());

                ui.GetComponent<ButtonBehavior>().ButtonInteractivity(false);
            }

            if (submitButton)
            {
                if (submitButton.name != "Background")
                {
                    allBtnImages.AddRange(submitButton.GetComponentsInChildren<Image>());
                    allBtnTexts.AddRange(submitButton.GetComponentsInChildren<Text>());

                    submitButton.GetComponent<ButtonBehavior>().ButtonInteractivity(false);
                }
            }

            if (showHideHeader)
            {
                showHideHeaderIcon = showHideHeader.GetComponent<Image>();
                showHideHeader.gameObject.SetActive(false);

            }


            foreach (Image i in allBtnImages)
                i.color = new Color(i.color.r, i.color.g, i.color.b, 0f);
            foreach (Text t in allBtnTexts)
                t.color = new Color(t.color.r, t.color.g, t.color.b, 0f);

            StartCoroutine(FadeTextToButtons());
        }
        else
            headerText.gameObject.SetActive(false);

        
        userAnswers = new List<bool>();
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator FadeTextToButtons()
    {
        // Decision Header Image + Animation last longer
        //GetComponent<InteractiveInterfaceSpawnAnimation>().observeHeaderImageDur = 1.8f;
       // float waitSpawnAnim = GetComponent<InteractiveInterfaceSpawnAnimation>().GetSpawnAnimationTime();

        // Wait at least one frame more till animation is complete and coroutine does not run anymore
        //yield return new WaitForSeconds(waitSpawnAnim);
        //yield return null; yield return null;

        // keep button interactivity false for text
        foreach (GameObject ui in allButtons)
            ui?.GetComponent<ButtonBehavior>().ButtonInteractivity(false);

        if (submitButton)
        {
            submitButton.GetComponent<ButtonBehavior>().ButtonInteractivity(false);
        }

        
        yield return new WaitForSeconds(textHeaderLifetime);
        
        while (!automaticContinue)
        {
            yield return new WaitForEndOfFrame();
        }
        
        headerText.gameObject.SetActive(false);


        float timer = 0f, duration = 0.6f;

        while (timer <= duration)
        {
            float newAlpha = Mathf.SmoothStep(1f, 0f, timer / duration);

            //if (headerText.color.a >= 0f)
               // headerText.color = new Color(headerText.color.r, headerText.color.g, headerText.color.b, newAlpha / 2f);

            foreach (Image i in allBtnImages)
                i.color = new Color(i.color.r, i.color.g, i.color.b, 1f - newAlpha);
            foreach (Text t in allBtnTexts)
                t.color = new Color(t.color.r, t.color.g, t.color.b, 1f - newAlpha);

            timer += Time.deltaTime;
            yield return null;
        }

        Image uiImageBorder = null;
        foreach (GameObject ui in allButtons)
        {
            if (ui.transform.Find("Background/Border"))
            {
                uiImageBorder = ui.transform.Find("Background/Border").GetComponent<Image>();
                uiImageBorder.color = new Color(uiImageBorder.color.r, uiImageBorder.color.g, uiImageBorder.color.b, 0f);
            }
        }

        Image uiSumbitBorder = null;
        if (submitButton)
        {
            if (submitButton.transform.Find("Background/Border"))
            {
                uiSumbitBorder = submitButton.transform.Find("Background/Border").GetComponent<Image>();
                uiSumbitBorder.color = new Color(uiSumbitBorder.color.r, uiSumbitBorder.color.g, uiSumbitBorder.color.b, 0f);
            }
        }


        foreach (GameObject ui in allButtons)
            ui.GetComponent<ButtonBehavior>().ButtonInteractivity(true);

        if (submitButton)
        {

            submitButton.GetComponent<ButtonBehavior>().ButtonInteractivity(true);
        }

        if (showHideHeader)
        {
            showHideHeader.gameObject.SetActive(true);
        }

        if (isDialogue)
        {
            if (dialogueBox != null)
            {
                //Instantiate dialogue box
                dialogueBoxPrefab = Instantiate(dialogueBox);

                contentBox = dialogueBoxPrefab.transform.Find("Viewport/Content").gameObject;
            }
        }


        headerText.gameObject.SetActive(false);
        allBtnImages.Clear();
        allBtnTexts.Clear();
    }

    private IEnumerator DecisionAnimationRestOfButtons()
    {
        float timer = 0f, duration = 0.6f;

        if (allBtnImages == null || allBtnTexts == null)
        {
            allBtnImages = new List<Image>();
            allBtnTexts = new List<Text>();
        }

        foreach (GameObject ui in allButtons)
        {
            allBtnImages.AddRange(ui.GetComponentsInChildren<Image>());
            allBtnTexts.AddRange(ui.GetComponentsInChildren<Text>());
        }

        while (timer <= duration)
        {
            float newAlpha = Mathf.SmoothStep(1f, 0f, timer / duration);

            foreach (Image i in allBtnImages)
                i.color = new Color(i.color.r, i.color.g, i.color.b, newAlpha);
            foreach (Text t in allBtnTexts)
                t.color = new Color(t.color.r, t.color.g, t.color.b, newAlpha);

            timer += Time.deltaTime;
            yield return null;
        }

        foreach (GameObject ui in allButtons)
            Destroy(ui);

        allButtons.Clear();
    }

    private IEnumerator DecisionAnimationSelectedButton(GameObject _ui, bool _correct)
    {
        if (_correct && correctAnswerTrigger != null)
        {
            userAnswers.Add(true);
        }
        if (!_correct && wrongAnswerTrigger != null)
        {
            userAnswers.Add(false);
        }
        // Give a head start to fade out anim of other buttons
        yield return new WaitForSeconds(0.2f);
        
        float timer = 0f, duration = 1.6f;

        Image uiImageBorder = null;
        Image uiImageForeGround = null;
        if (_ui.transform.Find("Background/Border"))
        {
            uiImageBorder = _ui.transform.Find("Background/Border").GetComponent<Image>();
            uiImageBorder.rectTransform.localScale = new Vector3(1f, 1f, 1f);

            uiImageForeGround = _ui.transform.Find("Background/Foreground").GetComponent<Image>();
        }
        Color answerColor;
        if (_correct)
            answerColor = new Color32(0, 255, 40, 255);
        else
            answerColor = Color.red;

        while (timer <= duration)
        {

            if (uiImageBorder&& RevealCorrectAnswers)
                uiImageBorder.color = Color.Lerp(uiImageBorder.color, answerColor, timer / duration);

            timer += Time.deltaTime;
            yield return null;
        }
        
        yield return new WaitForSeconds(2f);
        if (_correct && correctAnswerTrigger != null)
        {
            result = true;
            StoreUsersAnswers();
        }

        if (!_correct && wrongAnswerTrigger != null)
        {
            result = false;
            StoreUsersAnswers();
        }
        if (transform.root.GetComponent<InteractiveInterfaceSpawnAnimation>())
            transform.root.GetComponent<InteractiveInterfaceSpawnAnimation>().DestroyInterface();
        else
            Destroy(gameObject);
    }

    private IEnumerator CheckQuestionAnswers()
    {
        if (submitButton)
        {

            submitButton.GetComponent<ButtonBehavior>().ButtonActivation(false);
        }

        Destroy(dialogueBoxPrefab);

        /*Lock User's Answers*/
        foreach (GameObject ui in allButtons)
        {
            if (ui.GetComponent<MultipleToggleHandle>())
            {
                ui.GetComponent<MultipleToggleHandle>().AnswersBackgroundUpdateOnSubmit();
            }

            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.5f);


        wrongAnswersCounter = 0;
        foreach (GameObject ui in allButtons)
        {
        
            if (answersWithOrder)
            {
                if (!String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().orderOfAnswer))
                {
                    if (ui.GetComponent<MultipleToggleHandle>().answerOrder.text == ui.GetComponent<MultipleToggleHandle>().orderOfAnswer)
                    {
                        StartCoroutine(DecisionAnimationSelectedButton(ui, true));
                        ui.GetComponent<MultipleToggleHandle>().IsCorrectAnswerSelection();

                    }
                    else if (String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().answerOrder.text) && !String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().orderOfAnswer))
                    {

                        StartCoroutine(DecisionAnimationSelectedButton(ui, false));
                        ui.GetComponent<MultipleToggleHandle>().SetCorrectAnswerOrder();
                    }

                    else if (ui.GetComponent<MultipleToggleHandle>().answerOrder.text != ui.GetComponent<MultipleToggleHandle>().orderOfAnswer)
                    {
                        StartCoroutine(DecisionAnimationSelectedButton(ui, false));
                        ui.GetComponent<MultipleToggleHandle>().SetCorrectAnswerOrder();
                    }
                }
                else if (!String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().answerOrder.text) && String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().orderOfAnswer))
                {
                    StartCoroutine(DecisionAnimationSelectedButton(ui, false));
                    ui.GetComponent<MultipleToggleHandle>().InCorrectAnswer();
                }
                else if (String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().answerOrder.text) && String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().orderOfAnswer))
                {
                    ui.GetComponent<MultipleToggleHandle>().IgnoreAnswer();
                }
            }
            else
            {

                if (ui.GetComponent<MultipleToggleHandle>().isToggled && ui.GetComponent<MultipleToggleHandle>().isCorrect)
                {
                    StartCoroutine(DecisionAnimationSelectedButton(ui, true));
                }
                else if (!ui.GetComponent<MultipleToggleHandle>().isToggled && !ui.GetComponent<MultipleToggleHandle>().isCorrect)
                {
                    ui.GetComponent<MultipleToggleHandle>().IgnoreAnswer();
                }
                else if (ui.GetComponent<MultipleToggleHandle>().isToggled && !ui.GetComponent<MultipleToggleHandle>().isCorrect)
                {
                    StartCoroutine(DecisionAnimationSelectedButton(ui, false));
                    ui.GetComponent<MultipleToggleHandle>().InCorrectAnswer();

                }
                else if (!ui.GetComponent<MultipleToggleHandle>().isToggled && ui.GetComponent<MultipleToggleHandle>().isCorrect)
                {
                    StartCoroutine(DecisionAnimationSelectedButton(ui, false));
                    ui.GetComponent<MultipleToggleHandle>().IsCorrectAnswerSelection();

                }
            }

            ui.GetComponent<ButtonBehavior>().ButtonActivation(false);

            yield return new WaitForSeconds(0.5f);

        }

        StoreUsersAnswers();

        yield return null;
    }

    public void StoreUsersAnswers()
    {
        
        InterfaceManagement.Get.SetResultofDecision(userAnswers);
        correctAnswerTrigger.Invoke();
    }
    
    public void IsCorrectAnswer(GameObject _ui)
    {
        for (int i = allButtons.Count - 1; i >= 0; --i)
        {
            if (allButtons[i].GetComponent<ButtonBehavior>())
                allButtons[i].GetComponent<ButtonBehavior>().ButtonInteractivity(false);

            if (allButtons[i].name == _ui.name)
                allButtons.RemoveAt(i);
        }

        StartCoroutine(DecisionAnimationRestOfButtons());
        StartCoroutine(DecisionAnimationSelectedButton(_ui, true));
    }

    public void IsWrongAnswer(GameObject _ui)
    {
        for (int i = allButtons.Count - 1; i >= 0; --i)
        {
            if (allButtons[i].GetComponent<ButtonBehavior>())
                allButtons[i].GetComponent<ButtonBehavior>().ButtonInteractivity(false);

            if (allButtons[i].name == _ui.name)
                allButtons.RemoveAt(i);
        }

        StartCoroutine(DecisionAnimationRestOfButtons());
        StartCoroutine(DecisionAnimationSelectedButton(_ui, false));
    }

    public void MultipleAnswersCorrection()
    {

        StartCoroutine(CheckQuestionAnswers());

    }

    public void TwoAnswersCorrection()
    {
        foreach (GameObject ui in allButtons)
        {
            if (ui.GetComponent<MultipleToggleHandle>().isToggled && ui.GetComponent<MultipleToggleHandle>().isCorrect)
            {
                StartCoroutine(DecisionAnimationSelectedButton(ui, true));

            }
            else if (ui.GetComponent<MultipleToggleHandle>().isToggled && !ui.GetComponent<MultipleToggleHandle>().isCorrect)
            {
                StartCoroutine(DecisionAnimationSelectedButton(ui, false));
            }
        }

    }

    public int GetNumberOfCorrectAnswers()
    {
        return correctAnswersCounter;
    }

    public int GetNumberOfWrongAnswers()
    {
        return wrongAnswersCounter;
    }

    private void CorrectAnswersNoOrder()
    {
        foreach (GameObject ui in allButtons)
        {
            if (ui.GetComponent<MultipleToggleHandle>().isCorrect)
            {
                correctAnswersCounter++;
            }
        }
    }

    private void CorrectAnswersWithOrder()
    {
        foreach (GameObject ui in allButtons)
        {
            if (!String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().orderOfAnswer))
            {
                correctAnswersCounter++;
            }
        }
    }

    public int GetNumberOfCorrectSelectedAnswers() {

        int corrects = 0;

        foreach (GameObject ui in allButtons)
        {
            if (answersWithOrder)
            {
                if (!String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().orderOfAnswer))
                {
                    if (ui.GetComponent<MultipleToggleHandle>().answerOrder.text == ui.GetComponent<MultipleToggleHandle>().orderOfAnswer)
                    {
                        corrects++;

                    }
                }  
            }
            else
            {

                if (ui.GetComponent<MultipleToggleHandle>().isToggled && ui.GetComponent<MultipleToggleHandle>().isCorrect)
                {
                    corrects++;

                }
                
            }
        }

        return corrects;
    }
    
    public int GetNumberOfWrongSelectedAnswers()
    {
        int wrongs = 0;

        foreach (GameObject ui in allButtons)
        {
            if (answersWithOrder)
            {
                if (!String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().orderOfAnswer))
                {
                    if (String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().answerOrder.text) && !String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().orderOfAnswer))
                    {
                        wrongs++;

                    }

                    else if (ui.GetComponent<MultipleToggleHandle>().answerOrder.text != ui.GetComponent<MultipleToggleHandle>().orderOfAnswer)
                    {
                        wrongs++;

                    }
                }
                else if (!String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().answerOrder.text) && String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().orderOfAnswer))
                {
                    wrongs++;

                }
            }
            else
            {

                if (ui.GetComponent<MultipleToggleHandle>().isToggled && !ui.GetComponent<MultipleToggleHandle>().isCorrect)
                {
                    wrongs++;

                }
                
            }
        }

        return wrongs;
    }
    
    public bool GetQuestionResult()
    {

        CorrectAnswersNoOrder();

        foreach (GameObject ui in allButtons)
        {
            if (answersWithOrder)
            {
                if (!String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().orderOfAnswer))
                {
                    if (ui.GetComponent<MultipleToggleHandle>().answerOrder.text == ui.GetComponent<MultipleToggleHandle>().orderOfAnswer)
                    {
                        correctSelectedAnswers++;


                    }
                    else if (String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().answerOrder.text) && !String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().orderOfAnswer))
                    {
                        wrongAnswersCounter++;

                    }

                    else if (ui.GetComponent<MultipleToggleHandle>().answerOrder.text != ui.GetComponent<MultipleToggleHandle>().orderOfAnswer)
                    {
                        wrongAnswersCounter++;

                    }
                }
                else if (!String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().answerOrder.text) && String.IsNullOrEmpty(ui.GetComponent<MultipleToggleHandle>().orderOfAnswer))
                {
                    wrongAnswersCounter++;

                }
            }
            else
            {

                if (ui.GetComponent<MultipleToggleHandle>().isToggled && ui.GetComponent<MultipleToggleHandle>().isCorrect)
                {
                    correctSelectedAnswers++;

                }
                else if (ui.GetComponent<MultipleToggleHandle>().isToggled && !ui.GetComponent<MultipleToggleHandle>().isCorrect)
                {
                    wrongAnswersCounter++;


                }
                else if (!ui.GetComponent<MultipleToggleHandle>().isToggled && ui.GetComponent<MultipleToggleHandle>().isCorrect)
                {
                    wrongAnswersCounter++;


                }
            }
        }

        if (correctSelectedAnswers == correctAnswersCounter && wrongAnswersCounter == 0)
        {
            questionResult = true;
        }
        else
        {
            questionResult = false;
        }

        return questionResult;
    }

    public void ShowHideHeader()
    {

        Sprite toggleSprite;

        if (hideHeader)
        {
            toggleSprite = Resources.Load("MAGESres/UI/InterfaceMaterial/Images/questionOptions", typeof(Sprite)) as Sprite;

            if (toggleSprite)
            {
                showHideHeaderIcon.sprite = toggleSprite;
                showHideHeaderIcon.color = new Color(showHideHeaderIcon.color.r, showHideHeaderIcon.color.g, showHideHeaderIcon.color.b, 1f);
            }

            foreach (GameObject ui in allButtons)
                ui?.SetActive(false);

            if (submitButton)
            {
                submitButton.SetActive(false);
            }
            headerText.gameObject.SetActive(true);

            hideHeader = false;
        }
        else
        {
            toggleSprite = Resources.Load("MAGESres/UI/InterfaceMaterial/Images/questionMark", typeof(Sprite)) as Sprite;

            if (toggleSprite)
            {
                showHideHeaderIcon.sprite = toggleSprite;
                showHideHeaderIcon.color = new Color(showHideHeaderIcon.color.r, showHideHeaderIcon.color.g, showHideHeaderIcon.color.b, 1f);
            }

            headerText.gameObject.SetActive(false);

            foreach (GameObject ui in allButtons)
                ui?.SetActive(true);

            if (submitButton)
            {
                submitButton.SetActive(true);
            }

            hideHeader = true;
        }

        if (showHideHeaderIcon)
        {


            showHideHeaderIcon.sprite = toggleSprite;
            showHideHeaderIcon.color = new Color(showHideHeaderIcon.color.r, showHideHeaderIcon.color.g, showHideHeaderIcon.color.b, 1f);

        }


    }
}