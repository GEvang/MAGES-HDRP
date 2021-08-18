
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

[InitializeOnLoad]
public static class InitializeWaveSDK
{
    static ListRequest Request;

    static InitializeWaveSDK()
    {
        Request = Client.List();
        EditorApplication.update += Progress;
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

    static void Progress()
    {
        bool installed = false;
        if (Request.IsCompleted)
        {
            foreach (var package in Request.Result)
            {
                if (package.name.Equals("com.htc.upm.wave.native"))
                {
                    AddDefinition("WAVE_SDK");
                    installed = true;
                }
            }

            if (!installed)
                RemoveDefinition("WAVE_SDK");
            EditorApplication.update -= Progress;
        }
    }
}
