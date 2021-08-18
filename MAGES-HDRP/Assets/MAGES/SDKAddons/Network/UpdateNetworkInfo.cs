using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateNetworkInfo : MonoBehaviour
{
    public int currentUsers;
    private Text hostDynamic, usersDynamic;

    private string roomName;
    // Use this for initialization
    void Start () {
     
        StartCoroutine(UpdateNetInfo());

        GameObject networkText = GameObject.Find("NetworkingInfoWindow(Clone)");

        hostDynamic = networkText.transform.Find("InfoCanvas/HostNameTextDynamic").GetComponent<Text>();
        usersDynamic = networkText.transform.Find("InfoCanvas/LoggedUsersTextDynamic").GetComponent<Text>();

        hostDynamic.text = UIFunctionsCoop.ConnectedRoomName;
        usersDynamic.text = "1";
    }

    private IEnumerator UpdateNetInfo()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);        
            currentUsers = PhotonNetwork.CurrentRoom.PlayerCount;
            if (string.IsNullOrEmpty(roomName))
            {
                roomName = GetParsedName(PhotonNetwork.CurrentRoom.Name);
            }
            GameObject networkText = GameObject.Find("NetworkingInfoWindow(Clone)");

            hostDynamic = networkText.transform.Find("InfoCanvas/HostNameTextDynamic").GetComponent<Text>();
            usersDynamic = networkText.transform.Find("InfoCanvas/LoggedUsersTextDynamic").GetComponent<Text>();

            hostDynamic.text = roomName;
            usersDynamic.text = currentUsers.ToString();

        }
    }

    private string GetParsedName(string UnParsedName)
    {
        string result = "";
        string[] splitted = UnParsedName.Split('_');
        result = splitted[0];
        for (int i=1;i<splitted.Length-1; i++)
        {
            result +='_'+ splitted[i];
        }

        return result;
    }
          



}
