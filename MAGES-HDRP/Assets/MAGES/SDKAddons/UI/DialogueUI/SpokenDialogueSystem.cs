using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ovidVR.Utilities.VoiceActor;
using UnityEngine.Events;

public class SpokenDialogueSystem : MonoBehaviour {

    [System.Serializable]
    public class Message
    {
        public string spokenText;
        public string userName;
        public float messageLifeTime;

        public string voiceActorClip;
        public bool lipsync = true;
        public UnityEvent OnStartAnswering;

        public GameObject messageObject;

    }

    public int maxMessages = 4;
    public GameObject contentBox, messageObject;
    public float displayMessageTimer = 5f;
    public float typingMessageTimer = 0.05f;
    private string currentText = "";
    

    [SerializeField]
    public List<Message> messageList = new List<Message>();

    private List<Message> dialogueList = new List<Message>();

    public List<Transform> dialoguesTransform = new List<Transform>();

    
	// Use this for initialization
	void Start () {

       
        StartCoroutine(sendMessageToDialogueBox());
    }
	
    void OnDisable()
    {
        StopAllCoroutines();
    }

   
    private IEnumerator sendMessageToDialogueBox()
    {

        while (dialogueList.Count < maxMessages)
        {

            for (int i = 0; i < messageList.Count; i++)
            {

                Message newMessage = new Message();

                newMessage.spokenText = messageList[i].spokenText;
                newMessage.userName = messageList[i].userName;
                newMessage.messageLifeTime = messageList[i].messageLifeTime;
                newMessage.voiceActorClip = messageList[i].voiceActorClip;
                newMessage.OnStartAnswering = messageList[i].OnStartAnswering;
                //newMessage.lipsync = messageList[i].lipsync;

                Transform dialogueTransform = null;

                if (newMessage.userName != "Care Coordinator")
                {
                    if (newMessage.userName == "Emil")
                    {
                        dialogueTransform = dialoguesTransform[0];
                    }
                    else if (newMessage.userName == "Patricia")
                    {
                        dialogueTransform = dialoguesTransform[1];
                    }
                    else if (newMessage.userName == "Nurse")
                    {
                        dialogueTransform = dialoguesTransform[2];
                    }


                    GameObject newText = Instantiate(messageObject, dialogueTransform.position, dialogueTransform.rotation);
                    Text[] textComponents = newText.GetComponentsInChildren<Text>();

                    textComponents[1].text = newMessage.userName;

                    //Play Audio
                    if (!string.IsNullOrEmpty(newMessage.voiceActorClip))
                    {
                        AudioClip playedAudioClip = null;

                        if (!string.IsNullOrEmpty(newMessage.userName))
                        {
                            playedAudioClip = VoiceActor.PlayVoiceActor(newMessage.voiceActorClip, newMessage.userName);
                        }
                        else
                        {
                            playedAudioClip = VoiceActor.PlayVoiceActor(newMessage.voiceActorClip);
                        }

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

                    yield return new WaitForSeconds(displayMessageTimer);
                }
            }
        }
        yield return null;
    }

    IEnumerator CloseLipSync(GameObject LipSyncTargets)
    {
        AudioSource audio = LipSyncTargets.transform.GetChild(0).GetComponent<AudioSource>();
        yield return new WaitWhile(() => audio.isPlaying);
        LipSyncTargets.SetActive(false);

    }
}
