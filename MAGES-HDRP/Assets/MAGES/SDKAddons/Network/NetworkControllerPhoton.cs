using System;
using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.sceneGraphSpace;
using ovidVR.Utilities;
using ovidVR.toolManager.tool;
using ovidVR.toolManager;
using ovidVR.Utilities.prefabSpawnManager.prefabSpawnConstructor;
using System.Linq;
using ovidVR.ActionPrototypes;
using ovidVR.Networking;
using Photon.Pun;
using Photon.Realtime;
using Random = UnityEngine.Random;

public class NetworkControllerPhoton : MonoBehaviourPunCallbacks
{
    List<Color> avatarDefaultColorList;
    private static NetworkControllerPhoton instance = null;

    public static NetworkControllerPhoton Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<NetworkControllerPhoton>();
            return instance;
        }

        set
        {
            instance = value;
        }
    }
    short playerNumber = -1;
    public string customAvatarPath;
    [NonSerialized] public string Region="";
    /*
    Start Function responsible for registering or spawnable objects
    This functionality needs to happen in a coroutine thus it was added in IEnumarator start
    and not in awake.
    */
    private IEnumerator Start()
    {

        PhotonNetwork.SendRate = 50;
        PhotonNetwork.SerializationRate = 50;
        
        int loadPerFrame =5;
        int loaded = 0;
        
        object[] o = Resources.LoadAll("LessonPrefabs/");
        foreach (object n in o)
        {
            try
            {
                GameObject go = (GameObject) n;
                if (go.GetComponent<INetworkID>()!=null)
                {
                    //Todo : Add spawnable prefab to new "pooling" system
                    //NetworkFunctions.RegisterPrefabToScene(go);

                }
            }
            catch { }
            
            loaded++;
            if (loaded <= loadPerFrame) continue;
            
            loaded = 0;
            yield return 0;
        }

        object[] t = Resources.LoadAll("MAGESres\\NetworkR\\Tools\\");
        foreach (object g in t)
        {
            try
            {
                //Todo : Add spawnable prefab
                //NetworkFunctions.RegisterPrefabToScene((GameObject)g);
            }
            catch (System.Exception)
            {

            }
            loaded++;
            if (loaded <= loadPerFrame) continue;
            
            loaded = 0;
            yield return 0;
        }
    }
    
    private void Awake()
    {
        RequestAuthority.requestCallbackGiveAuthorityToServer = callback;
        RequestAuthority.requestCallbackGiveAuthorityToClient = callback;
        RequestAuthorityForChildren.requestCallbackGiveAuthorityToServer = callback;
        RequestAuthorityForChildren.requestCallbackGiveAuthorityToClient = callback;
        ovidVR.Utilities.prefabSpawnNotifier.PrefabSpawnNotifier.NotifierCallback = notifierCallback;
        GenericPrefabConstructor.AcceptCallback = prefabFinillizeCallback;
        ToolColliderPrefabConstructor.actionCallForToolPrefab = prefabFinillizeCallback;
        QuestionPrefabConstructor.actionCallForQuestionPrefab = prefabFinillizeCallback;

        NetworkToolSync.activateTool = prefabSelectToolCallback;
        NetworkToolSync.deactivateTool = prefabDeselectToolCallback;
        CombinedAction.PerformSubActionCallback = PerformCombinedActionCallback;
        NetworkFunctions.DestroyObject = DestroyCallback;
        NetworkFunctions.FindLocalObject = FindLocalObject;
        NetworkFunctions.SpawnPrefabWithServerAuthority = SpawnPrefabWithServerAuthorityCallback;
        NetworkFunctions.SpawnPrefabChildWithServerAuthority = SpawnPrefabChildWithServerAuthorityCallback;
        NetworkFunctions.GetPoolGameObject = GetGameObjectFromPoolCallBack;
        
       avatarDefaultColorList = new List<Color>();
        avatarDefaultColorList.Add(new Color(0, 0f, 0.5f));//Navy 1
        avatarDefaultColorList.Add(new Color(0.5f, 0.5f, 0));//Olive 2
        avatarDefaultColorList.Add(new Color(0.5f, 0.5f, 0.5f));//Gray 3
        avatarDefaultColorList.Add(new Color(0.5f, 0, 0));//Maroon 4
        avatarDefaultColorList.Add(new Color(0, 0.5f, 0.5f));//Teal 5
        avatarDefaultColorList.Add(new Color(0.5f, 0, 0.5f));//Purple 6
        avatarDefaultColorList.Add(new Color(0.3f, 0.2f, 0.6f));//Navy 2 7
        avatarDefaultColorList.Add(new Color(0.1f, 0.1f, 1));//Blue 8 
        avatarDefaultColorList.Add(new Color(0, 0.9f, 0));//Lime 9 
        avatarDefaultColorList.Add(new Color(0, 0.5f, 0));//Green 10
        avatarDefaultColorList.Add(new Color(0.498f, 0.325f, 0.262f));// 11
        avatarDefaultColorList.Add(new Color(0.294f, 0.039f, 0.25f));// 12 
        avatarDefaultColorList.Add(new Color(0.6f, 0.5f, 0.3f));// 13
        avatarDefaultColorList.Add(new Color(0.8f, 0.4f, 0.1f));// 14
        avatarDefaultColorList.Add(new Color(0.8f, 0.4f, 0.7f));// 15
        avatarDefaultColorList.Add(new Color(0.6f, 0.8f, 0.7f));// 16
        avatarDefaultColorList.Add(new Color(0.95f, 0.9f, 0.3f));// 17
        avatarDefaultColorList.Add(new Color(0.7f, 0.7f, 0.7f));// 18
        avatarDefaultColorList.Add(new Color(0.8f, 0.1f, 0.2f));// 19
        avatarDefaultColorList.Add(new Color(0.8f, 0.5f, 0.5f));// Light Coral 20
        avatarDefaultColorList.Add(new Color(0.95f, 0.7f, 0.5f));// Light Salmon 21
        avatarDefaultColorList.Add(new Color(0.99f, 0.8f, 0f));// Gold 21
        avatarDefaultColorList.Add(new Color(0.99f, 0.85f, 0.75f));// PapayaWhip 22
        avatarDefaultColorList.Add(new Color(0.9f, 0.9f, 0.98f));// Lavender 23
        avatarDefaultColorList.Add(new Color(0.5f, 0.5f, 0f));// Olive 24
        avatarDefaultColorList.Add(new Color(0.5f, 0.99f, 0.83f));// Aquamarine 25

        

        //OvidVRControllerClass.NetworkManager = this;
        Instance = this;
        
        
    }

    private void DestroyCallback(GameObject go)
    {
        PhotonNetwork.Destroy(go);
    }
    private void callback(GameObject toSend)
    {
        PhotonView photonView = toSend.GetComponent<PhotonView>();
        if (photonView != null && !photonView.IsMine)
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
        else if (photonView == null)
            Debug.LogError("Error change authority, object does not contain  PhotonView :" + toSend.name);
    }

    private GameObject SpawnPrefabWithServerAuthorityCallback(string prefab)
    {
        return PhotonNetwork.Instantiate(prefab, 0);
    }
    
    private GameObject SpawnPrefabChildWithServerAuthorityCallback(string prefab, GameObject parent)
    {
        return PhotonNetwork.Instantiate(prefab, parent, 0);
    }

    private void notifierCallback(INetworkID toSend)
    {
        NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.StopJigNotifier, toSend.NetID);
        UNETChat.u.SendMessage(m);
    }

    private void prefabFinillizeCallback(INetworkID _id)
    {
        NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.finilizePrefab, _id.NetID);
        UNETChat.u.SendMessage(m);
    }

    private void prefabFinillizeCallback(INetworkID _id, string netIdGameobject)
    {
        NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.finilizePrefab, _id.NetID, netIdGameobject);
        UNETChat.u.SendMessage(m);
    }

    private void prefabSelectToolCallback(string tool)
    {
        NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.SelectTool, tool);
        UNETChat.u.SendMessage(m);
    }

    private void prefabDeselectToolCallback(string tool)
    {
        NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.DeselectTool, tool);
        UNETChat.u.SendMessage(m);
    }

    private void PerformCombinedActionCallback()
    {
        NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.PerformCombinedAction, Operation.Get.GetLessonID(),
                                                                                               Operation.Get.GetStageID(),
                                                                                               Operation.Get.GetActionID());
        UNETChat.u.SendMessage(m);
    }

    private GameObject FindLocalObject(ushort id)
    {
        return PhotonView.Find((int)id)?.gameObject;
    }
    
    private GameObject GetGameObjectFromPoolCallBack(string prefab_name)
    {

        var views = PhotonNetwork.PhotonViewCollection;
        foreach (var view in views)
        {
            if (view.name.Equals(prefab_name))
                return view.gameObject;
        }       
        
        Debug.LogError("Could not find newtwork gameObject : " + prefab_name);     
        return null;
    }
    public void EstablishConnection()
    {
        if (string.IsNullOrEmpty(Region) || Region == "" || Region.ToLower().Equals("auto"))
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = Region.ToLower();
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        playerNumber++;
        if (!spawnServerObjetsOnce)
        {
            clearCurrentToolset();
        }
        GameObject startCanvas = GameObject.Find("UICreateSession(Clone)");
        
        if (startCanvas == null || startCanvas.name != "UICreateSession(Clone)")
            return;
        
        Transform button = startCanvas.transform.Find("InterfaceContent/StartSession");
        ButtonBehavior buttonScript = button.GetComponent<ButtonBehavior>();

        if (buttonScript )
        {
            Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                buttonScript.ButtonActivation(true);
            }
            
        }
        else
        {
            Debug.Log("Did not find button");
        }

        Transform waitAnim = startCanvas.transform.Find("WaitUsersAnim");

        if (waitAnim)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                waitAnim.gameObject.SetActive(false);
            }
        }
    }

    

    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps )
    {
        base.OnPlayerPropertiesUpdate(target, changedProps);


        if (changedProps.ContainsKey("Profile"))
        {
            MultiplayerAvater[] mpa = FindObjectsOfType<MultiplayerAvater>();
            foreach (MultiplayerAvater mpaa in mpa)
            {
                if (!Equals(mpaa.GetComponent<PhotonView>().Owner, target)) continue;

                mpaa.ChangeProfile(changedProps["Profile"].ToString());
                mpaa.ChangeName(changedProps["Username"].ToString());
                break;
            }
        }

        if (changedProps.ContainsKey("Hands"))
        {
            NetworkHandsAnimationSyncronizer[] nhasObjs = FindObjectsOfType<NetworkHandsAnimationSyncronizer>();
            foreach (NetworkHandsAnimationSyncronizer networkHand in nhasObjs)
            {
                if (!Equals(networkHand.GetComponent<PhotonView>().Owner, target)) continue;

                networkHand.ColorChange(changedProps["Hands"].ToString());
            }
        }


    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        IEnumerator coroutine = enumerator();
        StartCoroutine(coroutine);
        OvidVRControllerClass.NetworkManager.OnStartServer();

        Operation.Get.AddActionOnPerform(OnperformSendSyncMessage);
        Operation.Get.AddActionAfterUndo(OnperformSendSyncMessage);
        
        
    }

    
    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            OvidVRControllerClass.NetworkManager.OnStartClient();
            clearCurrentToolset();
        }

        IEnumerator coroutineClient = enumeratorClient();
        StartCoroutine(coroutineClient);
        if (string.IsNullOrEmpty(customAvatarPath))
        {
            PhotonNetwork.Instantiate("MAGESres\\NetworkR\\DummyNetworkAvatar", 0);
        }
        else
        {
            PhotonNetwork.Instantiate(customAvatarPath, 0);
        }
        

        base.OnJoinedLobby();
    }



    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        //Todo: Regain authority from client to server
    }

    IEnumerator enumerator()
    {
        yield return new WaitForSeconds(0.7f);
        SpawnObjects();
    }

    IEnumerator enumeratorClient()
    {
        SpawnObjectsClient( AvatarColorSelector(playerNumber + 1) + 1);
        yield return 0;
    }

    private bool spawnServerObjetsOnce = false;

    

    void SpawnObjects()
    {
        if (Operation.Get.GetOperationDifficulty() == Difficulty.Easy)
        {
            NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.OperationDiff, "Easy");
            UNETChat.u.SendMessage(m);
        }
        if (Operation.Get.GetOperationDifficulty() == Difficulty.Medium)
        {
            NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.OperationDiff, "Medium");
            UNETChat.u.SendMessage(m);
        }
        if (Operation.Get.GetOperationDifficulty() == Difficulty.Hard)
        {
            NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.OperationDiff, "Hard");
            UNETChat.u.SendMessage(m);
        }

        //Syncronize client LSA
        NetMessageClass syncMessage = new NetMessageClass(NetMessageClass.keycode.SyncOperationState, Operation.Get.GetLessonID(),
                                                                                                      Operation.Get.GetStageID(),
                                                                                                      Operation.Get.GetActionID());
        UNETChat.u.SendMessage(syncMessage);
        
        PrefabImporter.SpawnGenericPrefab("MAGESres/NetworkR/NetworkingInfoWindow");

    }

    void SpawnObjectsClient(int _color)
    {
        
        GameObject v1 = null;
        GameObject v2 = null;

       
        v1 = PhotonNetwork.Instantiate("MAGESres\\NetworkR\\OvidVRLeftHand_Client", 0);
        v2 = PhotonNetwork.Instantiate("MAGESres\\NetworkR\\OvidVRRightHand_Client", 0);

       

       
    }

    

    private void OnperformSendSyncMessage()
    {
        NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.SyncOperationState, Operation.Get.GetLessonID(),
                                                                                            Operation.Get.GetStageID(),
                                                                                            Operation.Get.GetActionID()
                                                                                            );
        UNETChat.u.SendMessage(m);
    }

    private void OnUndoSendSyncMessage()
    {
        NetMessageClass m = new NetMessageClass(NetMessageClass.keycode.Undo, Operation.Get.GetLessonID(),
                                                                                            Operation.Get.GetStageID(),
                                                                                            Operation.Get.GetActionID()
                                                                                            );
        UNETChat.u.SendMessage(m);
    }

    

   



    void clearCurrentToolset()
    {
        ToolsManager.ClearCurrentToolset();
    }

    int AvatarColorSelector(int num)
    {
        if (avatarDefaultColorList != null && avatarDefaultColorList.Count > num)
        {
            Color c = avatarDefaultColorList.ElementAt(num);
            int cr = (int)(c.r >= 1.0f ? 255.0f : c.r * 256.0f),
                cg = (int)(c.g >= 1.0f ? 255.0f : c.g * 256.0f),
                cb = (int)(c.b >= 1.0f ? 255.0f : c.b * 256.0f);
            return cr + 1000 * cg + 1000000 * cb;
        }
        else
        {
            int cr = Random.Range(0, 255), cb = Random.Range(0, 255), cg = Random.Range(0, 255);
            return cr + 1000 * cg + 1000000 * cb;
        }



    }

}

