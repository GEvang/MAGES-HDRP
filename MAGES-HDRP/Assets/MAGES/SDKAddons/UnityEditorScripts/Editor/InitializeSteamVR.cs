
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using System.IO;
#if STEAMVR_SDK && !UNITY_ANDROID
using Valve.VR;
#endif

[InitializeOnLoad]
public static class InitializeSteamVR
{
    static InitializeSteamVR()
    {
        if(Directory.Exists(Application.dataPath + "\\SteamVR"))
        {
            try
            {
                FileUtil.CopyFileOrDirectory(Application.dataPath + "\\SteamVR", Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\SteamVR");
                FileUtil.DeleteFileOrDirectory(Application.dataPath + "\\SteamVR");
                AssetDatabase.Refresh();
            }
            catch (IOException)
            {
                return;
            }
        }
        if (Directory.Exists(Application.dataPath + "\\SteamVR_Resources"))
        {
            try
            {
                FileUtil.CopyFileOrDirectory(Application.dataPath + "\\SteamVR_Resources", Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\SteamVR_Resources");
                FileUtil.DeleteFileOrDirectory(Application.dataPath + "\\SteamVR_Resources");
                AssetDatabase.Refresh();
            }
            catch (IOException)
            {
                return;
            }
        }
#if !STEAMVR_SDK
        if (Directory.Exists(Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\SteamVR"))
        {
            AddDefinition("STEAMVR_SDK");
        }
#else
        if (!Directory.Exists(Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\SteamVR"))
        {
            RemoveDefinition("STEAMVR_SDK");
        }
#endif
#if STEAMVR_SDK && !UNITY_ANDROID
        SteamVR_Settings.instance.autoEnableVR = false;
#endif
    }

    public static void AddDefinition(string defineSymbol)
    {
        foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
        {
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

            if (group == BuildTargetGroup.Unknown)
            {
                continue;
            }

            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();

            if (!defineSymbols.Contains(defineSymbol))
            {
                defineSymbols.Add(defineSymbol);
               
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defineSymbols.ToArray()));
             
            }
        }
    }

    public static void RemoveDefinition(string defineSymbol)
    {
        foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
        {
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

            if (group == BuildTargetGroup.Unknown)
            {
                continue;
            }

            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();

            if (defineSymbols.Contains(defineSymbol))
            {
                defineSymbols.Remove(defineSymbol);

                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defineSymbols.ToArray()));

            }
        }
    }
}
