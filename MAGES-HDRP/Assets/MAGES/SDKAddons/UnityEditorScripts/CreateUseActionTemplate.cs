#if UNITY_EDITOR

using ovidVR.Utilities.prefabSpawnManager.prefabSpawnConstructor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateUseActionTemplate : EditorWindow
{

    string ActionPath = "LessonX/StageX/ActionX";
    string ActionName = "ActionName";
    GameObject UsePrefab, UseColliderPrefab,HoloPrefab;
    bool generateDefaultColliders = true;

    bool generateHologram = false;

    bool UsePrefabGenerated = false;
    bool UseColliderPrefabGenerated = false;

    string UsePathForActionScript;
    string UseColliderPathForActionScript;
    string HologramPathForActionScript;

    [MenuItem("MAGES/Action Editor/Create Use Action")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CreateUseActionTemplate));
    }

    void OnGUI()
    {
        ActionPath = EditorGUILayout.TextField("Path to store the action", ActionPath);
        ActionName = EditorGUILayout.TextField("Action Script Name", ActionName);
        #region UsePrefab
        if (string.IsNullOrEmpty(UsePathForActionScript))
        { 
            GUILayout.BeginHorizontal();
            UsePrefab = EditorGUILayout.ObjectField("Use Prefab", UsePrefab, typeof(GameObject), true, GUILayout.Width(position.width / 2 - 10)) as GameObject;
            if (UsePrefabGenerated == false)
            {
                if (GUILayout.Button("Generate Use Prefab", GUILayout.Width(position.width / 2 - 10)))
                {
                    if (UsePrefab == null)
                    {
                        EditorUtility.DisplayDialog("Error generating prefab", "Please select an asset", "OK");
                    }
                    else
                    {
                        GameObject newInteractablePrefab = ovidVRExtendEditor.NewOvidVRPrefabInteractable();
                        newInteractablePrefab.GetComponent<InteractablePrefabConstructor>().prefabInteractableType = PrefabInteractableType.Generic;
                        Instantiate(UsePrefab, newInteractablePrefab.transform, false);
                        UsePrefab = newInteractablePrefab;
                        ovidVRExtendEditor.AddDefaultBoxColliders(UsePrefab, true, true);
                        ovidVRExtendEditor.AddDefaultBoxColliders(UsePrefab, false, true);
                        Selection.activeObject = UsePrefab;
                        UsePrefabGenerated = true;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Save Use Prefab", GUILayout.Width(position.width / 2 - 10)))
                {
                    if (UsePrefab == null)
                    {
                        EditorUtility.DisplayDialog("Error saving prefab", "Please close the window and restart the process", "OK");
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName("Assets/Resources/LessonPrefabs/" + ActionPath + "/"));
                        string savePath = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/LessonPrefabs/" + ActionPath + "/" + "Use_" + ActionName + ".prefab");
                        PrefabUtility.SaveAsPrefabAssetAndConnect(UsePrefab, savePath, InteractionMode.AutomatedAction);
                        UsePathForActionScript = ActionPath + "/" + "Use_" + ActionName;
                        DestroyImmediate(UsePrefab,true);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        #endregion

        #region UseColliderPrefab
        if (string.IsNullOrEmpty(UseColliderPathForActionScript))
        {
            GUILayout.BeginHorizontal();
            UseColliderPrefab = EditorGUILayout.ObjectField("Use Collider Prefab", UseColliderPrefab, typeof(GameObject), true, GUILayout.Width(position.width / 2 - 10)) as GameObject;
            if (UseColliderPrefabGenerated == false)
            {
                if (GUILayout.Button("Generate Use Collider Prefab", GUILayout.Width(position.width / 2 - 10)))
                {
                    if (UseColliderPrefab == null)
                    {
                        EditorUtility.DisplayDialog("Error generating prefab", "Please select an asset", "OK");
                    }
                    else
                    {
                        GameObject newFinalPlacementPrefab = ovidVRExtendEditor.NewOvidVRUseActionCollider();
                        Instantiate(UseColliderPrefab, newFinalPlacementPrefab.transform, false);
                        UseColliderPrefab = newFinalPlacementPrefab;
                        ovidVRExtendEditor.AddDefaultBoxColliders(UseColliderPrefab, true, true);
                        UseColliderPrefab.GetComponent<Rigidbody>().isKinematic = true;
                        Selection.activeObject = UseColliderPrefab;
                        UseColliderPrefabGenerated = true;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Save Collider Prefab", GUILayout.Width(position.width / 2 - 10)))
                {
                    if (UseColliderPrefab == null)
                    {
                        EditorUtility.DisplayDialog("Error saving prefab", "Please close the window and restart the process", "OK");
                    }
                    else if(string.IsNullOrEmpty(UsePathForActionScript))
                    {
                        EditorUtility.DisplayDialog("Error saving prefab", "Please generate use prefab before saving the Collider prefab", "OK");
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName("Assets/Resources/LessonPrefabs/" + ActionPath + "/"));
                        string savePath = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/LessonPrefabs/" + ActionPath + "/" + "UseCollider_" + ActionName + ".prefab");

                        GameObject usePrefabTMP = AssetDatabase.LoadAssetAtPath("Assets/Resources/LessonPrefabs/" + UsePathForActionScript + ".prefab",typeof(GameObject)) as GameObject; // Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Resources/LessonPrefabs/" + UsePathForActionScript + ".prefab", typeof(GameObject))) as GameObject;

                        UseColliderPrefab.GetComponent<UseColliderPrefabConstructor>().prefabsUsed = new List<GameObject>() { usePrefabTMP };

                        PrefabUtility.SaveAsPrefabAssetAndConnect(UseColliderPrefab, savePath, InteractionMode.AutomatedAction);
                        UseColliderPathForActionScript = ActionPath + "/" + "UseCollider_" + ActionName;

                        DestroyImmediate(UseColliderPrefab, true);
                        DestroyImmediate(usePrefabTMP, true);


                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        #endregion


        generateDefaultColliders = EditorGUILayout.Toggle("Generate default box colliders for all prefabs", generateDefaultColliders);
        generateHologram = EditorGUILayout.Toggle("Include Hologram", generateHologram);


        if (GUILayout.Button("Open documentation page", GUILayout.Width(position.width / 2 - 10)))
        {
            Help.BrowseURL("https://docs.oramavr.com/en/latest/unity/tutorials/action_prototypes/use_action.html");
        }


        if (GUILayout.Button("Generate action script", GUILayout.Width(position.width / 2 - 10)))
        {
            if (string.IsNullOrEmpty(UsePathForActionScript) || string.IsNullOrEmpty(UseColliderPathForActionScript))
            {
                EditorUtility.DisplayDialog("Error generating action", "Please create Use and UseCollider prefabs first", "OK");
                return;
            }


            if(generateHologram)
            {
                HoloPrefab = Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Resources/LessonPrefabs/"+ UseColliderPathForActionScript + ".prefab", typeof(GameObject))) as GameObject;
                ovidVRExtendEditor.RemoveAllComponenetsFromGameObject(HoloPrefab);
                ovidVRExtendEditor.SetUpNetwork(HoloPrefab);
                ovidVRExtendEditor.AddDefaultHologramMaterial(HoloPrefab, true);

                Directory.CreateDirectory(Path.GetDirectoryName("Assets/Resources/LessonPrefabs/" + ActionPath + "/"));
                string savePath = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/LessonPrefabs/" + ActionPath + "/" + "Hologram_" + ActionName + ".prefab");
                string savedPrefabName = PrefabUtility.SaveAsPrefabAssetAndConnect(HoloPrefab, savePath, InteractionMode.AutomatedAction).name;
                HologramPathForActionScript = ActionPath + "/" + savedPrefabName;
                DestroyImmediate(HoloPrefab,true);
            }

            CreateActionScriptFromTemplate.CreateUseActionScript(ActionName, ActionPath, UsePathForActionScript, UseColliderPathForActionScript, HologramPathForActionScript);
            this.Close();
        }

    }
}

#endif