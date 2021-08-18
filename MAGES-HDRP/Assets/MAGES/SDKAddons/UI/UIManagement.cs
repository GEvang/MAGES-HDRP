/**
 * The way the UI works:
 * 
 * 1. Architecture
 * 
 *  - Application Generic: 
 * i. UserUITypes
 * UI prefabs that are called in every application without changes following the UI "architecture"
 * They can be accessed using the enum: UserUITypes
 * ii. NotificationTypes
 * Ui Notifications that can only be spawned not destoryed. The all have limited (visualized) lifetime.
 * They can be accessed using the enum: NotificationUITypes
 * 
 * - Application Specific:
 * UI prefabs that are created for specific applications. Their creation might follow the UI 
 * "aarchitecture" partially and contain extra scripts of visuals
 * They can be accessed using their name as a string
 * 
 * - Special Cases:
 * Special Created prefabs (UI or not e.g. a hologram) that are Generic for all applications
 * but they dont follow the "architecture" neither the priority system (explained below)
 * They can be accessed using the enum: SpecialCaseUITypes
 * 
 * 2. Priority
 * 
 * Notifications have the top priority. If a UserUI is spanwed in will be closed automatically in order for the notification
 * to be displayed. After the notification is done hte UserUI will come back. If a notifications is about to be spawned, and another one is
 * already active, it goes on a queue and waits for its turn till the other notifications are done.
 * 
 * The userUIs work the same way as teh notificationUIs regarding themselves. If aUserUI is called to be spawned and anotehr one is already on display
 * it waits in a queue its turn until the already active one gets destroyed
 * 
 * Special Cases dont follow any rules no matter what is at display they will be spawned. There is also no limit to the amount of these UIs to
 * be spawned.
 * 
 * Application Specific has no priority. It will be spawned whenever. Oly rule: only one UI msut be available at a time.
 * Since application Specific UIs have to do with explicit actions, no queue is used for later spawn.
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using ovidVR.sceneGraphSpace;
using UnityEngine.UI;
using System.Linq;
using ovidVR.UIManagement.LanguageText;
using ovidVR.GameController;

namespace ovidVR.UIManagement
{
    //public enum ApplicationLanguages { GR, ENG }

    public enum UISounds { Notification, Warning, Error, Correct, ButtonPress, SpawnUI, DestroyUI, SpawnQuiz };

    public class UIManagement : MonoBehaviour
    {
        public ApplicationLanguages applicationLanguage;

        [Header("Spawn Point for 6 D.O.F."), Tooltip("Where the UI is going to be spawned, Cameras > VRCamera > Camera Rig > Camera(eye) > UserUISpawnPoint")]
        public Transform spawnPoint;

        [Header("Spawn Point for 3 D.O.F."), Tooltip("Where the UI is going to be spawned, Set (w/ same values) spawn point to mobile VR camera")]
        public Transform spawnPoint3DOF;

        [Header("Spawn Point for 2 D.O.F."), Tooltip("Where the UI is going to be spawned, Set (w/ same values) spawn point of 3D camera")]
        public Transform spawnPoint2DOF;

        [HideInInspector]
        public Transform leftController;
        [HideInInspector]
        public Transform rightController;

        [SerializeField, Range(0f, 2.6f), Tooltip("Set the Pause-Timer between the ProgressUI spawns")]
        private float minPauseTimerProgressUI = 1.2f;

        [SerializeField, Tooltip("Add Sounds in the same order as the enum UISounds is created")]
        private List<AudioClip> allSounds;

        private AudioSource audioUI;
        private AudioSource audioSpeech;

        private Transform activeUserUI, activeNotificationUI, activeSpecialCaseUI, activeAppSpecificUI, activeProgressUI, activeQuiz;

        private string userUIPath, notificationUIPath, specialCaseUIPath, appSpecificUIPath, progressUIPath;

        private string currSuspendedUserUIName;

        private float currPauseTimerProgressUI;
        private bool newProgressUI;

        private Queue<string> userUISpawn;
        private Queue<string> notificationUISpawn;

        private List<string> progressUISpawn;

        // V2.0 START ------------------------------

        private GameObject fadeCameraCanvas;
        private List<Material> leftHandMats, rightHandMats;
        private Material uiHandMaterial;

        // V2.0 END --------------------------------

        public bool isSpeechPlaying;

        private static UIManagement uiManager;

        public static UIManagement Get
        {
            get
            {
                if (!uiManager)
                {
                    uiManager = FindObjectOfType(typeof(UIManagement)) as UIManagement;

                    if (!uiManager) { Debug.LogError("Error, No UIManagement was found"); }
                }

                return uiManager;
            }
        }

        private void Awake()
        {
            // Bind All Spawn + Destructor Methods to static Actions
            UIManagementMediator.SpawnUserUI = SpawnUserUI;
            UIManagementMediator.SpawnNotificationUI = SpawnNotificationUI;
            UIManagementMediator.SpawnSpecialCases = SpawnSpecialCases;
            UIManagementMediator.SpawnApplicationSpecificUI = SpawnApplicationSpecificUI;
            UIManagementMediator.SpawnProgressUI = SpawnProgressUI;
            UIManagementMediator.DestroyCurrentUI = DestroyCurrentUI;
            UIManagementMediator.DestroyCurrentNotificationUI = DestroyCurrentNotificationUI;
            UIManagementMediator.DestroyAllCurrentSpecialCaseUI = DestroyAllCurrentSpecialCaseUI;
            UIManagementMediator.DestroyCurrentApplicationSpecificUI = DestroyCurrentApplicationSpecificUI;
            UIManagementMediator.ResetUIManagement = ResetUIManagement;
            UIManagementMediator.ClearUIQueues = ClearUIQueues;
            UIManagementMediator.GetUIMessage = GetUIMessage;

            if (spawnPoint == null && spawnPoint3DOF == null)
                Debug.LogError("Both Spawn Points needed for UI are empty (Desktop & Mobile VR)");
            else
            {
                if (OvidVRControllerClass.Get.DOF == ControllerDOF.ThreeDOF)
                    spawnPoint = spawnPoint3DOF;
                if (OvidVRControllerClass.Get.DOF == ControllerDOF.TwoDOF)
                    spawnPoint = spawnPoint2DOF;
            }

            leftController = OvidVRControllerClass.Get.leftHand.transform;
            rightController = OvidVRControllerClass.Get.rightHand.transform;

            if (allSounds.Count == 0)
                Debug.LogError("UIManagement, No Sounds are given in list: allSounds");
                
            if(GetComponent<AudioSource>() != null)
            {
                Debug.LogError("Depricated UIManagement prefab. Destroy AudioSource and add it to a child: AudioUI. 3D Sound with Linear Rolloff (0.1 - 7)");
                audioUI = GetComponent<AudioSource>();
            }
            else if(transform.Find("AudioUI"))
                audioUI = transform.Find("AudioUI").GetComponent<AudioSource>();
            else
                Debug.LogError("No AudioSOurce found either in UIManagement or as a child AudioUI");

            if (transform.Find("AudioSpeech"))
                audioSpeech = transform.Find("AudioSpeech").GetComponent<AudioSource>();
            else
                Debug.LogError("NO AudioSpeech Child with AudioSource found. Add as a child: AudioSpeech. 3D Sound with Linear Rolloff (0.1 - 7)");

            activeUserUI = transform.Find("CurrentActiveUserUI");
            activeNotificationUI = transform.Find("CurrentActiveNotificationUI");
            activeSpecialCaseUI = transform.Find("CurrentActiveSpecialCaseUI");
            activeAppSpecificUI = transform.Find("CurrentActiveApplicationSpecificUI");

            activeProgressUI = transform.Find("CurrentActiveProgressUI")?.GetChild(0);
            activeQuiz = transform.Find("CurrentActiveOperationQuiz");

            userUIPath = "MAGESres/UI/ApplicationGeneric/UserUI/";
            notificationUIPath = "MAGESres/UI/ApplicationGeneric/NotificationUI/";
            specialCaseUIPath = "MAGESres/UI/SpecialCases/";
            appSpecificUIPath = "MAGESres/UI/ApplicationSpecific/";
            progressUIPath = "MAGESres/UI/ActionProgress/";

            // When a Progress UI is spawned there is a pause-timer for the next one to spawn
            currPauseTimerProgressUI = 0f;
            newProgressUI = false;

            userUISpawn = new Queue<string>();
            notificationUISpawn = new Queue<string>();
            progressUISpawn = new List<string>();

            // NotificationUI temporarily suspend current active userUI
            // and displays its name so the user will apprehend its re-appearance
            currSuspendedUserUIName = null;

            // V2.0 START ------------------------------
            leftHandMats = new List<Material>();
            rightHandMats = new List<Material>();
            uiHandMaterial = Resources.Load("MAGESres/UI/Materials/HandsZTestOff", typeof(Material)) as Material;
            if (uiHandMaterial == null)
                Debug.LogError("No HandsZTestOff found at Resources/MAGESres/UI/Materials");
        }

        private void Update()
        {
            if (notificationUISpawn.Count != 0)
                SpawnNotificationUIFromQueue();

            if (userUISpawn.Count != 0)
                SpawnUserUIFromQueue();

            if (progressUISpawn.Count != 0)
                SpawnProgressUIFromList();
        }

        private void SpawnUserUIFromQueue()
        {
            if (activeUserUI.childCount == 0 && activeNotificationUI.childCount == 0)
            {
                // reset current active UI name for notification
                currSuspendedUserUIName = null;

                string[] splitUIString = Regex.Split(userUISpawn.Dequeue(), "~~");
                string uiName = splitUIString[0];
                string isDynamic = splitUIString[1];

                GameObject ui = Instantiate((GameObject)Resources.Load(userUIPath + uiName) as GameObject, activeUserUI);

                if (ui == null)
                {
                    Debug.LogError("ERROR: User UI type NOT found: " + uiName);
                    return;
                }

                ui.name = uiName;    // Remove (Clone) at the end of spawned prefab

                UISpawn uiScript = ui.GetComponent<UISpawn>();

                if (isDynamic.Equals("dynamic"))
                    UpdateUITransformTorwardsUserHead(ui.transform);
                else
                {
                    if (uiScript != null)
                        uiScript.isDynamicAtSpawn = false;
                }
            }
        }

        private void SpawnNotificationUIFromQueue()
        {
            if (activeUserUI.childCount != 0)
            {
                GameObject ui = GetCurrentActiveUserUI();
                if (!ui.GetComponent<UISpawn>().isUIDestroyed)
                {
                    currSuspendedUserUIName = ui.name;

                    UISpawn uiScript = ui.GetComponent<UISpawn>();
                    if (uiScript == null || uiScript.isDynamicAtSpawn)
                        userUISpawn.Enqueue(ui.name + "~~dynamic");
                    else
                        userUISpawn.Enqueue(ui.name + "~~static");

                    DestroyCurrentUI();
                }

                return;
            }

            if (activeNotificationUI.childCount == 0)
            {
                string[] splitNotificationString = Regex.Split(notificationUISpawn.Dequeue(), "~~");

                string uiName = splitNotificationString[0];
                string uiMessage = splitNotificationString[1];
                float lifeTime;
                if (!float.TryParse(splitNotificationString[2], out lifeTime))
                    lifeTime = 10f;
                string uniqueID = splitNotificationString[3];
                string isDynamic = splitNotificationString[4];

                GameObject ui = Instantiate((GameObject)Resources.Load(notificationUIPath + uiName) as GameObject, activeNotificationUI);

                if(ui == null)
                {
                    Debug.LogError("ERROR: Notification UI type NOT found: " + uiName);
                    return;
                }

                ui.name = uiName;    // Remove (Clone) at the end of spawned prefab

                UINotificationSpawn uiScript = ui.GetComponent<UINotificationSpawn>();
                if (uiScript != null)
                    uiScript.InitializeNotification(uiMessage, currSuspendedUserUIName, lifeTime, uniqueID);

                if (isDynamic.Equals("dynamic"))
                    UpdateUITransformTorwardsUserHead(ui.transform);
                else
                {
                    if (uiScript != null)
                        uiScript.isDynamicAtSpawn = false;
                }
            }
        }

        private void SpawnProgressUIFromList()
        {
            if (activeProgressUI.childCount != 0)
            {
                Get.currPauseTimerProgressUI += Time.deltaTime;

                if (Get.currPauseTimerProgressUI < Get.minPauseTimerProgressUI)
                    return;
            }

            if (!newProgressUI)
            {
                progressUISpawn.Clear();
                return;
            }

            Get.currPauseTimerProgressUI = 0f;

            string[] splitUIString = Regex.Split(progressUISpawn.LastOrDefault(), "~~");
            string uiName = splitUIString[0];
            string uiMessage = splitUIString[1];

            GameObject ui = Instantiate((GameObject)Resources.Load(progressUIPath + uiName) as GameObject, activeProgressUI);
            ui.name = uiName;

            Text uiText = ui.GetComponentInChildren<Text>();
            uiText.text = uiMessage;

            newProgressUI = false;
        }

        private void UpdateUITransformTorwardsUserHead(Transform _uiTransform, bool _preserveY = true)
        {
            Vector3 newPos = spawnPoint.position;
            if (_preserveY)
                newPos.y = _uiTransform.position.y;
            _uiTransform.position = newPos;

            Quaternion newRot = spawnPoint.rotation;
            newRot.z = 0; newRot.x = 0;
            _uiTransform.rotation = newRot;
        }

        // Public Methods ------------------------------------------

        public static void PlaySound(UISounds _sound)
        {
            if (Get.audioUI == null || Get.isSpeechPlaying)
                return;

            if (Get.audioUI.isPlaying)
                Get.audioUI.Stop();

            Get.audioUI.clip = Get.allSounds[(int)_sound];
            Get.audioUI.volume = 1f;
            Get.audioUI.Play();
        }

        public static void PlaySpeech(LanguageSpeech _speech, bool _playVirtualSpeakerAnim = true)
        {
            Get.isSpeechPlaying = true;

            string fullFilePath = "Sounds/" + Configuration.ProductCode + "/Speech/" + _speech.ToString() + "/" + Get.applicationLanguage.ToString() + "/";

            Object[] clip = Resources.LoadAll(fullFilePath, typeof(AudioClip));

            if (clip == null || clip.Length == 0)
            {
                Debug.LogError("No Speech sound file found in: " + fullFilePath);
                return;
            }

            if (Get.audioUI.isPlaying)
            {
                //Make this fade out
                Get.audioUI.Stop();
            }

            Get.audioSpeech.clip = (AudioClip) clip[0];
            Get.audioSpeech.Play();
            Get.StartCoroutine(WaitForAudioSource(Get.audioSpeech));

            // If Audio is updated with simple Animation
            if (!_playVirtualSpeakerAnim)
                return;

            Animation audioVSAnim = Get.audioSpeech.gameObject.GetComponent<Animation>();
            if (audioVSAnim && !audioVSAnim.isPlaying)
                audioVSAnim.Play();
        }

        private static IEnumerator WaitForAudioSource(AudioSource aSource)
        {
            while (aSource.isPlaying)
            {
                yield return null;
            }

            Get.isSpeechPlaying = false;
        }

        // Spawn Methods -------------------------------------------

        /// <summary>
        /// UserUI by default must be spawned as dynamic.
        /// Dynamic means that it's spawned in front of the user. Then it remains static
        /// If lifetime is given 0 value, then it is regarded as infinite, use with caution
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="_spawnAsDynamic"></param>
        public static void SpawnUserUI(UserUITypes _type, bool _spawnAsDynamic = true)
        {
            string dynamicUI = _type.ToString() + "~~" + "dynamic";
            string staticUI = _type.ToString() + "~~" + "static";

            // If UI is already waiting to be spawned inside the queue, do not re add
            if (Get.userUISpawn.Contains(dynamicUI) || Get.userUISpawn.Contains(staticUI))
                return;

            if (_spawnAsDynamic)
                Get.userUISpawn.Enqueue(dynamicUI);
            else
                Get.userUISpawn.Enqueue(staticUI);
        }

        /// <summary>
        /// Spawn a generic notification UI. Select the type of UI, the key for it's translated message and how long it should last
        /// Also you can select a uniqueID to be held in its script (UINotificationSpawn) to recognize it from the other ones
        /// and if you want it to be spawneddynamically in front of the user or not
        /// 
        /// +1 Overload
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="_displayMessageKey"></param>
        /// <param name="_lifeTime">[0,30] seconds</param>
        /// <param name="uniqueID"></param>
        /// <param name="_spawnAsDynamic"></param>
        public static void SpawnNotificationUI(NotificationUITypes _type, LanguageTranslator _displayMessageKey, float _lifeTime, string uniqueID = "", bool _spawnAsDynamic = true)
        {
            _lifeTime = Mathf.Clamp(_lifeTime, 0f, 30f);

            string translatedMessage = GetUIMessage(_displayMessageKey);

            if (_spawnAsDynamic)
                Get.notificationUISpawn.Enqueue(_type.ToString() + "~~" + translatedMessage + "~~" + _lifeTime.ToString() + "~~" + uniqueID + "~~" + "dynamic");
            else
                Get.notificationUISpawn.Enqueue(_type.ToString() + "~~" + translatedMessage + "~~" + _lifeTime.ToString() + "~~" + uniqueID + "~~" + "static");
        }

        public static void SpawnSpecialCases(string _uiName, Transform _parent = null, bool _spawnAsDynamic = true)
        {
            GameObject ui = Instantiate((GameObject)Resources.Load(Get.specialCaseUIPath + _uiName) as GameObject, Get.activeSpecialCaseUI);
            ui.name = _uiName;
            
            if (_spawnAsDynamic)
                Get.UpdateUITransformTorwardsUserHead(ui.transform, false);

            if (ui.GetComponent<OvidVRParenting>() != null && _parent != null)
                ui.GetComponent<OvidVRParenting>().parentTransform = _parent;

        }

        /// <summary>
        /// ApplicationSpecificUI by default must be spawned as static
        /// </summary>
        /// <param name="_uiName"></param>
        /// <param name="_spawnAsDynamic"></param>
        public static void SpawnApplicationSpecificUI(string _uiName, bool _spawnAsDynamic = false)
        {
            if (Get.activeAppSpecificUI.childCount != 0)
            {
                Debug.LogError("No more than one ApplicationSpecific UI can exist at the same time");
                return;
            }

            GameObject loadUI = Resources.Load(Get.appSpecificUIPath + _uiName) as GameObject;

            if (loadUI == null)
            {
                Debug.LogError("NO UI prefab '" + _uiName + "' found in: " + Get.appSpecificUIPath);
                return;
            }

            GameObject ui = Instantiate(loadUI, Get.activeAppSpecificUI);
            ui.name = _uiName;

            if (_spawnAsDynamic)
                Get.UpdateUITransformTorwardsUserHead(ui.transform);
        }

        /// <summary>
        /// Called Internally from Prefab Constructors when a new progress callback is available
        /// 
        /// Progress UIs are always spawned in a specific location
        /// (the location where the prefabs were "Applied" inside the Unity Scene)
        /// </summary>
        public static void SpawnProgressUI(string _message, bool _isActionCompleted = false)
        {
            // Do not spawn a new progressUI if the current active is the action's completion UI
            if (Get.activeProgressUI.childCount != 0 &&
                Get.activeProgressUI.GetChild(0).gameObject.name == ProgressUITypes.ProgressUI100.ToString())
                return;

            if (string.IsNullOrEmpty(_message))
                return;

            Get.newProgressUI = true;

            string progressUIName = ProgressUITypes.ProgressUI.ToString();
            if (_isActionCompleted)
            {
                progressUIName = ProgressUITypes.ProgressUI100.ToString();
                Get.progressUISpawn.Clear();

                foreach (Transform child in Get.activeProgressUI)
                    Destroy(child.gameObject);
            }

            Get.progressUISpawn.Add(progressUIName + "~~" + _message);
        }

        // Destruction Methods -------------------------------------
        public static void DestroyCurrentUI(bool _destroyImmediate = false)
        {
            GameObject ui = GetCurrentActiveUserUI();
            if (ui != null && !_destroyImmediate)
            {
                UISpawn uiScript = ui.GetComponent<UISpawn>();
                if (uiScript == null)
                    Destroy(ui);
                else
                    uiScript.DestroyUI();
            }

            // Sometimes Expanding Colliders remain, delete All children when _destroyImmediate is called
            if (_destroyImmediate)
            {
                foreach (Transform child in Get.activeUserUI)
                    Destroy(child.gameObject);
            }
        }

        public static void DestroyCurrentNotificationUI(bool _destroyImmediate = false)
        {
            GameObject ui = GetCurrentNotificationUI();

            if (ui != null && !_destroyImmediate)
            {
                UINotificationSpawn uiScript = ui.GetComponent<UINotificationSpawn>();
                if (_destroyImmediate || uiScript == null)
                    Destroy(ui);
                else
                    uiScript.DestroyNotification();
            }

            if (_destroyImmediate)
            {
                foreach (Transform child in Get.activeNotificationUI)
                    Destroy(child.gameObject);
            }
        }

        public static void DestroyAllCurrentSpecialCaseUI()
        {
            if (Get.activeSpecialCaseUI.childCount != 0)
            {
                foreach (Transform child in Get.activeSpecialCaseUI)
                    Destroy(child.gameObject);
            }
        }

        public static void DestroyCurrentApplicationSpecificUI(bool _destroyImmediate = false)
        {
            GameObject childUI = GetCurrentApplicationSpecificUI();

            if (childUI != null)
            {
                if (childUI.GetComponent<UISpawn>() && !_destroyImmediate)
                    childUI.GetComponent<UISpawn>().DestroyUI(true);
                else
                    Destroy(childUI.gameObject);
            }
        }

        /// <summary>
        /// Hard Destroy all current UI. Clear all queues and lists.
        /// </summary>
        public static void ResetUIManagement()
        {
            foreach (Transform child in Get.transform)
            {
                if (child.childCount == 0)
                    continue;

                //AudioSpeech has always onlyone child, it's animator for Virtual Speaker
                if (child.gameObject.name == "AudioSpeech")
                    continue;

                if (child.gameObject.name == Get.activeProgressUI.parent.gameObject.name)
                {
                    foreach (Transform grandChild in child.GetChild(0))
                        Destroy(grandChild.gameObject);
                }
                else
                {
                    foreach (Transform grandChild in child)
                        Destroy(grandChild.gameObject);
                }
            }

            Get.userUISpawn.Clear();
            Get.notificationUISpawn.Clear();
            Get.progressUISpawn.Clear();

            Get.currPauseTimerProgressUI = 0f;
            Get.currSuspendedUserUIName = null;
            Get.newProgressUI = false;
        }

        /// <summary>
        /// Clears ONLY queues and lists with UIs waiting to be spawned
        /// </summary>
        public static void ClearUIQueues()
        {
            Get.userUISpawn.Clear();
            Get.notificationUISpawn.Clear();
            Get.progressUISpawn.Clear();

            Get.currSuspendedUserUIName = null;
        }

        // Get State Methods ---------------------------------------

        public static GameObject GetCurrentActiveUserUI()
        {
            if (Get.activeUserUI.childCount != 0)
            {
                GameObject ui = Get.activeUserUI.GetChild(0).gameObject;

                // If UI is currently auto-destroyed with animation during the call of this function, count as destroyed
                UINotificationSpawn uiScript = ui.GetComponent<UINotificationSpawn>();
                if (uiScript != null && uiScript.isUIDestroyed)
                    return null;
                else
                    return ui;
            }

            return null;
        }

        public static GameObject GetCurrentNotificationUI()
        {
            if (Get.activeNotificationUI.childCount != 0)
            {
                GameObject ui = Get.activeNotificationUI.GetChild(0).gameObject;

                // If UI is currently auto-destroyed with animation during the call of this function, count as destroyed
                UINotificationSpawn uiScript = ui.GetComponent<UINotificationSpawn>();
                if (uiScript != null && uiScript.isUIDestroyed)
                    return null;
                else
                    return ui;
            }

            return null;
        }

        /// <summary>
        /// Application Generic UIs are considered only the:
        /// UserUIs (the ones that are interactable with the user, e.g. OperationStart UI)
        /// NotificationUIs (Notification, Warning, Error)
        /// 
        /// These are the ones used the most, this function is a faster and more generic way
        /// to know if a UIis spawned. It first searches teh Notifications (higher priority)
        /// and then the UserUI.
        /// 
        /// If a notification is found it returns it's unique ID (stored inside the script)
        /// of if it is an UserUI it returns it's gameobject name
        /// </summary>
        /// <returns>Null if nothing found, else the name or string ID</returns>
        public static string GetCurrentApplicationGenericUIID()
        {
            GameObject ui = GetCurrentNotificationUI();

            if (ui != null)
                return ui.GetComponent<UINotificationSpawn>().NotificationUniqueID;

            ui = GetCurrentActiveUserUI();

            if (ui != null)
                return ui.name;

            return null;
        }

        public static List<GameObject> GetAllCurrentSpecialCaseUI()
        {
            if (Get.activeSpecialCaseUI.childCount != 0)
            {
                List<GameObject> allChildren = new List<GameObject>();

                foreach (Transform child in Get.activeSpecialCaseUI)
                    allChildren.Add(child.gameObject);

                return allChildren;
            }

            return null;
        }

        public static GameObject GetCurrentApplicationSpecificUI()
        {
            if (Get.activeAppSpecificUI.childCount != 0)
                return Get.activeAppSpecificUI.GetChild(0).gameObject;

            return null;
        }

        public static GameObject GetCurrentActiveQuiz()
        {
            if (Get.activeQuiz.childCount != 0)
                return Get.activeQuiz.GetChild(0).gameObject;

            return null;
        }

        // Get Messages from Translator Keys ---------------------

        public static void GetUIMessage(ref Text _uiText)
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
            if (!UILanguageImporter.Get.languageData[keyToEnum].ContainsKey(UIManagement.Get.applicationLanguage))
            {
                Debug.LogWarning("Key In Text Dictionary does not contain currently selected App Language: " + UIManagement.Get.applicationLanguage);
                return;
            }
            // Check Message if Contained in Language
            if (string.IsNullOrEmpty(UILanguageImporter.Get.languageData[keyToEnum][UIManagement.Get.applicationLanguage]))
            {
                Debug.LogWarning("Message is null or empty, Text Dictionary with specific key and language: " + _key + " & " + UIManagement.Get.applicationLanguage);
                return;
            }

            _uiText.text = UILanguageImporter.Get.languageData[keyToEnum][UIManagement.Get.applicationLanguage];
        }

        public static string GetUIMessage(LanguageTranslator _key)
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
            if (!UILanguageImporter.Get.languageData[_key].ContainsKey(UIManagement.Get.applicationLanguage))
            {
                Debug.LogWarning("Key In Text Dictionary does not contain currently selected App Language: " + UIManagement.Get.applicationLanguage);
                return _key.ToString();
            }
            // Check Message if Contained in Language
            if (string.IsNullOrEmpty(UILanguageImporter.Get.languageData[_key][UIManagement.Get.applicationLanguage]))
            {
                Debug.LogWarning("Message is null or empty, Text Dictionary with specific key and language: " + _key + " & " + UIManagement.Get.applicationLanguage);
                return _key.ToString();
            }

            return UILanguageImporter.Get.languageData[_key][UIManagement.Get.applicationLanguage];
        }

        public static bool GetIsCameraFaded()
        {
            if (Get.fadeCameraCanvas == null)
                return false;
            else
                return true;
        }

        public static void CameraFade(bool _fadeIn)
        {
            SkinnedMeshRenderer leftHandRend = OvidVRControllerClass.Get.leftHand.GetComponentInChildren<SkinnedMeshRenderer>();
            SkinnedMeshRenderer rightHandRend = OvidVRControllerClass.Get.rightHand.GetComponentInChildren<SkinnedMeshRenderer>();

            if (_fadeIn)
            {
                if (Get.fadeCameraCanvas != null)
                    return;

                // Hand Materias Change ----------------------------
                // Store original hand materials. When fade in is called again to restore materials
                Get.leftHandMats.AddRange(leftHandRend.materials);
                Get.rightHandMats.AddRange(rightHandRend.materials);

                List<Material> uiMats = new List<Material>();
                for (int i = 0; i < Get.leftHandMats.Count; ++i)
                    uiMats.Add(Get.uiHandMaterial);

                leftHandRend.materials = uiMats.ToArray();
                rightHandRend.materials = uiMats.ToArray();

                // Spawn Camera UI Fade ----------------------------
                GameObject cameraFadePrefab = Resources.Load("MAGESres/UI/CameraFadeUI/UICameraFade", typeof(GameObject)) as GameObject;
                if (!cameraFadePrefab)
                {
                    Debug.LogError("No UICameraFade found in path: Resources/MAGESres/UI/CameraFadeUI");
                    return;
                }

                Get.fadeCameraCanvas = Instantiate(cameraFadePrefab, Get.spawnPoint);
                Get.StartCoroutine(Get.BeginFade(1f));
            }
            else
            {
                if (Get.fadeCameraCanvas == null)
                    return;

                // Hand Materias Change ----------------------------
                if (Get.leftHandMats.Count != 0)
                    leftHandRend.materials = Get.leftHandMats.ToArray();
                else
                    Debug.LogError("List does not contain original left hand materials! Has CameraFade(true) called before?");

                if (Get.rightHandMats.Count != 0)
                    rightHandRend.materials = Get.rightHandMats.ToArray();
                else
                    Debug.LogError("List does not contain original right hand materials! Has CameraFade(true) called before?");

                Get.leftHandMats.Clear();
                Get.rightHandMats.Clear();

                // Destroy Camera UI Fade --------------------------
                Get.StartCoroutine(Get.BeginFade(0f));
            }
        }

        private IEnumerator BeginFade(float _endAlpha)
        {
            Image uiImage = Get.fadeCameraCanvas.GetComponentInChildren<Image>();

            Color startAlphaC = uiImage.color;

            Color endAlphaC = uiImage.color;
            endAlphaC.a = _endAlpha;

            float timer = 0f;
            while (timer <= 2f)
            {
                uiImage.color = Color.Lerp(startAlphaC, endAlphaC, timer / 2f);
                timer += Time.deltaTime;

                yield return null;
            }

            // For fade out destory the Canvas Gameobject
            if (_endAlpha == 0)
                Destroy(Get.fadeCameraCanvas);
        }
    }
}