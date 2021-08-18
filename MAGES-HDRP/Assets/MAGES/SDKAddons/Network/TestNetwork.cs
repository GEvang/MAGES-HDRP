using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using ovidVR.UIManagement;
using System;

public class TestNetwork : MonoBehaviour
{
    //string testStatus = "Testing network connection capabilities.";
    //static string testMessage = "Test in progress";
    //string shouldEnableNatMessage = "";
    //bool doneTesting = false;
    //bool probingPublicIP = false;
    //int serverPort = 9999;
    //static bool useNat = false;
    //float timer = 0.0f;

    //bool once = false;

    //ConnectionTesterStatus connectionTestResult = ConnectionTesterStatus.Undetermined;

    //private void Start()
    //{
    //    once = false;
    //}

    //void Update()
    //{
    //    if (!doneTesting)
    //        TestConnection();
    //    else if (!once)
    //    {
    //        once = true;
           
    //        switch (connectionTestResult)
    //        {
    //            case ConnectionTesterStatus.Error:                    
    //                //testStatus = "Network Connection Error!" + Environment.NewLine;
    //                //testMessage = "Please try to re-connect again later";
    //                break;
    //            case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
    //                //testStatus = "Restricted Network Connection!" + Environment.NewLine;
    //                //testMessage = "Your connection seems to be blocked from firewall. Please contact your system administrator";
    //                break;
    //            case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
    //                //testStatus = "Limited Network Access!" + Environment.NewLine;
    //                //testMessage = "Connection to other Sessions might not be possible for you, or other users";
    //                break;
    //        }

    //        UIManagement.SpawnNotificationUI(NotificationUITypes.UINotification, LanguageTranslator.NetworkErrorNotification, 5f);

    //        //Destroy(this);
    //    }
    //}

    //void TestConnection()
    //{

    //    // Start/Poll the connection test, report the results in a label and
    //    // react to the results accordingly
    //    connectionTestResult = Network.TestConnection();
    //    switch (connectionTestResult)
    //    {
    //        case ConnectionTesterStatus.Error:
    //            testMessage = "Problem determining NAT capabilities";
    //            doneTesting = true;
    //            WriteLog.WriteLogFile(connectionTestResult.ToString() + " " + testMessage);
    //            break;

    //        case ConnectionTesterStatus.Undetermined:
    //            testMessage = "Undetermined NAT capabilities";
    //            doneTesting = false;
    //            break;

    //        case ConnectionTesterStatus.PublicIPIsConnectable:
    //            testMessage = "Directly connectable public IP address.";
    //            useNat = false;
    //            doneTesting = true;
    //            break;

    //        // This case is a bit special as we now need to check if we can
    //        // circumvent the blocking by using NAT punchthrough
    //        case ConnectionTesterStatus.PublicIPPortBlocked:
    //            testMessage = "Non-connectable public IP address (port " +
    //                serverPort + " blocked), running a server is impossible.";
    //            useNat = false;
    //            WriteLog.WriteLogFile(connectionTestResult.ToString() + " " + testMessage);

    //            // If no NAT punchthrough test has been performed on this public
    //            // IP, force a test
    //            if (!probingPublicIP)
    //            {
    //                connectionTestResult = Network.TestConnectionNAT();
    //                probingPublicIP = true;
    //                testStatus = "Testing if blocked public IP can be circumvented";
    //                timer = Time.time + 10;
    //            }
    //            // NAT punchthrough test was performed but we still get blocked
    //            else if (Time.time > timer)
    //            {
    //                probingPublicIP = false;        // reset
    //                useNat = true;
    //                doneTesting = true;
    //            }
    //            break;

    //        case ConnectionTesterStatus.PublicIPNoServerStarted:
    //            testMessage = "Public IP address but server not initialized, " +
    //                "it must be started to check server accessibility. Restart " +
    //                "connection test when ready.";
    //            WriteLog.WriteLogFile(connectionTestResult.ToString() + " " + testMessage);

    //            break;

    //        case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
    //            testMessage = "Limited NAT punchthrough capabilities. Cannot " +
    //                "connect to all types of NAT servers. Running a server " +
    //                "is ill advised as not everyone can connect.";
    //            useNat = true;
    //            doneTesting = true;
    //            WriteLog.WriteLogFile(connectionTestResult.ToString() + " " + testMessage);

    //            break;

    //        case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
    //            testMessage = "Limited NAT punchthrough capabilities. Cannot " +
    //                "connect to all types of NAT servers. Running a server " +
    //                "is ill advised as not everyone can connect.";
    //            useNat = true;
    //            doneTesting = true;
    //            WriteLog.WriteLogFile(connectionTestResult.ToString() + " " + testMessage);

    //            break;

    //        case ConnectionTesterStatus.NATpunchthroughAddressRestrictedCone:
    //        case ConnectionTesterStatus.NATpunchthroughFullCone:
    //            testMessage = "NAT punchthrough capable. Can connect to all " +
    //                "servers and receive connections from all clients. Enabling " +
    //                "NAT punchthrough functionality.";
    //            useNat = true;
    //            doneTesting = true;
    //            break;

    //        default:
    //            testMessage = "Error in test routine, got " + connectionTestResult;
    //            WriteLog.WriteLogFile("Default case:" + testMessage);
    //            break;
    //    }

    //    if (doneTesting)
    //    {
    //        if (useNat)
    //            shouldEnableNatMessage = "When starting a server the NAT " +
    //                "punchthrough feature should be enabled (useNat parameter)";
    //        else
    //            shouldEnableNatMessage = "NAT punchthrough not needed";
    //        testStatus = "Done testing";
    //    }
    //}
}