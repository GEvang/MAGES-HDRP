using UnityEditor;
using UnityEngine;
using OvidVRPhysX;
using System.Collections;

#if UNITY_EDITOR
[CustomEditor(typeof(HandPoser))]
public class HandPoserEditor : Editor {

	private HandPoser poser;

    private SerializedProperty showLeftPreviewProperty;
    private SerializedProperty showRightPreviewProperty;

    private SerializedProperty LefthandPoseListProperty;
    private SerializedProperty RighthandPoseListProperty;

    private SerializedProperty previewPoseSelection;

    private GameObject leftPreview;
    private GameObject rightPreview;

    private bool leftPreviewActive = false;
    private bool rightPreviewActive = false;


    Texture handTexL;
    Texture handTexR;

    int activePoseIndex = 0;

    // Use this for initialization
    void OnEnable()
    {
        LefthandPoseListProperty = serializedObject.FindProperty("LefthandPoseList");
        RighthandPoseListProperty = serializedObject.FindProperty("RighthandPoseList");

        previewPoseSelection = serializedObject.FindProperty("previewPoseSelection");

        showLeftPreviewProperty = serializedObject.FindProperty("showLeftPreview");
        showRightPreviewProperty = serializedObject.FindProperty("showRightPreview");

        poser = (HandPoser)target;

        if(poser.LefthandPoseList.Count == 0)
        {
            poser.LefthandPoseList.Add(null);
            poser.RighthandPoseList.Add(null);
        }
    }

    private void DrawPoseEditorMenu()
    {
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Cannot modify pose while in play mode.");
        }
        else
        {

            activePoseIndex = previewPoseSelection.intValue;

            //box containing all pose editing controls
            GUILayout.BeginVertical("box");

            //show selectable menu of all poses, highlighting the one that is selected
            EditorGUILayout.Space();

            poser.poseNames = new string[LefthandPoseListProperty.arraySize];

            for (int i = 0; i < LefthandPoseListProperty.arraySize; i++)
            {
                if(LefthandPoseListProperty.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    poser.poseNames[i] = "[not set]";
                }
                else
                {
                    poser.poseNames[i] = LefthandPoseListProperty.GetArrayElementAtIndex(i).objectReferenceValue.name; ;
                }
            }

            EditorGUILayout.BeginHorizontal();
            int poseSelected = GUILayout.Toolbar(activePoseIndex, poser.poseNames);


            if (poseSelected != activePoseIndex)
            {
                //---Pose changed---
                UpdatePreviewHand(false, (GameObject)LefthandPoseListProperty.GetArrayElementAtIndex(activePoseIndex).objectReferenceValue, "left");
                UpdatePreviewHand(false, (GameObject)RighthandPoseListProperty.GetArrayElementAtIndex(activePoseIndex).objectReferenceValue, "right");

                showLeftPreviewProperty.boolValue = false;
                showRightPreviewProperty.boolValue = false;
                //-----------------

                activePoseIndex = poseSelected;
                previewPoseSelection.intValue = activePoseIndex;
                serializedObject.ApplyModifiedProperties();
            }



            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(32));
            if (GUILayout.Button("+", GUILayout.MaxWidth(32)))
            {
                LefthandPoseListProperty.InsertArrayElementAtIndex(LefthandPoseListProperty.arraySize);
                RighthandPoseListProperty.InsertArrayElementAtIndex(RighthandPoseListProperty.arraySize);

                LefthandPoseListProperty.GetArrayElementAtIndex(LefthandPoseListProperty.arraySize - 1).objectReferenceValue = null;
                RighthandPoseListProperty.GetArrayElementAtIndex(RighthandPoseListProperty.arraySize - 1).objectReferenceValue = null;
            }

            //only allow deletion of additional postures
            EditorGUI.BeginDisabledGroup(LefthandPoseListProperty.arraySize == 0);

            if (GUILayout.Button("-", GUILayout.MaxWidth(32)) && LefthandPoseListProperty.arraySize > 0)
            {
                if((activePoseIndex >= LefthandPoseListProperty.arraySize) || ((activePoseIndex == 0) && (LefthandPoseListProperty.arraySize == 1)))
                {
                    return;
                }
                LefthandPoseListProperty.DeleteArrayElementAtIndex(activePoseIndex);
                RighthandPoseListProperty.DeleteArrayElementAtIndex(activePoseIndex);

                if (activePoseIndex >= LefthandPoseListProperty.arraySize + 1)
                {
                    activePoseIndex = LefthandPoseListProperty.arraySize - 1;
                    previewPoseSelection.intValue = activePoseIndex;
                    return;
                }else if(activePoseIndex == 0)
                {
                    activePoseIndex = 0;
                }
                else
                {
                    activePoseIndex--;
                }

            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");

            // sides of pose editor
            GUILayout.BeginHorizontal();

            if (handTexL == null)
                handTexL = (Texture)EditorGUIUtility.Load("Assets/MAGES/SDKAddons/HandPoserEditor/HandLeftIcon.png");
            if (handTexR == null)
                handTexR = (Texture)EditorGUIUtility.Load("Assets/MAGES/SDKAddons/HandPoserEditor/HandRightIcon.png");

            if(activePoseIndex >= LefthandPoseListProperty.arraySize)
            {
                activePoseIndex = LefthandPoseListProperty.arraySize - 1;
            }

            //----------------Left Hand-------------------
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            GUI.color = new Color(1, 1, 1, showLeftPreviewProperty.boolValue ? 1 : 0.25f);
            if (GUILayout.Button(handTexL, GUI.skin.label, GUILayout.Width(64), GUILayout.Height(64)))
            {
                showLeftPreviewProperty.boolValue = !showLeftPreviewProperty.boolValue;
                UpdatePreviewHand(showLeftPreviewProperty.boolValue, (GameObject)LefthandPoseListProperty.GetArrayElementAtIndex(activePoseIndex).objectReferenceValue, "left");
            }
            GUI.color = Color.white;

            EditorGUIUtility.labelWidth = 48;
            EditorGUILayout.LabelField("Left Hand", EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth = 0;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 120;

            LefthandPoseListProperty.GetArrayElementAtIndex(activePoseIndex).objectReferenceValue = (GameObject)EditorGUILayout.ObjectField(LefthandPoseListProperty.GetArrayElementAtIndex(activePoseIndex).objectReferenceValue, typeof(GameObject), true);
            EditorGUILayout.EndVertical();

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = 0;

            //----------------Right Hand---------------------
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 48;
            EditorGUILayout.LabelField("Right Hand", EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth = 0;
            GUI.color = new Color(1, 1, 1, showRightPreviewProperty.boolValue ? 1 : 0.25f);

            if (GUILayout.Button(handTexR, GUI.skin.label, GUILayout.Width(64), GUILayout.Height(64)))
            {

                showRightPreviewProperty.boolValue = !showRightPreviewProperty.boolValue;
                UpdatePreviewHand(showRightPreviewProperty.boolValue, (GameObject)RighthandPoseListProperty.GetArrayElementAtIndex(activePoseIndex).objectReferenceValue, "right");

            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 120;
            RighthandPoseListProperty.GetArrayElementAtIndex(activePoseIndex).objectReferenceValue = (GameObject)EditorGUILayout.ObjectField(RighthandPoseListProperty.GetArrayElementAtIndex(activePoseIndex).objectReferenceValue, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();



            GUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 120;
            EditorGUIUtility.labelWidth = 0;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

        }

    }



    private void UpdatePreviewHand(bool mode, GameObject handReference, string leftorRight)
    {
        if (handReference == null)
        {
            return;
        }

        if (mode)
        {
            if(leftorRight == "left" && !leftPreviewActive){
                leftPreview = Instantiate(handReference, poser.transform);
            }
            else if (leftorRight == "right" && !rightPreviewActive)
            {
                rightPreview = Instantiate(handReference, poser.transform);
            }
        }
        else
        {
            if (leftorRight == "left")
            {
                leftPreviewActive = false;
                DestroyImmediate(leftPreview);
            }
            else
            {
                rightPreviewActive = false;
                DestroyImmediate(rightPreview);
            }

            
        }


    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPoseEditorMenu();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
