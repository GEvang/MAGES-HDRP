using ovidVR.GameController;
using ovidVR.OperationAnalytics;
using ovidVR.sceneGraphSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ovidVR.UIManagement;
using System.Net;
using System.Net.Sockets;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using ovidVR.Utilities;
using Photon;
using Photon.Realtime;
using Photon.Pun;

public class UIFunctionsCoop : MonoBehaviourPunCallbacks
{

    License.LicenseType mLisenceType = License.LicenseType.None;
    
    private GameObject networkContrG;

    private bool serverMatchStarted = false;

    [SerializeField]
    private UnityEvent onStartFunctions;

    private Text netWorkingInfoTextReference;

    [HideInInspector]
    public static string ConnectedRoomName;

    [HideInInspector]
    public bool RefreshNow = false;

    private RoomInfo[] roomsInfo;

    /// <summary>
    /// Do NOT Use Awake or Enable. Only Start.
    /// Initialization should be done when the UI appears on the scene
    /// </summary>
    private void Start()
    {
        mLisenceType = sceneGraph.GetLicenseType();        
    
        if (onStartFunctions != null)
            onStartFunctions.Invoke();


        NetworkControllerPhoton.Instance.EstablishConnection();
        
    }

    public override void OnDisable()
    {
        StopAllCoroutines();
    }

    //ToDo:
    public override void OnDisconnected(DisconnectCause cause)
    {
        UIManagement.SpawnNotificationUI(NotificationUITypes.UINotification, LanguageTranslator.NoInternetConnectionWarningNotification, 0f);

        Debug.LogError("(On Match Create) The PC seems to be disconnected from the network.\nCheck the internet connection. " + "Network Error" + cause);
        
        base.OnDisconnected(cause);
    }

    
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
        
    }

    public IEnumerator LateJoinLobby()
    {
        yield return 0;
    }
    public void StartServerMatch()
    {
        if (serverMatchStarted)
            return;

        serverMatchStarted = true;

        string multiplayerRoomName = UserAccountManager.Get.GetUsername();
        multiplayerRoomName = multiplayerRoomName + "_" + UnityEngine.Random.Range(0, 99).ToString();

        ConnectedRoomName = UserAccountManager.Get.GetUsername();

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "isDemo", mLisenceType == License.LicenseType.Demo } };
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "isDemo" };

        roomOptions.MaxPlayers = 25;
        PhotonNetwork.CreateRoom(multiplayerRoomName, roomOptions, null);
    }

    public void StopServerMatch()
    {
        StopAllCoroutines();

        OvidVRControllerClass.Get.IsInNetwork = false;
        OvidVRControllerClass.Get.isServer = false;
        OvidVRControllerClass.Get.isClient = false;

        PhotonNetwork.CurrentRoom.IsOpen = false;        
    }

    public void UpdateServerMatches(GameObject _ui)
    {    
        StartCoroutine(RefreshServerMatches(_ui));
    }

    public void SetPlayerSpectator()
    {
        OvidVRControllerClass.NetworkManager.SetIsSpectator(true);
    }

    public void JoinServerMatch()
    {
        //    if (netManager.matchMaker == null)
        //    {
        //        Debug.LogError("Tried to Log into a session with the MatchMaker disabled!");
        //        return;
        //    }
        //if (netManager.matches == null)
        //    Debug.LogError("netManager had not Initialized its sessions in 'JoinServerMatch'. Call first 'UpdateServerMatches'");

        // Permanent button text in this case is the Name of the user. No reason to be translated from UIManagement

        string _roomName="";

        Transform baseUI = gameObject.transform.Find("Sessions");
        foreach (Transform roomUI in baseUI)
        {
            Text currentText = roomUI.Find("ButtonText").GetComponent<Text>();
            if (currentText.color == new Color(0, 0.6f, 1, 1))
            {
                _roomName = currentText.text;
            }
        }

        if (string.IsNullOrEmpty(_roomName))
        {
            Debug.LogError("Button Text containing the Session name is empty");
            return;
        }
        PhotonNetwork.JoinRoom(_roomName);        
    }

    public void SetServerTitle(GameObject _ui)
    {
        if (_ui == null)
            return;

        StartCoroutine(SetServerTitleAfterInit( _ui));
        
    }

    private IEnumerator SetServerTitleAfterInit(GameObject _ui)
    {
        Text uiText = _ui.GetComponentInChildren<Text>();
        while (PhotonNetwork.CurrentRoom == null)
        {
            yield return null;   
        }
        
        string str = PhotonNetwork.CurrentRoom.Name;
        while (string.IsNullOrEmpty(str))
        {
            str = PhotonNetwork.CurrentRoom.Name;
            yield return null;
        }
        
        uiText.text = str;
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    public void CreateTestNetwork()
    {
        if (networkContrG == null)
            return;

        if (networkContrG.GetComponent<TestNetwork>() == null)
            networkContrG.AddComponent<TestNetwork>();
    }

    public void DestroyTestNetwork()
    {
        if (networkContrG == null)
            return;

        if (networkContrG.GetComponent<TestNetwork>() != null)
            Destroy(networkContrG.GetComponent<TestNetwork>());
    }

    public void StartOperationSession()
    {
        //ToDo
        //GameObject toolsNet = Instantiate(NetworkManager.singleton.spawnPrefabs[2]);
        //NetworkServer.Spawn(toolsNet);
        PerformDelayed();
    }

    public void Restart()
    {
        //UserAccountManager.IncreaseUserSession();

        UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
        if (scene != null)
            SceneManager.LoadScene(scene.name);
    }
    
    public void EnableOnLobbyConnection(GameObject _ui)
    {
        StartCoroutine(CheckInLobby(_ui));

    }
    // Enumerators + Private -------------------------------------
    private IEnumerator CheckInLobby(GameObject _ui)
    {
        while (!PhotonNetwork.InLobby)
        {
            yield return null;
        }
        if (_ui == null)
        {
            Debug.LogError("The ui passed at checkInLobby function is null.");
            yield break;
        }
        
        var currentBbh = _ui.GetComponent<ButtonBehavior>();
        if (currentBbh) currentBbh.ButtonActivation(true);
        else Debug.LogError("The ui passed at checkInLobby function does not contain a buttonBehaviour");
    }
    
    private void PerformDelayed()
    {
        Operation.Get.Perform();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        roomsInfo = roomList.Where(r => ((bool)r.CustomProperties["isDemo"] == (mLisenceType == License.LicenseType.Demo))).ToArray();


    }

    private IEnumerator RefreshServerMatches(GameObject _ui)
    {


        var buttonUiBase = _ui.transform.Find("Sessions");

        if (!buttonUiBase)
            yield break;

        int buttonCounter = 0;
        bool firstEntry = true;

        Dictionary<string,GameObject> allButtons = new Dictionary<string, GameObject>();
        GameObject buttonPrefab = Resources.Load("MAGESres/UI/OperationPrefabs/ButtonTemplates/SessionButton") as GameObject;
        float startingTop=0;
        if (buttonPrefab != null)
        {
            RectTransform startTransform = buttonPrefab.GetComponent<RectTransform>();
            startingTop = startTransform.offsetMax.y;
        }
        
        // 6 seconds refresh
        while (true)
        {                
            // Do not wait for refresh on first entry
            if (!firstEntry)
            {
                if (RefreshNow)
                {
                        Transform allSessions = transform.Find("Sessions");
                        foreach(Transform child in allSessions)
                        {
                            Destroy(child.gameObject);
                        }
                        RefreshNow = false;
                        yield break;
                }
                yield return new WaitForSeconds(6f);
            }

            //uiText.text = UIManagement.GetUIMessage(LanguageTranslator.RefreshSessionsHeader);
            yield return new WaitForSeconds(1f);            

            if (roomsInfo == null || roomsInfo.Length == 0)
            {
                //uiText.text = UIManagement.GetUIMessage(LanguageTranslator.NoSessionFoundHeader);
                firstEntry = true;
                continue;
            }
            
            List<string> keysToDestroy = new List<string>();
            
            //Parse matches and delete old UI buttons
            foreach (var button in allButtons)
            {
                bool found = false;

                //Added for search bar purposes -- If the text of a button is no more matching with the text of the search bar, destroy the button/session.
                //----------------------------------------------------------------------------------------------------------------------------------------------
                if ((button.Value.transform != null) && (button.Value.transform.Find("ButtonText")))
                {
                    if (button.Value.transform.Find("ButtonText").GetComponent<Text>().text.Length - GetStringDistance(button.Value.transform.Find("ButtonText").GetComponent<Text>().text) <= 0)
                    {
                        Destroy(button.Value);
                        keysToDestroy.Add(button.Key);
                        continue;
                    }
                }
                //-----------------------------------------------------------------------------------------------------------------------------------------------

                foreach (var roomInfo in roomsInfo)
                {
                    if (!button.Key.Equals(roomInfo.Name)) continue;
                    
                    Text text = button.Value.transform.Find("UsersText").GetComponent<Text>();
                    text.text = roomInfo.PlayerCount.ToString();
                    found = true;
                }
                if (found) continue;
                
                Destroy(button.Value);
                keysToDestroy.Add(button.Key);

            }

            foreach (var key in keysToDestroy)
            {
                allButtons.Remove(key);
            }
            RepositionSessionsButtons(allButtons, startingTop);

            // Parse matches and spawn new UI buttons
            for (int i = 0; i < roomsInfo.Length; i++)
            {
                var match = roomsInfo[i];            
                

                string matchName = match.Name;

                //Added for search bar purposes -- If the name of the button that will be added does not match the text of the search bar, it will not be added.
                //----------------------------------------------------------
                if (matchName.Length - GetStringDistance(matchName) <= 0)
                    continue;
                //----------------------------------------------------------


                //Add new server button
                if (!allButtons.ContainsKey(matchName))
                {
                    GameObject newSession = Instantiate(buttonPrefab, buttonUiBase);
                    RectTransform newSessionTransform = newSession.GetComponent<RectTransform>();
                    
                    //Procedural positioning of buttons
                    var offsetMax = newSessionTransform.offsetMax;
                    offsetMax = new Vector2(offsetMax.x, startingTop-4f*allButtons.Count);
                    newSessionTransform.offsetMax = offsetMax;
                    newSession.transform.Find("ButtonText").GetComponent<Text>().text = matchName;
                    Text newSessionMembers = newSession.transform.Find("UsersText").GetComponent<Text>();
                    newSessionMembers.text = match.PlayerCount.ToString();
                    
                    newSession.GetComponent<ButtonBehavior>().buttonFunction.AddListener((() =>
                    {
                        SelectCoopButton(buttonUiBase.gameObject,newSession);
                    }));
                    
                    newSession.GetComponent<ButtonBehavior>().buttonFunction.AddListener((() =>
                    {
                        GameObject JoinButton = GameObject.Find("NetworkingUI(Clone)/AvailableSessions/JoinButton");
                        if (JoinButton)
                        {
                            var joinBtn = JoinButton.gameObject;
                            EnableDisableButton(joinBtn, newSession);
                        }
                    }));
                   
                    allButtons.Add(matchName,newSession);
                }
                
                ++buttonCounter;
            }

            buttonCounter = 0;
            if (firstEntry)
                firstEntry = false;
        }
    }

    private void RepositionSessionsButtons(Dictionary<string,GameObject> UIs,float startingTop)
    {
        int index = 0;
        foreach (var ui in UIs)
        {
            RectTransform sessionTransform = ui.Value.GetComponent<RectTransform>();
            var offsetMax = sessionTransform.offsetMax;
            offsetMax = new Vector2(offsetMax.x, startingTop-4f*index);
            sessionTransform.offsetMax = offsetMax;
            index++;
        }
    }
    
    private void SelectCoopButton(GameObject _buttonListParent , GameObject clickedUI)
    {
        
        foreach (Transform _uiButton in _buttonListParent.transform)
        {
            if (clickedUI.GetInstanceID() == _uiButton.gameObject.GetInstanceID())
            {
                if (_uiButton.Find("ButtonText").GetComponent<Text>().color == new Color(1,1,1, 1))
                {
                    _uiButton.Find("ButtonText").GetComponent<Text>().color = new Color(0, 0.6f, 1,1);
                    _uiButton.Find("UsersText").GetComponent<Text>().color = new Color(0, 0.6f, 1,1);
                }
                else
                {
                    _uiButton.Find("ButtonText").GetComponent<Text>().color = new Color(1,1,1,1);
                    _uiButton.Find("UsersText").GetComponent<Text>().color = new Color(1,1,1,1);
                }

            }
            else
            {
                _uiButton.Find("ButtonText").GetComponent<Text>().color = new Color(1,1,1,1);
                _uiButton.Find("UsersText").GetComponent<Text>().color = new Color(1,1,1,1);
            }
        }
    }

    private void EnableDisableButton(GameObject _uiButton, GameObject pressedBtn)
    {
        ButtonBehavior bH = _uiButton.GetComponent<ButtonBehavior>();
        ButtonBehavior pressedBH = pressedBtn.GetComponent<ButtonBehavior>();
        
        if (!bH ) return;
        bH.ButtonActivation(pressedBtn.transform.Find("ButtonText").GetComponent<Text>().color ==
                            new Color(0f, 0.6f, 1, 1));
    }

    //Function added for the purposes of search bar. It calculates the distance between two string using the Levenshtein distance algorithm.
    private int GetStringDistance(string compareString)
    {
        string searchString = "";

        if (GameObject.Find("AvailableSessions/Search"))
        {
            searchString = GameObject.Find("AvailableSessions/Search").GetComponent<InputField>().text;
            if (searchString == "")
                return 0;
        }
        else
        {
            return 0;
        }

        int n = searchString.Length;
        int m = compareString.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0)
            return m;

        if (m == 0)
            return n;

        for (int i = 0; i <= n; d[i, 0] = i++) ;


        for (int j = 0; j <= m; d[0, j] = j++) ;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (compareString[j - 1] == searchString[i - 1]) ? 0 : 1;

                d[i, j] = System.Math.Min(
                    System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }

    
}
