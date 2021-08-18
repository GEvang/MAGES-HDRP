using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class InitializeFinalIK
{
    static InitializeFinalIK()
    {

        if (Directory.Exists(Application.dataPath + "\\InverseKinematics"))
        {
            try
            {
                FileUtil.CopyFileOrDirectory(Application.dataPath + "\\InverseKinematics", Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\InverseKinematics");
                FileUtil.DeleteFileOrDirectory(Application.dataPath + "\\InverseKinematics");
                AssetDatabase.Refresh();
            }
            catch (IOException)
            {
                return;
            }
        }
#if !FINAL_IK
        if (Directory.Exists(Application.dataPath + "\\MAGES\\SDKAddons\\ThirdParty\\InverseKinematics"))
        {
            AddDefinition("FINAL_IK"); 
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
