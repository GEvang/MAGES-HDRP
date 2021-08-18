using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using static NetMessageClass;

[RequireComponent(typeof(PhotonView))]

public class UNETChat : MonoBehaviour
{
    private PhotonView _pView;

    public static UNETChat u;

    //public class OvidMsgType
    //{
    //    public static short OvidMsg = MsgType.Highest + 1;
    //}

    private void Start()
	{
        _pView = PhotonView.Get(this);

        u = this;        
    }

    [PunRPC]
    private void RpcReceiveMessage(int messageCode,
    string toolName,
    bool isActive,
    int lessonId, int stageID, int actionID,
    bool serverDone,
    string netIDGameobject,
    int netID,
    int _clientNo)
    {
        NetMessageClass m = new NetMessageClass();
        m.messageCode = (keycode)messageCode;
        m.toolName = toolName;
        m.isActive = isActive;
        m.lessonId = lessonId;
        m.stageID = stageID;
        m.actionID = actionID;
        m.serverDone = serverDone;
        m.netIDGameobject = netIDGameobject;
        m.netID = netID;
        m._clientNo = _clientNo;

        NetworkToolsActionSet.Instance.MessageExecute(m);
    }
	
    public void SendMessage(NetMessageClass m)
	{
        _pView.RPC("RpcReceiveMessage", RpcTarget.Others, new object[] {
            (int)m.messageCode,
            m.toolName,
            m.isActive ,
            m.lessonId ,
            m.stageID ,
            m.actionID ,
            m.serverDone ,
            m.netIDGameobject,
            m.netID ,
            m._clientNo
        });
    }


}