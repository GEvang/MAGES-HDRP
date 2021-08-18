#if UNITY_EDITOR

using ovidVR.Utilities.prefabSpawnManager.prefabSpawnConstructor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateInsertActionTemplate : EditorWindow
{

    string ActionPath = "LessonX/StageX/ActionX";
    string ActionName = "ActionName";
    GameObject InteractablePrefab, FinalPrefab,HoloPrefab;
    bool generateDefaultColliders = true;

    bool generateHologram = false;

    bool InteractablePrefabGenerated = false;
    bool FinalPrefabGenerated = false;

    string InterablePathForActionScript;
    string FinalPrefabForActionScript;
    string HologramPathForActionScript;

    [MenuItem("MAGES/Action Editor/Create Insert Action")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CreateInsertActionTemplate));
    }

    void OnGUI()
    {
        ActionPath = EditorGUILayout.TextField("Path to store the action", ActionPath);
        ActionName = EditorGUILayout.TextField("Action Script Name", ActionName);
        #region InteractablePrefab
        if (string.IsNullOrEmpty(InterablePathForActionScript))
        { 
            GUILayout.BeginHorizontal();
            InteractablePrefab = EditorGUILayout.ObjectField("Interactable Prefab", InteractablePrefab, typeof(GameObject), true, GUILayout.Width(position.width / 2 - 10)) as GameObject;
            if (InteractablePrefabGenerated == false)
            {
                if (GUILayout.Button("Generate Interactable Prefab", GUILayout.Width(position.width / 2 - 10)))
                {
                    if (InteractablePrefab == null)
                    {
                        EditorUtility.DisplayDialog("Error generating prefab", "Please select an asset", "OK");
                    }
                    else
                    {
                        GameObject newInteractablePrefab = ovidVRExtendEditor.NewOvidVRPrefabInteractable();
                        newInteractablePrefab.GetComponent<InteractablePrefabConstructor>().prefabInteractableType = PrefabInteractableType.Insert;
                        Instantiate(InteractablePrefab, newInteractablePrefab.transform, false);
                        InteractablePrefab = newInteractablePrefab;
                        ovidVRExtendEditor.AddDefaultBoxColliders(InteractablePrefab, true, true);
                        ovidVRExtendEditor.AddDefaultBoxColliders(InteractablePrefab, false, true);
                        Selection.activeObject = InteractablePrefab;
                        InteractablePrefabGenerated = true;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Save Interactable Prefab", GUILayout.Width(position.width / 2 - 10)))
                {
                    if (InteractablePrefab == null)
                    {
                        EditorUtility.DisplayDialog("Error saving prefab", "Please close the window and restart the process", "OK");
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName("Assets/Resources/LessonPrefabs/" + ActionPath + "/"));
                        string savePath = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/LessonPrefabs/" + ActionPath + "/" + "Interactable_" + ActionName + ".prefab");
                        PrefabUtility.SaveAsPrefabAssetAndConnect(InteractablePrefab, savePath, InteractionMode.AutomatedAction);
                        InterablePathForActionScript = ActionPath + "/" + "Interactable_" + ActionName;
                        DestroyImmediate(InteractablePrefab,true);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        #endregion

        #region FinalPrefab
        if(string.IsNullOrEmpty(FinalPrefabForActionScript))
        {
            GUILayout.BeginHorizontal();
            FinalPrefab = EditorGUILayout.ObjectField("Final Prefab", FinalPrefab, typeof(GameObject), true, GUILayout.Width(position.width / 2 - 10)) as GameObject;
            if (FinalPrefabGenerated == false)
            {
                if (GUILayout.Button("Generate Final Prefab", GUILayout.Width(position.width / 2 - 10)))
                {
                    if (FinalPrefab == null)
                    {
                        EditorUtility.DisplayDialog("Error generating prefab", "Please select an asset", "OK");
                    }
                    else
                    {
                        GameObject newFinalPlacementPrefab = ovidVRExtendEditor.NewOvidVRPrefabInteractableFinalPlacement();//TODO
                        Instantiate(FinalPrefab, newFinalPlacementPrefab.transform, false);
                        FinalPrefab = newFinalPlacementPrefab;
                        ovidVRExtendEditor.AddDefaultBoxColliders(FinalPrefab, true, true);
                        FinalPrefab.GetComponent<Rigidbody>().isKinematic = true;

                        Selection.activeObject = FinalPrefab;
                        FinalPrefabGenerated = true;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Save Final Prefab", GUILayout.Width(position.width / 2 - 10)))
                {
                    if (FinalPrefab == null)
                    {
                        EditorUtility.DisplayDialog("Error saving prefab", "Please close the window and restart the process", "OK");
                    }
                    else if (string.IsNullOrEmpty(InterablePathForActionScript))
                    {
                        EditorUtility.DisplayDialog("Error saving prefab", "Please generate interactable prefab before saving the Final prefab", "OK");
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName("Assets/Resources/LessonPrefabs/" + ActionPath + "/"));
                        string savePath = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/LessonPrefabs/" + ActionPath + "/" + "Final_" + ActionName + ".prefab");

                        GameObject interactablePrefabTMP = AssetDatabase.LoadAssetAtPath("Assets/Resources/LessonPrefabs/" + InterablePathForActionScript + ".prefab", typeof(GameObject)) as GameObject; // Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Resources/LessonPrefabs/" + UsePathForActionScript + ".prefab", typeof(GameObject))) as GameObject;

                        FinalPrefab.GetComponent<PrefabLerpPlacement>().interactablePrefabs = new List<GameObject>() { interactablePrefabTMP };

                        PrefabUtility.SaveAsPrefabAssetAndConnect(FinalPrefab, savePath, InteractionMode.AutomatedAction);
                        FinalPrefabForActionScript = ActionPath + "/" + "Final_" + ActionName;
                        DestroyImmediate(FinalPrefab,true);
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
            Help.BrowseURL("https://docs.oramavr.com/en/latest/unity/tutorials/action_prototypes/insert_action.html");
        }


        if (GUILayout.Button("Generate action script", GUILayout.Width(position.width / 2 - 10)))
        {
            if(string.IsNullOrEmpty(InterablePathForActionScript) || string.IsNullOrEmpty(FinalPrefabForActionScript))
            {
                EditorUtility.DisplayDialog("Error generating action", "Please create Interactable and Final prefabs first", "OK");
                return;
            }


            if(generateHologram)
            {
                HoloPrefab = Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Resources/LessonPrefabs/"+ FinalPrefabForActionScript + ".prefab", typeof(GameObject))) as GameObject;
                ovidVRExtendEditor.RemoveAllComponenetsFromGameObject(HoloPrefab);
                ovidVRExtendEditor.SetUpNetwork(HoloPrefab);
                ovidVRExtendEditor.AddDefaultHologramMaterial(HoloPrefab, true);

                Directory.CreateDirectory(Path.GetDirectoryName("Assets/Resources/LessonPrefabs/" + ActionPath + "/"));
                string savePath = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/LessonPrefabs/" + ActionPath + "/" + "Hologram_" + ActionName + ".prefab");
                string savedPrefabName = PrefabUtility.SaveAsPrefabAssetAndConnect(HoloPrefab, savePath, InteractionMode.AutomatedAction).name;
                HologramPathForActionScript = ActionPath + "/" + savedPrefabName;
                DestroyImmediate(HoloPrefab,true);
            }

            CreateActionScriptFromTemplate.CreateInsertActionScript(ActionName, ActionPath, InterablePathForActionScript, FinalPrefabForActionScript, HologramPathForActionScript);
            this.Close();
        }

    }
}

#endif