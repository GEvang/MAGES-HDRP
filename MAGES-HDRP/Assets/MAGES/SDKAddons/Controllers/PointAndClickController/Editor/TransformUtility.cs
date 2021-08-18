using UnityEngine;
using UnityEditor;
using System.Collections;

public class TransformUtility : ScriptableObject
{
    private static Vector3 position;
    private static Quaternion rotation;
    private static Vector3 scale;

    [MenuItem("CONTEXT/Transform/Copy Global Position + Rotation", false, 152)]
    static void DoRecordGlobal()
    {
        position = Selection.activeTransform.position;
        rotation = Selection.activeTransform.rotation;
    }

    // PASTE POSITION:
    [MenuItem("CONTEXT/Transform/Paste Global Position + Rotation", false, 153)]
    static void DoApplyGlobalPositionRotation()
    {
        Transform[] selections = Selection.transforms;
        foreach (Transform selection in selections)
        {
            Undo.RecordObject(selection, "Paste Position + Rotation" + selection.name);
            selection.position = position;
            selection.rotation = rotation;
        }
    }
}
