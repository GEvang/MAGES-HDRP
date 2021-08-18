
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using System.IO;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class InitializeURP
{
    static ListRequest Request;
    static AddRequest ARequest;
    static bool deactivated;

    static InitializeURP()
    {
        if (!deactivated)
        {
            Request = Client.List();
            EditorApplication.update += Progress;
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
        GraphicsSettings.renderPipelineAsset = (RenderPipelineAsset)AssetDatabase.LoadAssetAtPath("Assets/MAGES/SDKAddons/RenderPipeline/UniversalRenderPipelineAsset.asset", typeof(RenderPipelineAsset));
        if (Request.IsCompleted)
        {
            foreach (var package in Request.Result)
            {
                if (package.name.Equals("com.unity.render-pipelines.universal"))
                {
                    AddDefinition("URP");
                    installed = true;
                }
            }

            if (!installed)
            {
                RemoveDefinition("URP");
                if(EditorUtility.DisplayDialog("Warning", "It seems this project is not using Universal Render Pipeline. Your project will be now upgraded to Universal Render Pipeline.", "OK"))
                {                    
                    ARequest = Client.Add("com.unity.render-pipelines.universal");
                    EditorApplication.update += InstallURPFollowUp;
                }
            }
            deactivated = true;
            EditorApplication.update -= Progress;
        }
    }

    static void InstallURPFollowUp()
    {
        if (ARequest.IsCompleted)
        {
            AddDefinition("URP");
            Lightmapping.Clear();
            EditorSceneManager.OpenScene("Assets/MAGES/Operation/Scenes/SampleApp.unity");
            Lightmapping.BakeAsync();
            EditorApplication.update -= InstallURPFollowUp;
        }
    }
}
