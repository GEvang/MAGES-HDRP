/**
 * Typical UI Location: Resources/MAGESres/UI/
 * Typical font: SegoeUI
 * 
 * Searches all Prefabs inside the folder given. Seaches all childer of each prefab
 * and if it has a Text component it applies the Font Given. This algorithm assumes
 * all prefabs are GameObjects in the typecast
 **/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
public class UITextComponents : EditorWindow{

    private Font textFont;

    private string folderPath = "Resources/MAGESres/UI/", fontString = "SegoeUI";

    string[] assetsPaths;

    // possible to future updates in Applying to UI Assets
    private enum UIAssetUpdate { selectedFont , scriptUILanguageSwap } 

    [MenuItem("MAGES/UIs/UI Text Component")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(UITextComponents));
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Folder containing ALL UIs");
        folderPath = EditorGUILayout.TextField("All UIs Folder Path:", folderPath);

        EditorGUILayout.LabelField("-----------------------------------------------");

        EditorGUILayout.LabelField("Places selected Font to all UIs. They must be contained inside from the file path given");
        fontString = EditorGUILayout.TextField("Case Sensitive Font Name:", fontString);

        if (GUILayout.Button("Search Path & Place Font"))
        {
            if (fontString == null)
                return;
            
            assetsPaths = AssetDatabase.GetAllAssetPaths();

            foreach (string assetPath in assetsPaths)
            {
                if (assetPath.Contains(fontString))
                {
                    try
                    {
                        UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath(assetPath);
                        textFont = (Font)o;
                        Debug.Log(" Font Found: " + textFont.name);
                    }
                    catch
                    {
                        Debug.LogError("Typecast Object to font error");
                        return;
                    }
                }
            }

            if (textFont == null)
                return;

            StartUIAssetUpdate(UIAssetUpdate.selectedFont);
        }

        EditorGUILayout.LabelField("-----------------------------------------------");
        EditorGUILayout.LabelField("Search all UIs in file path given. If their child-gameobjects have a Text Component");
        EditorGUILayout.LabelField("it adds to them the UILanguageSwap script. Then the developer can choose from the dropdown");
        EditorGUILayout.LabelField("list of that script the message key (from: DLL LanguageTranslator) for this Text Component");

        if (GUILayout.Button("Search Path & Place UILanguageSwap Script"))
        {
            assetsPaths = AssetDatabase.GetAllAssetPaths();

            StartUIAssetUpdate(UIAssetUpdate.scriptUILanguageSwap);
        }
    }

    private void StartUIAssetUpdate(UIAssetUpdate _updateType)
    {
        if (folderPath == null)
        {
            Debug.LogError("File path is Empty!");
            return;
        }

        List<UnityEngine.Object> allUIPrefabs = new List<UnityEngine.Object>();

        foreach (string assetPath in assetsPaths)
        {
            if (assetPath.Contains(folderPath) && assetPath.Contains(".prefab"))
                allUIPrefabs.Add(AssetDatabase.LoadMainAssetAtPath(assetPath));
        }
        
        try
        {
            foreach (UnityEngine.Object o in allUIPrefabs)
            {
                // prefab cannot replace itself so isntansiate in Unity Scene as a new prefab instance
                GameObject g = Instantiate((GameObject)o);
                g.name = o.name;

                Transform[] allChildren = g.GetComponentsInChildren<Transform>(true);

                foreach (Transform child in allChildren)
                {
                    Text uiText = child.GetComponent<Text>();
                    if (uiText != null)
                    {
                        Debug.Log("Text Component found in: " + child.gameObject.name);

                        if (_updateType == UIAssetUpdate.selectedFont)
                        {
                            uiText.font = textFont;
                        }
                        else if(_updateType == UIAssetUpdate.scriptUILanguageSwap)
                        {
                            if (child.GetComponent<UILanguageSwap>() == null)
                                child.gameObject.AddComponent<UILanguageSwap>();
                        }
                    }
                        
                }

                PrefabUtility.ReplacePrefab(g, o, ReplacePrefabOptions.ConnectToPrefab | ReplacePrefabOptions.ReplaceNameBased);

                // After replacing prefab in Assets, delete instantciated prefab in Unity Scene
                DestroyImmediate(g);
            }
        }
        catch
        {
            Debug.LogError("Exception in casting to GameObjects.");
        }
    }
}
#endif
