using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using ovidVR.sceneGraphSpace;
using UnityEngine.UI;
using System.Linq;
using ovidVR.UIManagement.LanguageText;
using ovidVR.GameController;
using ovidVR.Networking;
using ovidVR.Utilities;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using ovidVR.Utilities.prefabSpawnManager.prefabSpawnConstructor;

namespace ovidVR.UIManagement
{
    public class InterfaceManagement : MonoBehaviour
    {
        public ApplicationLanguages applicationLanguage;

        [SerializeField, Header("Set Interface Sounds")]
        internal AudioClip buttonHoverSound, buttonPressSound, spawnUISound, destroyUISound, spawnDecisionUISound;

        private Transform leftHand;
        private Transform rightHand;

        private AudioSource audioSpeech;

        private Transform interactiveUI, notificationUI, actionProgressUI;

        private string uiPath;

        private Queue<GameObject> notificationUISpawn;

        private GameObject raycastRight, raycastLeft;
        private Camera vrCam;
        private int vrCamCullingMask;
        private GameObject fadeCameraCanvas;

        private bool allowUserSpawnedUIs;

        // Keep count of how many interactive Interfaces are currently on screen
        // For raycast activation / deactivation purposes
        private int currentlySpawnedInteractiveInterfaces;
                
        private static List<bool> resultOfDecision;

        private static InterfaceManagement uiManager;
        public static InterfaceManagement Get
        {
            get
            {
                if (!uiManager)
                {
                    uiManager = FindObjectOfType(typeof(InterfaceManagement)) as InterfaceManagement;

                    if (!uiManager) { Debug.LogError("Error, No InteractiveManagement was found"); }
                }

                return uiManager;
            }
        }

        private void Awake()
        {
            leftHand = OvidVRControllerClass.Get.leftHand.transform;
            rightHand = OvidVRControllerClass.Get.rightHand.transform;
            resultOfDecision = new List<bool>();
            
            if (GetComponent<AudioSource>())
                audioSpeech = GetComponent<AudioSource>();
            else
                Debug.LogError("Error no AudioSource Child found");

            interactiveUI = transform.Find("Interactive");
            notificationUI = transform.Find("Notification");
            actionProgressUI = transform.Find("ActionProgress");

            uiPath = "MAGESres/UI/";

            notificationUISpawn = new Queue<GameObject>();

            InterfaceManagementMediator.SpawnProgressUI = SpawnProgressUI;
            InterfaceManagementMediator.DestroyNotification = DestroyNotification;
            InterfaceManagementMediator.FacingUserInterface = FacingUserInterface;
            InterfaceManagementMediator.GetUIMessage = GetUIMessage;
            InterfaceManagementMediator.FollowHandInterface = FollowHandInterface;
            InterfaceManagementMediator.GetActiveNotification = GetActiveNotification;
            InterfaceManagementMediator.GetAllNotifications = GetAllNotifications;
            InterfaceManagementMediator.InterfaceRaycastActivation = InterfaceRaycastActivation;
            InterfaceManagementMediator.ResetInterfaceManagement = ResetInterfaceManagement;
            InterfaceManagementMediator.SpawnDynamicNotificationUI = SpawnDynamicNotificationUI;
            InterfaceManagementMediator.SpawnUI = SpawnUI;
            DecisionInterfaceMediator.SetDecisionCorrectAction = SetDecisionCorrectAction;
            DecisionInterfaceMediator.SetDecisionWrongAction = SetDecisionWrongAction;

            DecisionInterfaceMediator.GetResult = GetResultofDecision;
            
            // V2.0 START ------------------------------

            if (!buttonHoverSound || !buttonPressSound || !spawnUISound || !destroyUISound || !spawnDecisionUISound)
                Debug.LogError("Error in Interface Management, at least one of AudioClips is empty");

            raycastRight = rightHand.transform.Find("RaycastRightHand").gameObject;
            raycastLeft = leftHand.transform.Find("RaycastLeftHand").gameObject;
            raycastRight.SetActive(false);
            raycastLeft.SetActive(false);

            allowUserSpawnedUIs = false;
            currentlySpawnedInteractiveInterfaces = 0;

            vrCam = Camera.main;
            vrCamCullingMask = vrCam.cullingMask;                       
        }
       
        // Public Useful Methods --------------------------------

        /// <summary>
        /// Allow for user to spawn or not UIs.
        /// For example options cannot open when on OperationStart or End
        /// </summary>
        /// <param name="_allowUSerSapwnedUI"></param>
        public void SetUserSpawnedUIAllowance(bool _allowUSerSapwnedUI)
        {
            allowUserSpawnedUIs = _allowUSerSapwnedUI;
        }
 
        /// <summary>
        /// Get if user spawned UIs are currently allowed
        /// </summary>
        /// <returns></returns>
        public bool GetUserSpawnedUIAllowance()
        {
            return allowUserSpawnedUIs;
        }

        public void InterfaceRaycastActivation(bool _activate)
        {
            //If the user is spectator do not let him interact with anything.
            if (OvidVRControllerClass.Get.IsInNetwork)
            {
                if (OvidVRControllerClass.Get.GetIsSpectator())
                {
                    raycastRight.SetActive(false);
                    raycastLeft.SetActive(false);
                    return;
                }
            }
            
            if (raycastLeft && raycastRight)
            {
                raycastRight.SetActive(_activate);
                raycastLeft.SetActive(_activate);
            }
        }

        public void FacingUserInterface(GameObject _ui, bool _makeCameraParent, float _distance = 1f)
        {
            Transform camTr = OvidVRControllerClass.Get.GetCameraHead().transform;
            _ui.transform.position = camTr.position;
            _ui.transform.rotation = camTr.rotation;


            _ui.transform.Translate(Vector3.forward * Mathf.Clamp(_distance, -6f, 6f), Space.Self);

            if (_makeCameraParent)
                _ui.transform.parent = camTr;
        }

        /// <summary>
        /// Adds FollowTransformHand script if ui parent does not contain it
        /// Follows the transform only of user hand with user defined offset on Y axis
        /// </summary>
        /// <param name="_ui">reference to gameObject itself</param>
        /// <param name="_followLeftHand"> true to appear on left hand, false to appear on right</param>
        /// <param name="offsetY"> how high up from hand it should appear</param>
        public void FollowHandInterface(GameObject _ui, bool _followLeftHand, float offsetY = 0.12f)
        {
            if (!_ui)
                return;

            FollowTransformHand uiScript;
            if (_ui.GetComponent<FollowTransformHand>())
                uiScript = _ui.GetComponent<FollowTransformHand>();
            else
                uiScript = _ui.AddComponent<FollowTransformHand>();

            if(_followLeftHand)
                uiScript.SetHandTransform(leftHand, offsetY);
            else
                uiScript.SetHandTransform(rightHand, offsetY);
        }

        /// <summary>
        /// Has no Access from Utilities Mediator
        /// </summary>
        /// <param name="_speech"></param>
        public void PlaySpeech(LanguageSpeech _speech)
        {
            string fullFilePath = "Sounds/" + gameObject.scene.name +  "/Speech/" + _speech.ToString() + "/" + applicationLanguage.ToString() + "/";

            Object[] clip = Resources.LoadAll(fullFilePath, typeof(AudioClip));

            if (clip == null || clip.Length == 0)
            {
                Debug.LogError("No Speech sound file found in: " + fullFilePath);
                return;
            }

            // CHECK ALL UIs for Audio
            List<AudioSource> allChildUIAudios = new List<AudioSource>();
            allChildUIAudios.AddRange(GetComponentsInChildren<AudioSource>());
            foreach(AudioSource ad in allChildUIAudios)
            {
                if (ad.isPlaying)
                    ad.volume = 0.2f;
            }

            audioSpeech.clip = (AudioClip)clip[0];
            audioSpeech.Play();
        }

        // Spawn Notification ------------------------------------

        /// <summary>
        /// Has no Access from Utilities Mediator
        /// For Internal use from NotificationUIs only
        /// </summary>
        internal void SpawnNextNotificationUIFromQueue()
        {
            if (notificationUISpawn.Count == 0)
                return;

            GameObject ui = notificationUISpawn.Dequeue();
            if (!ui)
                return;

            if (ui && ui.GetComponent<NotificationUI>())
            {
                if (audioSpeech.isPlaying)
                    ui.GetComponent<NotificationUI>().StartNotification(0.2f);
                else
                    ui.GetComponent<NotificationUI>().StartNotification();
            }               
            else
                Debug.LogError("No NotificationUI script or UI as gameobject found");
        }

        /// <summary>
        /// Has no Access from Utilities Mediator
        /// For Internal use from Spawned Interactive UIs with Spawn Animator only
        /// </summary>
        internal void UpdateSpanwedInterfaceCount(bool _spawnedNew = true)
        {
            if (_spawnedNew)
            {
                if (currentlySpawnedInteractiveInterfaces == 0)
                    InterfaceRaycastActivation(true);

                ++currentlySpawnedInteractiveInterfaces;
            }
            else
            {
                --currentlySpawnedInteractiveInterfaces;
                if (currentlySpawnedInteractiveInterfaces < 0)
                {
                    Debug.LogError("Negative value for currentlySpawnedInteractiveInterfaces. Something went wrong");
                    currentlySpawnedInteractiveInterfaces = 0;
                }
                else if (currentlySpawnedInteractiveInterfaces == 0)
                    InterfaceRaycastActivation(false);
            }
        }

        public GameObject SpawnDynamicNotificationUI(NotificationUITypes _type, LanguageTranslator _displayMessageKey, float _lifeTime, string _uniqueID = "")
        {
            string message = GetUIMessage(_displayMessageKey);

            return SpawnNotification(null, _type, message, _lifeTime, _uniqueID);
        }

        public GameObject SpawnDynamicNotificationUI(NotificationUITypes _type, string _displayMessage, float _lifeTime, string _uniqueID = "")
        {
            return SpawnNotification(null, _type, _displayMessage, _lifeTime, _uniqueID);
        }

        /// <summary>
        /// Only for special cases from developers. By default it should not be used
        /// Has no Access from Utilities Mediator
        /// </summary>
        /// <param name="_spawnPoint"></param>
        /// <param name="_type"></param>
        /// <param name="_displayMessageKey"></param>
        /// <param name="_lifeTime"></param>
        /// <param name="_uniqueID"></param>
        /// <returns></returns>
        public GameObject SpawnStaticNotificationUI(GameObject _spawnPoint, NotificationUITypes _type, LanguageTranslator _displayMessageKey, float _lifeTime, string _uniqueID = "")
        {
            string message = GetUIMessage(_displayMessageKey);

            return SpawnNotification(_spawnPoint, _type, message, _lifeTime, _uniqueID);
        }

        public GameObject SpawnStaticNotificationUI(GameObject _spawnPoint, NotificationUITypes _type, string _displayMessage, float _lifeTime, string _uniqueID = "")
        {
            return SpawnNotification(_spawnPoint, _type, _displayMessage, _lifeTime, _uniqueID);
        }

        /// <summary>
        /// Caution: Still Under Development & Testing. Spawns without a parent
        /// Different Type of Notification. It is spawned no at Notifications. Versitile - use might vary
        /// </summary>
        /// <param name="_displayMessage"> message displayed</param>
        /// <param name="_lineEndTransfromPos"> transform of the object the end sphere will be</param>
        /// <param name="_followConstantly">if ui follows constantly posiiton of transform provided</param>
        /// <param name="_scaleMul">from x0 up to x6 scale of notification ui</param>
        /// <returns></returns>
        public GameObject SpawnExtraExplanationNotification(string _displayMessage, Transform _lineEndTransfromPos, bool _followConstantly, float _scaleMul = 1f)
        {
            if (string.IsNullOrEmpty(_displayMessage))
                return null;

            GameObject ui = (GameObject)Resources.Load(uiPath + "Notifications/ExtraExplanationNotification") as GameObject;

            if (ui == null)
            {
                Debug.LogError("ERROR: Notification found: ExtraExplanationNotification");
                return null;
            }

            ui = Instantiate(ui);
            ui.name = "ExtraExplanationNotification";

            ui.GetComponent<ExtraExpNotificationUI>().SetUpExtraExplanationNotification(_displayMessage, _lineEndTransfromPos, _followConstantly, _scaleMul);

            return ui;
        }

        public GameObject SpawnExtraExplanationNotification(LanguageTranslator _displayMessageKey, Transform _lineEndTransfromPos, bool _followConstantly, float _scaleMul = 1f)
        {
            string message = GetUIMessage(_displayMessageKey);

            GameObject ui = (GameObject)Resources.Load(uiPath + "Notifications/ExtraExplanationNotification") as GameObject;

            if (ui == null)
            {
                Debug.LogError("ERROR: Notification found: ExtraExplanationNotification");
                return null;
            }

            ui = Instantiate(ui);
            ui.name = "ExtraExplanationNotification";

            ui.GetComponent<ExtraExpNotificationUI>().SetUpExtraExplanationNotification(message, _lineEndTransfromPos, _followConstantly, _scaleMul);

            return ui;
        }

        /// <summary>
        /// Spawn Custom Extra explanation GameObject
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="_lineEndTransfromPos"></param>
        /// <param name="_followConstantly"></param>
        /// <param name="_scaleMul"></param>
        /// <returns></returns>
        public GameObject SpawnCustomExtraExplanationNotification(string _path, Transform _lineEndTransfromPos, bool _followConstantly, float _scaleMul = 1f)
        {
            GameObject ui = (GameObject)Resources.Load(_path) as GameObject;

            if (ui == null)
            {
                Debug.LogError("ERROR: Notification found: ExtraExplanationNotification");
                return null;
            }

            ui = PrefabImporter.SpawnGenericPrefab(_path);
            ui.name = "ExtraExplanationNotification";

            ui.GetComponent<ExtraExpNotificationUI>().SetUpExtraExplanationNotification("", _lineEndTransfromPos, _followConstantly, _scaleMul);

            return ui;
        }

        // Internal Notification Function ------------------------

        private GameObject SpawnNotification(GameObject _spawnPoint, NotificationUITypes _type, string _displayMessage, float _lifeTime, string _uniqueID = "")
        {
            if (string.IsNullOrEmpty(_displayMessage))
                return null;

            GameObject ui = (GameObject)Resources.Load(uiPath + "Notifications/" + _type.ToString()) as GameObject;

            if (ui == null)
            {
                Debug.LogError("ERROR: Notification UI type NOT found: " + _type.ToString());
                return null;
            }

            ui = Instantiate(ui, notificationUI);
            ui.name = _type.ToString();    // Remove (Clone)

            NotificationUI uiScript = ui.GetComponent<NotificationUI>();
            if (uiScript != null)
            {
                uiScript.SetUniqueID(_uniqueID);
                uiScript.SetMessage(_displayMessage);
                uiScript.SetLifetime(_lifeTime);
            }
            else
            {
                Debug.LogError("Destroyed Notification UI due to lacking the script: NotificationUI");
                DestroyImmediate(ui);
                return null;
            }

            // If no other Notification is spawned, spawn immediately
            if (notificationUI.childCount == 1 && notificationUISpawn.Count == 0)
            {
                if (audioSpeech.isPlaying)
                    uiScript.StartNotification(0.2f);
                else
                    uiScript.StartNotification();
            }
            else
                notificationUISpawn.Enqueue(ui);

            return ui;
        }

        // Spawn Interfaces --------------------------------------

        /// <summary>
        /// Spawns UI with given name.
        /// Default path is: MAGESres/UI/
        /// To load a UI from a custom path (after Resources) set
        /// _customPath to true
        /// </summary>
        /// <param name="_uiPath">path to UI</param>
        /// <param name="_customPath">set to false for default path, true for custom path after Resources</param>
        /// <returns></returns>
        public GameObject SpawnUI(string _uiPath, bool _customPath = false)
        {
            string path;
            if (_customPath)
                path = _uiPath;
            else
                path = uiPath + "OperationPrefabs/" + _uiPath;

            GameObject ui = Resources.Load(path, typeof(GameObject)) as GameObject;
            if (!ui)
            {
                Debug.LogError("UI Error: '" + path + "' prefab not found");
                return null;
            }

            string uiName = ui.name;
            ui = Instantiate(ui, interactiveUI);
            ui.name = uiName;

            // If Virtual Assistance is speaking do not make a loud spawn sound
            if (audioSpeech.isPlaying && ui.GetComponent<AudioSource>())
                ui.GetComponent<AudioSource>().volume = 0.2f;

            return ui;
        }


        public void SpawnProgressUI(string _message, bool _isActionCompleted = false)
        {
            // NOT FINALIZED PREFAB
            return;

            //PLEASE Do not delete we need to fix it
            /*
            if (string.IsNullOrEmpty(_message))
                return;

            // If no progressUI is psawned spawn a new one
            // otherwise update the text and replay the arrow animation on the existing one and reset the timer
            if (actionProgressUI.childCount == 0)
            {
                GameObject ui = (GameObject) Resources.Load(uiPath + "ActionProgress/ProgressUI") as GameObject;
                if (!ui)
                {
                    Debug.LogError("Error: " + uiPath + "ActionProgress/ProgressUI not found!");
                    return;
                }

                ui = Instantiate(ui, actionProgressUI);
                ui.GetComponentInChildren<Text>().text = _message;
            }
            else
            {
                GameObject ui = actionProgressUI.GetChild(0).gameObject;
                ui.GetComponentInChildren<Text>().text = _message;

                if (ui.GetComponent<GenericUIResetTimer>())
                    ui.GetComponent<GenericUIResetTimer>().ResetTimer();

                Transform arrowMask = ui.transform.Find("ArrowMask");
                if (!arrowMask || !arrowMask.GetComponentInChildren<Animator>(true))
                {
                    Debug.LogError("No ArrowMask child or Animator found in UI: " + ui.name);
                    return;
                }
                Image[] arrows = arrowMask.GetComponentsInChildren<Image>(true);
                if (_isActionCompleted)
                { 
                    foreach (Image arrow in arrows)
                        arrow.color = Color.green;
                }
                else
                {
                    foreach (Image arrow in arrows)
                        arrow.color = new Color32(0,155,255,255);
                }

                Animator anim = arrowMask.GetComponentInChildren<Animator>(true);
                anim.enabled = true;

                anim.Play("ActionProgressArrowAnim", -1, 0f);
            }

            */
        }

        // Destruction Methods -----------------------------------

        /// <summary>
        /// Destroy current active notification
        /// or all notifications waiting
        /// </summary>
        /// <param name="_destoryAll">set to false to destory only active one, true to destroy all</param>
        public void DestroyNotification(bool _destoryAll = false)
        {
            GameObject ui = GetActiveNotification();
            if (!ui)
                return;

            if (!ui.GetComponent<NotificationUI>())
                Destroy(ui);
            else
                ui.GetComponent<NotificationUI>().DestroyManually();

            for(int i =0; i < Get.notificationUISpawn.Count; ++i)
                Destroy(Get.notificationUISpawn.Dequeue());

            Get.notificationUISpawn.Clear();
        }

        public void ResetInterfaceManagement()
        {
            foreach (Transform child in Get.transform)
            {
                if (child.childCount == 0)
                    continue;

                foreach (Transform grandChild in child)
                    Destroy(grandChild.gameObject);
            }

            Get.notificationUISpawn.Clear();

            // DOES NOT WORK - FINDS AND RETURNS CHILDREN TOO
            // Find all UIs without parent and destroy them
            /*var goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
            var goList = new List<GameObject>();
            for (int i = 0; i < goArray.Length; i++)
            {
                if (goArray[i].layer == LayerMask.NameToLayer("UI") && goArray[i].name != Get.gameObject.name)
                    Destroy(goArray[i]);
            }*/
        }

        // Get State Methods -------------------------------------

        public GameObject GetActiveNotification()
        {
            if (notificationUI.childCount == 0)
                return null;

            GameObject ui = notificationUI.GetChild(0).gameObject;

            // if canvas is enabled then this is the active notification
            if (ui.GetComponent<Canvas>().enabled)
                return ui;
            else
                return null;
        }

        public List<GameObject> GetAllNotifications()
        {
            return notificationUISpawn.ToList<GameObject>();
        }

        public bool GetIsSpeechPlaying()
        {
            if (audioSpeech.isPlaying)
                return true;
            else
                return false;
        }

        // Get Key Translations ----------------------------------

        /// <summary>
        /// Change immediately the thext to the UI reference
        /// Has no Access from Utilities Mediator
        /// </summary>
        /// <param name="_uiText"></param>
        public void GetUIMessage(ref Text _uiText)
        {
            // Check Component State
            if (_uiText == null)
                return;

            // In case developer forgot empty spaces or new lines in Unity's Text Component inside Editor
            string _key = _uiText.text.Replace(" ", string.Empty);
            _key = _key.Replace("\n", string.Empty);
            _key = _key.Replace("\r", string.Empty);
            _key = _key.Replace("\t", string.Empty);

            if (string.IsNullOrEmpty(_key))
                return;

            LanguageTranslator keyToEnum;

            try
            { keyToEnum = (LanguageTranslator)System.Enum.Parse(typeof(LanguageTranslator), _key); }
            catch
            { Debug.LogError("Key does not exist in LanguageTranslator Enumerator"); return; }

            // Check Dictionary State
            if (UILanguageImporter.Get.languageData == null)
            {
                Debug.LogWarning("Text Dictionary is null or empty");
                return;
            }
            // Check Key in Dictionary
            if (!UILanguageImporter.Get.languageData.ContainsKey(keyToEnum))
            {
                Debug.LogWarning("Key not found In Text Dictionary: " + _key);
                return;
            }
            // Check Language if Contained in Key
            if (!UILanguageImporter.Get.languageData[keyToEnum].ContainsKey(Get.applicationLanguage))
            {
                Debug.LogWarning("Key In Text Dictionary does not contain currently selected App Language: " + Get.applicationLanguage);
                return;
            }
            // Check Message if Contained in Language
            if (string.IsNullOrEmpty(UILanguageImporter.Get.languageData[keyToEnum][Get.applicationLanguage]))
            {
                Debug.LogWarning("Message is null or empty, Text Dictionary with specific key and language: " + _key + " & " + Get.applicationLanguage);
                return;
            }

            _uiText.text = UILanguageImporter.Get.languageData[keyToEnum][Get.applicationLanguage];
        }

        public string GetUIMessage(LanguageTranslator _key)
        {
            // Check Dictionary State
            if (UILanguageImporter.Get.languageData == null)
            {
                Debug.LogWarning("Text Dictionary is null or empty");
                return _key.ToString();
            }
            // Check Key in Dictionary
            if (!UILanguageImporter.Get.languageData.ContainsKey(_key))
            {
                Debug.LogWarning("Key not found In Text Dictionary: " + _key);
                return _key.ToString();
            }
            // Check Language if Contained in Key
            if (!UILanguageImporter.Get.languageData[_key].ContainsKey(Get.applicationLanguage))
            {
                Debug.LogWarning("Key In Text Dictionary does not contain currently selected App Language: " + Get.applicationLanguage);
                return _key.ToString();
            }
            // Check Message if Contained in Language
            if (string.IsNullOrEmpty(UILanguageImporter.Get.languageData[_key][Get.applicationLanguage]))
            {
                Debug.LogWarning("Message is null or empty, Text Dictionary with specific key and language: " + _key + " & " + Get.applicationLanguage);
                return _key.ToString();
            }

            return UILanguageImporter.Get.languageData[_key][Get.applicationLanguage];
        }

        // Decision Interfaces -----------------------------------

        public void SetDecisionCorrectAction(GameObject _decisionPrefab, UnityAction _invokeAction)
        {
            _decisionPrefab.GetComponent<QuestionPrefabConstructor>().correctAnswerTrigger.AddListener(_invokeAction);

        }
        
        public void SetDecisionWrongAction(GameObject _decisionPrefab, UnityAction _invokeAction)
        {
            _decisionPrefab.GetComponent<QuestionPrefabConstructor>().wrongAnswerTrigger.AddListener(_invokeAction);
        }

        public void SetResultofDecision(List<bool> res)
        {
            resultOfDecision = res;
        }

        public List<bool> GetResultofDecision(GameObject decisionPrefab)
        {
            return resultOfDecision;
        }
        
    }
}