using ovidVR.Utilities.VoiceActor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MultipleToggleHandle : MonoBehaviour
{

    [System.Serializable]
    public class Message
    {
        public string spokenText;
        public string userName;
        public float messageLifeTime;
        public string voiceActorClip;
        public bool lipsync = true;
        public UnityEvent OnStartAnswering;

        [HideInInspector]
        public GameObject messageObject;

    }

    public bool isCorrect = false;
    [HideInInspector]
    public bool isToggled = false;

    [Header("Set the order of the answer with a number e.x (1). Leave it empty if the answer is wrong")]
    public string orderOfAnswer = null;


    [Tooltip("Message object prefab")]
    public GameObject messageObject;

    public List<Transform> dialoguesTransform = new List<Transform>();

    [Header("")]
    public float displayMessagesTimer = 2f;
    public float typingMessageTimer = 0.02f;

    [SerializeField, Header("Set question message")]
    public Text questionMessage;
    public string questionMessageUser;
    public float questionMessageLifetime;
    public string questionvoiceActorClip;
    public bool questionlipsync = true;
    public UnityEvent questionOnStartAnswering;

    [SerializeField, Header("Set option's answers")]
    public List<Message> answersList = new List<Message>();

    private GameObject contentBox;
    private string currentText = "";

    private List<Message> messageList = new List<Message>();
    private List<Message> dialogueList = new List<Message>();


    [HideInInspector]
    public Text answerOrder;
    [HideInInspector]
    public Image answerBackground;
    [HideInInspector]
    public Image correctionImage;
    [HideInInspector]
    public Text correctionText;
    [HideInInspector]
    public Image checkBoxImage;
    [HideInInspector]
    public Image foregroundImage;


    // Checks if root parent object, with DecisionInterface script, reveals correct answers.
    private bool RootRevealAnswers;


    // Use this for initialization
    void Start()
    {

        Transform buttonTextChildren = transform.Find("ButtonAnimatedImage/OrderText");

        Transform buttonImageChildren = transform.Find("ButtonAnimatedImage/ChoiceBackground");

        Transform buttonCorrectionChildren = transform.Find("AnswerCorrection");

        Transform buttonCheckBoxChildren = transform.Find("ButtonAnimatedImage/Animator");

        Transform buttonChildren = transform.Find("Background/Foreground");

        if (buttonChildren)
        {
            foregroundImage = buttonChildren.GetComponent<Image>();
        }

        if (buttonCheckBoxChildren)
        {
            checkBoxImage = buttonCheckBoxChildren.GetComponentInChildren<Image>();
        }

        if (buttonTextChildren)
        {
            answerOrder = buttonTextChildren.GetComponentInChildren<Text>();
        }

        if (buttonImageChildren)
        {
            answerBackground = buttonImageChildren.GetComponentInChildren<Image>();

            if (answerBackground)
            {
                answerBackground.enabled = false;
            }
        }

        if (buttonCorrectionChildren)
        {
            correctionImage = buttonCorrectionChildren.GetComponentInChildren<Image>();
            correctionText = buttonCorrectionChildren.GetComponentInChildren<Text>();

            if (correctionImage)
            {
                correctionImage.enabled = false;
            }

        }


        Message questionMsg = new Message();

        if (questionMessage != null)
        {
            if (questionMessageUser != "Care Coordinator")
            {
                questionMsg.spokenText = questionMessage.text;


                questionMsg.userName = questionMessageUser;
                questionMsg.messageLifeTime = questionMessageLifetime;
                questionMsg.voiceActorClip = questionvoiceActorClip;
                questionMsg.lipsync = questionlipsync;
                questionMsg.OnStartAnswering = questionOnStartAnswering;


                messageList.Add(questionMsg);
            }
        }
        if (answersList.Count != 0)
        {
            foreach (Message msg in answersList)
            {
                messageList.Add(msg);
            }
        }


        RootRevealAnswers = transform.root.gameObject.GetComponent<DecisionInterface>().RevealCorrectAnswers;

    }

    public void AnswersBackgroundUpdateOnSubmit()
    {
        if (answerBackground)
        {
            answerBackground.color = new Color32(0, 0, 0, 0);
        }

        if (checkBoxImage)
        {
            checkBoxImage.color = new Color32(149, 149, 149, 255);
        }

        if (answerOrder)
        {
            answerOrder.color = new Color32(149, 149, 149, 255);
        }

    }

    private void NotToggledButtons(GameObject toggled_button)
    {
        List<GameObject> Buttons = new List<GameObject>();
        //Debug.Log("Buttons");
        for (int i = 0; i < GetComponentInParent<DecisionInterface>().allButtons.Count; i++)
        {
            //Debug.Log(GetComponentInParent<DecisionInterface>().allButtons[i].name);
            Buttons.Add(GetComponentInParent<DecisionInterface>().allButtons[i]);
        }

        //Debug.Log("Remove "+ toggled_button.name);
        Buttons.Remove(toggled_button);

        foreach (GameObject button in Buttons)
        {
            button.GetComponent<MultipleToggleHandle>().isToggled = false;
        }

    }

    public void ImageToggleUpdate()
    {

        if (GetComponent<ButtonBehavior>())
        {
            if (GetComponentInParent<DecisionInterface>())
            {
                if (GetComponentInParent<DecisionInterface>().answersWithOrder)
                {
                    if (GetComponent<ButtonBehavior>().GetIsButtonToggled())
                    {
                        isToggled = true;

                        answerOrder.text = GetComponentInParent<DecisionInterface>().answersCounter.ToString();
                        GetComponentInParent<DecisionInterface>().answersCounter++;

                        if (GetComponentInParent<DecisionInterface>().isDialogue)
                        {
                            if (messageList != null)
                            {
                                StartCoroutine(sendMessageToDialogueBox(messageList));
                            }

                        }

                    }
                    else
                    {
                        isToggled = false;
                        answerOrder.text = "";

                        GetComponentInParent<DecisionInterface>().answersCounter--;
                    }

                }
                else
                {

                    Sprite toggleSprite;



                    if (GetComponent<ButtonBehavior>().GetIsButtonToggled())
                    {

                        isToggled = true;

                        if (gameObject.transform.parent.transform.parent.GetComponent<DecisionInterface>().isSingleSelection)
                            NotToggledButtons(this.gameObject);

                        toggleSprite = Resources.Load("MAGESres/UI/InterfaceMaterial/Images/CheckBoxON", typeof(Sprite)) as Sprite;

                        if (GetComponentInParent<DecisionInterface>().isDialogue)
                        {
                            if (messageList != null)
                            {

                                StartCoroutine(sendMessageToDialogueBox(messageList));

                            }

                        }



                    }
                    else
                    {
                        isToggled = false;
                        toggleSprite = Resources.Load("MAGESres/UI/InterfaceMaterial/Images/CheckBoxOFF", typeof(Sprite)) as Sprite;
                    }

                    if (toggleSprite)
                        GetComponent<ButtonBehavior>().UpdateButtonSprite(toggleSprite);


                    if (GetComponentInParent<DecisionInterface>().isSingleSelection)
                    {
                        foreach (GameObject _ui in GetComponentInParent<DecisionInterface>().allButtons)
                        {
                            if (_ui.GetComponent<ButtonBehavior>().GetIsButtonToggled())
                            {
                                if (gameObject.name != _ui.name)
                                {
                                    toggleSprite = Resources.Load("MAGESres/UI/InterfaceMaterial/Images/CheckBoxOFF", typeof(Sprite)) as Sprite;

                                    _ui.GetComponent<ButtonBehavior>().SetIsButtonToggled(false);

                                    if (toggleSprite)
                                        _ui.GetComponent<ButtonBehavior>().UpdateButtonSprite(toggleSprite);
                                }

                            }
                        }
                    }


                }


            }

        }
    }

    public void IsCorrectAnswerSelection()
    {
        if (RootRevealAnswers)
        {
            Sprite correctionSprite;

            correctionSprite = Resources.Load("MAGESres/UI/ApplicationGeneric/Sprites/AcceptWhite", typeof(Sprite)) as Sprite;

            if (correctionImage)
            {

                if (correctionSprite)
                {
                    correctionImage.sprite = correctionSprite;
                }

                correctionImage.color = new Color32(0, 255, 40, 255);
            }

            correctionImage.enabled = true;
        }

    }



    public void SetCorrectAnswerOrder()
    {

        if (correctionText)
        {
            correctionText.text = orderOfAnswer;

        }
    }


    public void InCorrectAnswer()
    {
        if (RootRevealAnswers)
        {
            Sprite errorSprite;

            errorSprite = Resources.Load("MAGESres/UI/ApplicationGeneric/Sprites/Error", typeof(Sprite)) as Sprite;

            if (correctionImage)
            {
                if (errorSprite)
                {
                    correctionImage.sprite = errorSprite;
                }

                correctionImage.color = Color.red;
            }

            correctionImage.enabled = true;
        }

    }


    public void IgnoreAnswer()
    {
        if (GetComponent<ButtonBehavior>())
        {
            foregroundImage.color = new Color32(121, 121, 121, 255);

        }
    }


    public void CorrectAnswerNoOrder()
    {
        Sprite toggleSprite;


        answerOrder.enabled = false;

        toggleSprite = Resources.Load("MAGESres/UI/InterfaceMaterial/Images/CheckBoxON", typeof(Sprite)) as Sprite;

        if (toggleSprite)
            GetComponent<ButtonBehavior>().UpdateButtonSprite(toggleSprite);
    }


    private IEnumerator sendMessageToDialogueBox(List<Message> messagesToDisplay)
    {
        //yield return new WaitForSeconds(1f);

        contentBox = GetComponentInParent<DecisionInterface>().contentBox;

        //Disable button interactivity
        foreach (GameObject ui in GetComponentInParent<DecisionInterface>().allButtons)
        {
            ui.GetComponent<ButtonBehavior>().ButtonInteractivity(false);
        }

        GameObject submitButton = GetComponentInParent<DecisionInterface>().submitButton;
        submitButton.GetComponent<ButtonBehavior>().ButtonInteractivity(false);

        GameObject showHideHeader = GetComponentInParent<DecisionInterface>().showHideHeader;
        showHideHeader.GetComponent<ButtonBehavior>().ButtonInteractivity(false);


        for (int i = 0; i < messagesToDisplay.Count; i++)
        {

            Message newMessage = new Message();


            newMessage.spokenText = messagesToDisplay[i].spokenText;
            newMessage.userName = messagesToDisplay[i].userName;
            newMessage.messageLifeTime = messagesToDisplay[i].messageLifeTime;
            newMessage.voiceActorClip = messagesToDisplay[i].voiceActorClip;
            newMessage.OnStartAnswering = messagesToDisplay[i].OnStartAnswering;
            newMessage.lipsync = messagesToDisplay[i].lipsync;

            Transform dialogueTransform = null;

            if (newMessage.userName == "Emil")
            {

                dialogueTransform = dialoguesTransform[0];
            }
            else if (newMessage.userName == "Patricia")
            {
                dialogueTransform = dialoguesTransform[1];
            }
            else if (newMessage.userName == "Nurse" || newMessage.userName == "Registered Nurse")
            {
                dialogueTransform = dialoguesTransform[2];
            }

            GameObject newText = Instantiate(messageObject, dialogueTransform.position, dialogueTransform.rotation);
            Text[] textComponents = newText.GetComponentsInChildren<Text>();

            textComponents[1].text = newMessage.userName;

            if (newMessage.OnStartAnswering != null)
            {
                newMessage.OnStartAnswering.Invoke();
            }


            //Play Audio
            if (!string.IsNullOrEmpty(newMessage.voiceActorClip))
            {
                AudioClip playedAudioClip = null;

                if (!string.IsNullOrEmpty(newMessage.userName))
                    playedAudioClip = VoiceActor.PlayVoiceActor(newMessage.voiceActorClip, newMessage.userName);
                else
                    playedAudioClip = VoiceActor.PlayVoiceActor(newMessage.voiceActorClip);

                //Play lipsync
                if (newMessage.lipsync && playedAudioClip)
                {
                    GameObject character = GameObject.Find(newMessage.userName);
                    character.transform.GetChild(0).gameObject.SetActive(true);
                    AudioSource audio = character.transform.Find("LipSyncTargets/InputType").GetComponent<AudioSource>();
                    audio.loop = false;
                    AudioClip clip = Resources.Load("Dialogues/" + newMessage.userName + "/" + newMessage.voiceActorClip) as AudioClip;
                    audio.clip = clip;
                    audio.Play();
                    StartCoroutine(CloseLipSync(character.transform.Find("LipSyncTargets").gameObject));
                }
            }


            //Typing Effect       
            for (int j = 0; j <= newMessage.spokenText.Length; j++)
            {
                currentText = newMessage.spokenText.Substring(0, j);
                textComponents[0].text = currentText;
                newMessage.messageObject = newText;
                yield return new WaitForSeconds(typingMessageTimer);
            }


            dialogueList.Add(newMessage);

            Destroy(dialogueList[i].messageObject, dialogueList[i].messageLifeTime);

            if (dialogueList[i].messageObject == null)
            {
                dialogueList.Remove(dialogueList[i]);
            }

            yield return new WaitForSeconds(displayMessagesTimer);

            Destroy(newText);
        }


        //Enable button interactivity
        foreach (GameObject ui in GetComponentInParent<DecisionInterface>().allButtons)
        {
            ui.GetComponent<ButtonBehavior>().ButtonInteractivity(true);
        }

        submitButton.GetComponent<ButtonBehavior>().ButtonInteractivity(true);

        showHideHeader.GetComponent<ButtonBehavior>().ButtonInteractivity(true);

    }


    IEnumerator CloseLipSync(GameObject LipSyncTargets)
    {
        AudioSource audio = LipSyncTargets.transform.GetChild(0).GetComponent<AudioSource>();
        yield return new WaitWhile(() => audio.isPlaying);
        LipSyncTargets.SetActive(false);

    }
}
