using ovidVR.sceneGraphSpace;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using ovidVR.GameController;
using ovidVR.toolManager.tool;
using ovidVR.toolManager;
using ovidVR.Utilities.prefabSpawnManager.prefabSpawnConstructor;
using System;
using ovidVR.OperationAnalytics;
using ovidVR.ActionPrototypes;
using ovidVR.Networking;

public class NetworkToolsActionSet : MonoBehaviour
{

    private static NetworkToolsActionSet _instance;

    private static object _lock = new object();

    public static NetworkToolsActionSet Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singleton = new GameObject("NetworkToolsActionSetGameobject");
                _instance = singleton.AddComponent<NetworkToolsActionSet>();
            }
            return _instance;
        }
    }

    public void MessageExecute(NetMessageClass m)
    {
        //print(m.messageCode.ToString());
        switch (m.messageCode)
        {
            case NetMessageClass.keycode.SelectTool:
                ovidVR.toolManager.ToolsManager.GetTool(m.netIDGameobject).netSync.ChangeActive(true) ; 
                break;
            case NetMessageClass.keycode.DeselectTool:
                ovidVR.toolManager.ToolsManager.GetTool(m.netIDGameobject).netSync.ChangeActive(false);
                
                ovidVR.toolManager.ToolsManager.GetTool(m.netIDGameobject).gestureHands.EndToolGesture();

                break;
            case NetMessageClass.keycode.Perform:
                ovidVR.sceneGraphSpace.Operation.Get.Perform();
                break;
            case NetMessageClass.keycode.Undo:
                StopAllCoroutines();
                IEnumerator coroutineUndo = UndoLesson(m);
                StartCoroutine(coroutineUndo);
                break;
            case NetMessageClass.keycode.SyncOperationState:
                StopAllCoroutines();
                IEnumerator coroutine = CheckLesson(m);
                StartCoroutine(coroutine);
                break;            
            case NetMessageClass.keycode.ToolHandle:
                try
                {
                    if (m.isActive) ToolsManager.GetTool(m.toolName).toolGameobject.transform.GetChild(1).GetChild(2).gameObject.GetComponent<GestureHands>().ActivateTool(null);
                    else ToolsManager.GetTool(m.toolName).toolGameobject.transform.GetChild(1).GetChild(2).gameObject.GetComponent<GestureHands>().DeactivateTool(null);
                }
                catch {
                    Debug.LogError("Error In Tool Message");

                }
                break;
            case NetMessageClass.keycode.finilizePrefab:
                GameObject g;
                try
                {
                    if(OvidVRControllerClass.NetworkManager.GetIsServer())
                        g = NetworkFunctions.FindLocalObject((ushort)m.netID);
                    else
                        g = NetworkFunctions.FindLocalObject((ushort)m.netID);
                    //
                    GenericPrefabConstructor genConstructor = g.GetComponent<GenericPrefabConstructor>();
                    if (genConstructor.GetType() == typeof(ToolColliderPrefabConstructor))
                    {
                        ((ToolColliderPrefabConstructor)genConstructor).setChildEventByNetwork = m.netIDGameobject;
                    }
                    else if (genConstructor.GetType() == typeof(QuestionPrefabConstructor))
                    {
                        ((QuestionPrefabConstructor)genConstructor).setChildEventByNetwork = m.netIDGameobject;
                    }
                    genConstructor.FinalizeByNetwork();
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.StackTrace);
                }

                break;
            case NetMessageClass.keycode.changeAlternativePathCustom:

                string actionName = m.netIDGameobject;
                List<int> paths = new List<int>();
                foreach(Char a in m.toolName)
                {
                    paths.Add((int)Char.GetNumericValue(a));
                }

                if (ScenegraphTraverse.GetCurrentAction().name.Equals(actionName))
                {
                    changeAltPath(actionName, paths);
                }
                else
                {
                    Action changeCurrPath = () => { 

                        if (ScenegraphTraverse.GetCurrentAction().name.Equals(actionName))
                        {
                            changeAltPath(actionName, paths);
                        }
                    };

                    Operation.Get.AddActionOnPerform(changeCurrPath);
                    

                }

                break;
            case NetMessageClass.keycode.OperationDiff:
                if (!OvidVRControllerClass.NetworkManager.GetIsClient())
                {
                    return;
                }
                string diff = m.netIDGameobject;

                if (diff.Equals("Easy"))
                {
                    Operation.Get.SetHolograms(true);
                    Operation.Get.SetOperationDifficulty(Difficulty.Easy);
                    UserAccountManager.Get.SetDifficulty(UserAccountManager.Difficulty.Easy);

                }
                else if (diff.Equals("Medium"))
                {
                    Operation.Get.SetHolograms(true);
                    Operation.Get.SetOperationDifficulty(Difficulty.Medium);
                    UserAccountManager.Get.SetDifficulty(UserAccountManager.Difficulty.Medium);

                }
                else if (diff.Equals("Hard"))
                {
                    Operation.Get.SetHolograms(false);
                    Operation.Get.SetOperationDifficulty(Difficulty.Hard);
                    UserAccountManager.Get.SetDifficulty(UserAccountManager.Difficulty.Hard);

                }
                else
                {
                    Debug.Log("wrong diff level got by netwrok");
                }
                break;
            case NetMessageClass.keycode.ClientNumber:
                OvidVRControllerClass.NetworkManager.curPlayer = m._clientNo;
                break;
            case NetMessageClass.keycode.ObjectDestroy:
                GameObject _gameObject;
                if (OvidVRControllerClass.NetworkManager.GetIsServer())
                    _gameObject = NetworkFunctions.FindLocalObject((ushort)m.netID);
                else
                    _gameObject = NetworkFunctions.FindLocalObject((ushort)m.netID);
                Destroy(_gameObject);
                break;
            case NetMessageClass.keycode.IKLampEnable:
                GetIKManager(m).GetComponent<IKManager>().SetIsEnabledTrue();
                break;
            case NetMessageClass.keycode.IKLampDisable:
                GetIKManager(m).GetComponent<IKManager>().SetIsEnabledFalse();
                break;
            case NetMessageClass.keycode.IKLampRetarget:
                GetIKManager(m).GetComponent<IKManager>().ResetTargetPosition();
                break;
            case NetMessageClass.keycode.StopJigNotifier:
                GameObject _gameObjectID;
                if (OvidVRControllerClass.NetworkManager.GetIsServer())
                {
                    _gameObjectID = NetworkFunctions.FindLocalObject((ushort)m.netID);

                    INetworkID inID = NetworkFunctions.FindLocalObject((ushort)m.netID)?.GetComponent<INetworkID>();

                    if(inID!=null)
                        ovidVR.Utilities.prefabSpawnNotifier.PrefabSpawnNotifier.NotifierCallback(inID);
                }
                else
                {
                    _gameObjectID = NetworkFunctions.FindLocalObject((ushort)m.netID);
                }
                if (_gameObjectID.GetComponent<ovidVR.Utilities.prefabSpawnNotifier.PrefabSpawnNotifier>())
                    _gameObjectID.GetComponent<ovidVR.Utilities.prefabSpawnNotifier.PrefabSpawnNotifier>().StopNotification();
                break;            
            case NetMessageClass.keycode.PerformCombinedAction:
                try
                {
                    StopAllCoroutines();
                    IEnumerator coroutinePerformCombinedAction = PerformCombinedAction(m);
                    StartCoroutine(coroutinePerformCombinedAction);                        
                }
                catch(Exception e)
                {
                    Debug.LogError(e.StackTrace);
                }               

                break;
        }
    }

    void changeAltPath(string actionName, List<int> paths)
    {
        GameObject currentNode = ScenegraphTraverse.GetCurrentAction();
        //Return if no scenegraph changes made
        if (currentNode.GetComponent<IAction>().AlternativePath == -1)
        {
            foreach(int path in paths)
            {
                AlternativePath.SetAlternativePath(path);
            }
        }
    }


    IEnumerator CheckLesson(NetMessageClass message)
    {
        if ((OvidVRControllerClass.Get.isClient && !OvidVRControllerClass.Get.isServer) == false) yield break;
        yield return new WaitForSeconds(0.7f);
        if (message == null)
        {
            Debug.Log("Error message in CheckLesson");
        }
        int lessonID = message.lessonId, stageID = message.stageID, actionID = message.actionID;
        int currentLesson = Operation.Get.GetLessonID(),
            currentStage = Operation.Get.GetStageID(),
            currentAction = Operation.Get.GetActionID();

        while (lessonID != currentLesson)
        {
            //Debug.Log(l + " L" + opL);
            yield return new WaitForSeconds(0.1f);

            if (lessonID > currentLesson)
                Operation.Get.PerformByServer();
            else if (lessonID < currentLesson)
                Operation.Get.UndoByServer();


            currentLesson = Operation.Get.GetLessonID();
            currentStage = Operation.Get.GetStageID();
            currentAction = Operation.Get.GetActionID();
            //Debug.Log(l + " LL" + opL);

        }
        while (stageID != currentStage)
        {
            yield return new WaitForSeconds(0.1f);
            //Debug.Log(s + " S" + currentStage);

            if (stageID > currentStage)
                Operation.Get.PerformByServer();
            else if (stageID < currentStage)
                Operation.Get.UndoByServer();

            currentLesson = Operation.Get.GetLessonID();
            currentStage = Operation.Get.GetStageID();
            currentAction = Operation.Get.GetActionID();
            //Debug.Log(s + " SS" + currentStage);

        }
        while (actionID != currentAction)
        {
            //Debug.Log(a + " A" + opA);
            yield return new WaitForSeconds(0.1f);

            if (actionID > currentAction)
                Operation.Get.PerformByServer();
            else if (actionID < currentAction)
                Operation.Get.UndoByServer();
            currentLesson = Operation.Get.GetLessonID();
            currentStage = Operation.Get.GetStageID();
            currentAction = Operation.Get.GetActionID();
            //Debug.Log(a + "AA" + opA);
        }

    }

    IEnumerator UndoLesson(NetMessageClass message)
    {
        if ((OvidVRControllerClass.Get.isClient && !OvidVRControllerClass.Get.isServer) == false) yield break;
        yield return new WaitForSeconds(0.7f);
        if (message == null)
        {
            Debug.Log("Error message in CheckLesson");
        }
        int l = message.lessonId, s = message.stageID, a = message.actionID;
        int opL = Operation.Get.GetLessonID(),
            currentStage = Operation.Get.GetStageID(),
            opA = Operation.Get.GetActionID();
        while (l < opL)
        {
            //Debug.Log(l + " L" + opL);
            yield return new WaitForSeconds(0.1f);


            Operation.Get.UndoByServer();

            opL = Operation.Get.GetLessonID();
            currentStage = Operation.Get.GetStageID();
            opA = Operation.Get.GetActionID();
            //Debug.Log(l + " LL" + opL);

        }
        while (s < currentStage)
        {
            yield return new WaitForSeconds(0.1f);
            //Debug.Log(s + " S" + currentStage);

            Operation.Get.UndoByServer();

            opL = Operation.Get.GetLessonID();
            currentStage = Operation.Get.GetStageID();
            opA = Operation.Get.GetActionID();
            //Debug.Log(s + " SS" + currentStage);

        }
        while (a < opA)
        {
            //Debug.Log(a + " A" + opA);
            yield return new WaitForSeconds(0.1f);

            Operation.Get.UndoByServer();

            opL = Operation.Get.GetLessonID();
            currentStage = Operation.Get.GetStageID();
            opA = Operation.Get.GetActionID();
            //Debug.Log(a + "AA" + opA);


        }
    }

    IEnumerator PerformCombinedAction(NetMessageClass message)
    {
        yield return new WaitForSeconds(0.7f);
        int lessonID = message.lessonId, stageID = message.stageID, actionID = message.actionID;
        int currentLesson = Operation.Get.GetLessonID(),
            currentStage = Operation.Get.GetStageID(),
            currentAction = Operation.Get.GetActionID();
        if (lessonID == currentLesson && stageID == currentStage && actionID == currentAction)
        {
            ScenegraphTraverse.GetCurrentAction().GetComponent<CombinedAction>().NextModuleFinish();
        }

    }

    private GameObject GetIKManager(NetMessageClass message)
    {
        GameObject lampGrab, IKManager;

        if (OvidVRControllerClass.NetworkManager.GetIsServer())
            lampGrab = NetworkFunctions.FindLocalObject((ushort)message.netID);
        else
            lampGrab = NetworkFunctions.FindLocalObject((ushort)message.netID);
        IKManager = lampGrab.transform.parent.Find("IKManager").gameObject;

        return IKManager;
    }

}