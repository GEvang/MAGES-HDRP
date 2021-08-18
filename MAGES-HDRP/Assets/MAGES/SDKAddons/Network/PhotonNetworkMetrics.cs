using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun.UtilityScripts;
using ovidVR.Networking;

[RequireComponent(typeof(PlayerNumbering))]
public class PhotonNetworkMetrics : MonoBehaviourPunCallbacks
{

    public float broadCastRTTInterval = 2.7f;
    public float outputLogInterval = 3.0f;
    /// <summary>
    /// This should only be set in the editor, it's otherwise ignored at runtime
    /// </summary>
    public bool isTracking = false;
    private string outputTextFile;

    private static PhotonNetworkMetrics instance = null;
    public static PhotonNetworkMetrics Instance {
        get
        {
            return instance;
        }
    }

    // player rtts between each player and photon server
    private Dictionary<int, int> playerRTTs = null;
    private Text metricsText;
    private PhotonPeer peer;

    private long numOutgoingPositions = 0;
    private long numIncomingPositions = 0;
    private long numOutgoingRotations = 0;
    private long numIncomingRotations = 0;

    public void ResetMetrics()
    {
        numOutgoingPositions = 0;
        numIncomingPositions = 0;
        numOutgoingRotations = 0;
        numIncomingRotations = 0;

        // resets photon network traffic stats
        peer.TrafficStatsReset();
    }

    public override void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEvent;
        PlayerNumbering.OnPlayerNumberingChanged += this.OnPlayerNumberingChanged;
    }

    public override void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEvent;
        PlayerNumbering.OnPlayerNumberingChanged -= this.OnPlayerNumberingChanged;
    }

    public void OnPlayerNumberingChanged()
    {
        // clear ping dictionary and use an invalid value for current rtt for each player
        // except the local player
        if (playerRTTs == null) return;
            playerRTTs.Clear();

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == p.ActorNumber)
                playerRTTs.Add(p.ActorNumber, peer.RoundTripTime);
            else
                playerRTTs.Add(p.ActorNumber, -1);
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        instance = this;
    }
    
    void Start()
    {
        if (isTracking)
        {
            outputTextFile = Application.persistentDataPath + "/" + "PhotonNetworkMetrics.csv";

            Debug.Log("Began writing metrics to " + outputTextFile);

            System.IO.File.WriteAllText(outputTextFile, "ms since start of measurements,outgoing message count,incoming message count,outgoing message rate,incoming message rate,byte rate,outgoing position count,incoming position count,outgoing position rate, incoming position rate,outgoing rotation count,incoming rotation count,outgoing rotation rate,incoming rotation rate\n");

            metricsText = GetComponent<Text>();
            peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
            peer.TrafficStatsEnabled = true;

            playerRTTs = new Dictionary<int, int>();

            StartCoroutine(BroadcastRTTCoroutine());
            StartCoroutine(LogNetworkMetrics());

        }

    }

    internal void WriteNetworkMetrics__CSV(System.IO.StreamWriter writer)
    {
        writer.Write(peer.TrafficStatsElapsedMs +",");
        writer.Write(TotalOutgoingMessageCount  + "," + TotalIncomingMessageCount  + ",");
        writer.Write(TotalOutgoingMessageRate   + "," + TotalIncomingMessageRate   + ",");
        writer.Write(TotalByteRate + ",");
        writer.Write(TotalOutgoingPositionCount + "," + TotalIncomingPositionCount + ",");
        writer.Write(TotalOutgoingPositionRate  + "," + TotalIncomingPositionRate  + ",");
        writer.Write(TotalOutgoingRotationCount + "," + TotalIncomingRotationCount + ",");
        writer.Write(TotalOutgoingRotationRate  + "," + TotalIncomingRotationRate);

        Debug.Log("[PhotonNetworkMetrics.cs] Incoming Message Rate: " + TotalIncomingMessageRate);
        Debug.Log("[PhotonNetworkMetrics.cs] Incoming Position Rate: " + TotalIncomingPositionRate);
        Debug.Log("[PhotonNetworkMetrics.cs] Incoming Rotation Rate: " + TotalIncomingRotationRate);
        if (playerRTTs != null)
        {
            writer.Write(",");

            int rttCount = playerRTTs.Count;
            int counter = 0;
            foreach (KeyValuePair<int, int> pair in playerRTTs)
            {
                writer.Write(pair.Value);

                if (counter != (rttCount - 1))
                    writer.Write(",");
                counter++;
            }
        }

        writer.Write("\n");
    }

    IEnumerator LogNetworkMetrics()
    {
        while (true)
        {

            System.IO.StreamWriter writer = new System.IO.StreamWriter(outputTextFile, true);

            WriteNetworkMetrics__CSV(writer);

            writer.Close();

            yield return new WaitForSeconds(outputLogInterval);
        }
    }

    public const byte BroadcastRTTEventCode = 199;

    private void BroadcastRTT()
    {
        object[] content = new object[] { peer.RoundTripTime };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(BroadcastRTTEventCode, content, raiseEventOptions, SendOptions.SendUnreliable);
    }

    /// <summary>
    /// Get the time passed since the start of the current traffic measurements
    /// </summary>
    public long ElapsedMeasureMS
    {
        get
        {
            return peer.TrafficStatsElapsedMs;
        }
    }

    public long TotalMessageCount
    {
        get
        {
            return peer.TrafficStatsGameLevel.TotalMessageCount;
        }
    }

    public long TotalIncomingMessageCount
    {
        get
        {
            return peer.TrafficStatsGameLevel.TotalIncomingMessageCount;
        }
    }

    public long TotalOutgoingMessageCount
    {
        get
        {
            return peer.TrafficStatsGameLevel.TotalOutgoingMessageCount;
        }
    }

    public double TotalMessageRate
    {
        get
        {
            long elapsedMS = peer.TrafficStatsElapsedMs;
            if (elapsedMS == 0) elapsedMS = 1;

            return ((double)peer.TrafficStatsGameLevel.TotalMessageCount) / elapsedMS;
        }
    }

    public double TotalIncomingMessageRate
    {
        get
        {
            long elapsedMS = peer.TrafficStatsElapsedMs;
            if (elapsedMS == 0) elapsedMS = 1;

            return ((double)TotalIncomingMessageCount) / (elapsedMS);
        }
    }

    public double TotalOutgoingMessageRate
    {
        get
        {
            long elapsedMS = peer.TrafficStatsElapsedMs;
            if (elapsedMS == 0) elapsedMS = 1;

            return ((double)TotalOutgoingMessageCount) / (elapsedMS);
        }
    }

    public double TotalByteRate
    {
        get
        {
            long elapsedMS = peer.TrafficStatsElapsedMs;
            if (elapsedMS == 0) elapsedMS = 1;

            return ((double)peer.TrafficStatsGameLevel.TotalByteCount) / elapsedMS;
        }
    }

    public void IncrementTransformation(OvidVrSyncTransformPhoton.TransformFlags tf, bool outgoing)
    {
        if (!isTracking) return;

        if (tf != OvidVrSyncTransformPhoton.TransformFlags.None)
        {

            if ((tf & OvidVrSyncTransformPhoton.TransformFlags.Position) != 0)
            {
                if (outgoing)   numOutgoingPositions++;
                else            numIncomingPositions++;
            }

            if ((tf & OvidVrSyncTransformPhoton.TransformFlags.Rotation) != 0)
            {
                if (outgoing)   numOutgoingRotations++;
                else            numIncomingRotations++;
            }
        }
    }

    // Technically the same flags with the normal sync, but they are declared as different classes
    // TODO: Consider unifying to one function or making a distinction between MultiTransform / SingleTransform counts
    public void IncrementTransformation(NetMultiTransformPhoton.TransformFlags tf, bool outgoing)
    {
        if (!isTracking) return;

        if (tf != NetMultiTransformPhoton.TransformFlags.None)
        {

            if ((tf & NetMultiTransformPhoton.TransformFlags.Position) != 0)
            {
                if (outgoing) numOutgoingPositions++;
                else numIncomingPositions++;
            }

            if ((tf & NetMultiTransformPhoton.TransformFlags.Rotation) != 0)
            {
                if (outgoing) numOutgoingRotations++;
                else numIncomingRotations++;
            }
        }
    }

    public long TotalOutgoingTransformationCount {
        get
        {
            return numOutgoingPositions + numOutgoingRotations;
        }
    }

    public long TotalIncomingTransformationCount
    {
        get
        {
            return numIncomingPositions + numIncomingRotations;
        }
    }

    public long TotalOutgoingPositionCount
    {
        get
        {
            return numOutgoingPositions;
        }
    }


    public long TotalIncomingPositionCount
    {
        get
        {
            return numIncomingPositions;
        }
    }

    public long TotalOutgoingRotationCount
    {
        get
        {
            return numOutgoingRotations;
        }
    }


    public long TotalIncomingRotationCount
    {
        get
        {
            return numIncomingRotations;
        }
    }
    public double TotalOutgoingPositionRate
    {
        get
        {
            long elapsedMS = peer.TrafficStatsElapsedMs;
            if (elapsedMS == 0) elapsedMS = 1;

            return ((double)TotalOutgoingPositionCount) / elapsedMS;
        }
    }

    public double TotalIncomingPositionRate
    {
        get
        {
            long elapsedMS = peer.TrafficStatsElapsedMs;
            if (elapsedMS == 0) elapsedMS = 1;

            return ((double)TotalIncomingPositionCount) / elapsedMS;
        }
    }

    public double TotalOutgoingRotationRate
    {
        get
        {
            long elapsedMS = peer.TrafficStatsElapsedMs;
            if (elapsedMS == 0) elapsedMS = 1;

            return ((double)TotalOutgoingRotationCount) / elapsedMS;
        }
    }

    public double TotalIncomingRotationRate
    {
        get
        {
            long elapsedMS = peer.TrafficStatsElapsedMs;
            if (elapsedMS == 0) elapsedMS = 1;

            return ((double)TotalIncomingRotationCount) / elapsedMS;
        }
    }

    //private void CollectExtraMetrics(ref Text output)
    //{
    //    if (peer.TrafficStatsEnabled)
    //    {
    //        long msSinceStartTracking = peer.TrafficStatsElapsedMs;
    //        output.text += string.Format("Total messages: {0}, send, recv = {1}, {2}\n", TotalMessageCount, TotalOutgoingMessageCount, TotalIncomingMessageCount);
    //        output.text += string.Format("Total logic-level bytes / ms: {0}\n", TotalByteRate);
    //        output.text += string.Format("Total transformations (send, recv) = {0}, {1}\n", TotalOutgoingTransformationCount, TotalIncomingTransformationCount);
    //    }
    //}

    public void OnPhotonEvent(EventData eventData)
    {
        if (eventData.Code == BroadcastRTTEventCode)
        {
            if (playerRTTs == null) return;

            object[] data = (object[])eventData.CustomData;
            int peerRTT = (int)data[0];

            int actorNumber = eventData.Sender;

            int tmpValue;
            if (playerRTTs.TryGetValue(actorNumber, out tmpValue))
            {
                playerRTTs[actorNumber] = peerRTT;
            }
            else
            {
                playerRTTs.Add(actorNumber, peerRTT);
            }

            //metricsText.text = "";
            //foreach (var player in playerRTTs.Keys)
            //{
            //    metricsText.text += player + ": " + playerRTTs[player] + "\n";
            //}

            //CollectExtraMetrics(ref metricsText);
        }
    }

    private IEnumerator BroadcastRTTCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(broadCastRTTInterval);

            if (PhotonNetwork.InLobby || PhotonNetwork.InRoom)
                BroadcastRTT();
        }
    }
}
