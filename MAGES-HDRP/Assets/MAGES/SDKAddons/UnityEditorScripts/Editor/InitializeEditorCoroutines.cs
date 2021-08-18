using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;


[InitializeOnLoad]
static class InitializeEditorCoroutines
{
    static ListRequest LRequest;
    static InitializeEditorCoroutines()
    {
        LRequest = Client.List();
        EditorApplication.update += Progress;
    }

    static void Progress()
    {
        bool installed = false;
        if (LRequest.IsCompleted)
        {
            foreach (var package in LRequest.Result)
            {
                if (package.name.Equals("com.unity.editorcoroutines"))
                {
                    installed = true;
                }
            }

            if (!installed)
            {
                Client.Add("com.unity.editorcoroutines");
            }
#if !EDITORC
            AddDefinition("EDITORC");
#endif
            EditorApplication.update -= Progress;
        }
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
