using ovidVR.Networking;
using ovidVR.toolManager.tool;

public class NetMessageClass
{
    public enum keycode { SelectTool, DeselectTool,
        Perform, Undo, SyncOperationState,
        RequestAuthority,
        ToolHandle,finilizePrefab,changeAlternativePathCustom,
        OperationDiff, ClientNumber, ClientMode,
        ObjectDestroy ,
        IKLampEnable, IKLampDisable, IKLampRetarget, 
        stageNodeInteraction, StopJigNotifier, PerformCombinedAction
    };


    public keycode messageCode;
    public string toolName;
    public bool isActive;
    public int lessonId, stageID, actionID;
    public bool serverDone;
    //public NetworkConnection connetion;
    public string netIDGameobject;
    public int netID;
    public int _clientNo = 0;
    //public GestureHands tool;

    public NetMessageClass() {

    }
    public NetMessageClass(keycode k)
    {
        messageCode = k;
    }



    public NetMessageClass(keycode k, int _lessonId, int _stageID, int _actionID)
    {
        messageCode = k;
        lessonId = _lessonId;
        stageID = _stageID;
        actionID = _actionID;
    }

    //public NetMessageClass(keycode k, )

    //public NetMessageClass(keycode k, string _toolName,bool _toolIsRight)
    //{
    //    if (k != keycode.ToolHandle)
    //    {
    //        messageCode = k;
    //        toolName = _toolName;
    //        toolIsRight = _toolIsRight;
    //    }
    //    else
    //    {
    //        messageCode = k;
    //        toolName = _toolName;
    //        isActive = _toolIsRight;
    //    }
        
    //}

    public NetMessageClass(keycode k , bool _serverDone)
    {
        messageCode = k;
        serverDone = _serverDone;
    }

    public NetMessageClass (keycode k,string _netIDGameobject)
    {
        messageCode = k;
        netIDGameobject = _netIDGameobject;
    }

    public NetMessageClass(keycode k, int _netId)
    {
        messageCode = k;
        netID = _netId;
        _clientNo = _netId;
    }

    public NetMessageClass(keycode k, int _netId, string _netGameobject)
    {
        messageCode = k;
        netID = _netId;
        netIDGameobject = _netGameobject;
    }

    public NetMessageClass(keycode k, string altpathActionName,string paths)
    {
        messageCode = k;
        netIDGameobject = altpathActionName;
        toolName = paths;
    }
}

