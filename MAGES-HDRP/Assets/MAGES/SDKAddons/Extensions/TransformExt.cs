using System.Collections.Generic;
using UnityEngine;

public struct TransformData
{
    public Vector3 position;
    public Quaternion rotation;

    public Vector3 localPosition;
    public Vector3 localScale;
    public Vector3 lossyScale;
    public Quaternion localRotation;

    public Transform parent;
}

public static class TransformExt
{

    public static void CopyFrom(this Transform td, Transform transform)
    {
        td.position = transform.position;
        td.localPosition = transform.localPosition;

        td.rotation = transform.rotation;
        td.localRotation = transform.localRotation;

        td.localScale = transform.localScale;
        td.parent = transform.parent;
    }
    
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == aName)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }

}