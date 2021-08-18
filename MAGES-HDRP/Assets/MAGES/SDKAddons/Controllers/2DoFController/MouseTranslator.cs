using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Map 2D axis system as input from the mouse to 2 components of a 3d Axis rotation system, and provide the appropriate
/// translation vector (in Update)
/// </summary>
public class MouseTranslator
{
    private Vector3 prevTranslation;
    private Vector3 xMapping, yMapping;
    public Vector3 Translation { get; private set; }
    public float smoothness = 3.0f;

    public float speed = 0.01f;
    public MouseTranslator(Vector3? _xMapping = null, Vector3? _yMapping = null)
    {
        xMapping = _xMapping != null
            ? _xMapping.Value
            : Vector3.right;
        yMapping = _yMapping != null
            ? _yMapping.Value
            : Vector3.forward;
    }

    public Vector3 Update()
    {
        Vector2 delta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * speed;

        prevTranslation = Translation;
        Translation += xMapping * delta.x + yMapping * delta.y;
        return Vector3.Lerp(prevTranslation, Translation, 1.0f / smoothness);
    }

    public void ExtractTranslation(Vector3 v)
    {
        prevTranslation = v;
        Translation = v;
    }
}