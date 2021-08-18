#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateActionScriptFromTemplate : MonoBehaviour
{

    public const string PATH_TO_TEMPLATES = "Assets/MAGES/SDKAddons/UnityEditorScripts/ActionScriptsTempltes/";
    public const string PATH_TO_ACTION_SCRIPTS = "Assets/MAGES/Operation/ActionScripts/";
    public const string INSERT_ACTION_TEMPLATE = "InsertActionTemplate.oramavr_template";
    public const string USE_ACTION_TEMPLATE = "UseActionTemplate.oramavr_template";

    public static void CreateInsertActionScript(string actionName,string path,string interactable,string final,string hologram)
    {
        string fileContent = System.IO.File.ReadAllText(PATH_TO_TEMPLATES + INSERT_ACTION_TEMPLATE);
        fileContent = fileContent.Replace("#SCRIPTNAME#", actionName);
        fileContent = fileContent.Replace("#INTERACTABLE_PREFAB#", interactable);
        fileContent = fileContent.Replace("#FINAL_PREFAB#", final);
        fileContent = fileContent.Replace("#HOLO_PREFAB#", hologram);

        if (path.EndsWith("/") == false)
            path += "/";

        Directory.CreateDirectory(Path.GetDirectoryName(PATH_TO_ACTION_SCRIPTS + path));

        System.IO.File.WriteAllText(PATH_TO_ACTION_SCRIPTS + path + actionName + ".cs", fileContent);
        EditorUtility.OpenWithDefaultApp(PATH_TO_ACTION_SCRIPTS + path + actionName + ".cs");

        //AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(PATH_TO_ACTION_SCRIPTS + path + actionName + ".cs"));
        AssetDatabase.Refresh();
    }


    public static void CreateUseActionScript(string actionName, string path, string use_prefab, string collider_prefab, string hologram)
    {
        string fileContent = System.IO.File.ReadAllText(PATH_TO_TEMPLATES + USE_ACTION_TEMPLATE);
        fileContent = fileContent.Replace("#SCRIPTNAME#", actionName);
        fileContent = fileContent.Replace("#USE_PREFAB#", use_prefab);
        fileContent = fileContent.Replace("#COLLIDER_PREFAB#", collider_prefab);
        fileContent = fileContent.Replace("#HOLO_PREFAB#", hologram);

        if (path.EndsWith("/") == false)
            path += "/";

        Directory.CreateDirectory(Path.GetDirectoryName(PATH_TO_ACTION_SCRIPTS + path));

        System.IO.File.WriteAllText(PATH_TO_ACTION_SCRIPTS + path + actionName + ".cs", fileContent);
        EditorUtility.OpenWithDefaultApp(PATH_TO_ACTION_SCRIPTS + path + actionName + ".cs");

        //AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(PATH_TO_ACTION_SCRIPTS + path + actionName + ".cs"));
        AssetDatabase.Refresh();
    }

}

#endif