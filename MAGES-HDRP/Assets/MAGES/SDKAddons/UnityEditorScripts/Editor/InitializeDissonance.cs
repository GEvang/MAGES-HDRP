
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using System.IO;

[InitializeOnLoad]
public static class InitializeDissonance
{
    static InitializeDissonance()
    {

        if(Directory.Exists(Application.dataPath + "\\Dissonance"))
        {
            try
            {
                FileUtil.CopyFileOrDirectory(Application.dataPath + "\\Dissonance", Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\Dissonance");
                FileUtil.DeleteFileOrDirectory(Application.dataPath + "\\Dissonance");
                AssetDatabase.Refresh();
            }
            catch (IOException)
            {
                return;
            }
        }
#if !DISSONANCE
        if (Directory.Exists(Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\Dissonance"))
        {
            AddDefinition("DISSONANCE"); 
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
}
