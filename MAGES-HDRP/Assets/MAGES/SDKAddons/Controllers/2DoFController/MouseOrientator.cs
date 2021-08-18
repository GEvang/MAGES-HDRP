using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Map 2D axis system as input from the mouse to 2 components of a 3d Axis rotation system, and provide the appropriate
/// orientation quaternion (in Update)
/// </summary>
[System.Serializable]
public class MouseOrientator
{
    private Quaternion prevOrientation;
    public float mx { get; private set; } = 0.0f;
    public float my { get; private set; } = 0.0f;

    private Vector3 xMapping = Vector3.up;
    private Vector3 yMapping = Vector3.right;

    /// Scale mouse input delta vector
    public float ScaleX { get; set; }
    public float ScaleY { get; set; }

    /// How closely should the apparent position be following the desired position
    public float smoothness = 3.0f;

    /// Clamp mouse vector components, if set to null, wrap around 360 degrees, to avoid precision problems
    internal Vector2? mxClamp = null;
    internal Vector2? myClamp = null;

    internal Quaternion orientation;
    /// <summary>
    /// xMapping's and yMapping's components must all be _exactly_ zero, except for one
    /// </summary>
    public MouseOrientator(Vector3? _xMapping = null, Vector3? _yMapping = null, float scaleX = 1.0f, float scaleY = -1.0f)
    {
        xMapping = _xMapping != null
            ? _xMapping.Value
            : Vector3.up;

        yMapping = _yMapping != null
            ? _yMapping.Value
            : Vector3.right;
        ScaleX = scaleX;
        ScaleY = scaleY;
    }

    public void SetClamp(Vector2? mouseX = null, Vector2? mouseY = null)
    {
        mxClamp = mouseX;
        myClamp = mouseY;
    }

    /// <summary>
    /// Get the raw delta, turn it in into a quaternion to avoid euler angles and interpolate the quaternions.
    /// </summary>
    /// <returns>Smooth orientation for this frame</returns>
    public Quaternion Update()
    {
        Vector2 delta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        mx += delta.x * ScaleX;
        my += delta.y * ScaleY;

        Func<float,Vector2?, float> mouseVectorBehavior = (float v, Vector2? clamp) =>
        {
            if (clamp == null)
            {
                if (v > 360.0f) v = v - 360.0f;
                else if (v < 0.0f) v = 360.0f + v;
                return v;
            }
            else
            {
                v = Mathf.Clamp(v, clamp.Value.x, clamp.Value.y);
                return v;
            }
        };

        mx = mouseVectorBehavior(mx, mxClamp);
        my = mouseVectorBehavior(my, myClamp);

        prevOrientation = orientation;
        orientation = Quaternion.AngleAxis(mx, xMapping) * Quaternion.AngleAxis(my, yMapping);

        
        return Quaternion.Slerp(prevOrientation, orientation, 1.0f / smoothness);
    }

    public Quaternion GetCurrentOrientation()
    {
        return Quaternion.AngleAxis(mx, xMapping) * Quaternion.AngleAxis(my, yMapping);
    }

    public void ExtractOrientation(Quaternion orientation)
    {
        var euler = orientation.eulerAngles;

        
        mx = PickNonZero(Vector3.Scale(euler, xMapping));
        my = PickNonZero(Vector3.Scale(euler, yMapping));
    }

    internal float PickNonZero(Vector3 v)
    {
        var choice = v.x != 0.0f
            ? v.x
            : (v.y != 0.0f
                ? v.y
                : v.z);
        return choice;
    }
}