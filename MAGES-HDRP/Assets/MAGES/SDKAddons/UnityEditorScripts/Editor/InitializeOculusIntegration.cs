
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using System.IO;

[InitializeOnLoad]
public static class InitializeOculusIntegration
{
    static InitializeOculusIntegration()
    {
        if(Directory.Exists(Application.dataPath + "\\Oculus"))
        {
            try
            {
                FileUtil.CopyFileOrDirectory(Application.dataPath + "\\Oculus", Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\Oculus");
                FileUtil.DeleteFileOrDirectory(Application.dataPath + "\\Oculus");
                AssetDatabase.Refresh();
            }
            catch (IOException)
            {
                return;
            }
        }
#if !OCULUS_SDK
        if (Directory.Exists(Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\Oculus"))
        {
            AddDefinition("OCULUS_SDK");
        }
#else
        if (!Directory.Exists(Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\Oculus"))
        {
            RemoveDefinition("OCULUS_SDK");
        }
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
