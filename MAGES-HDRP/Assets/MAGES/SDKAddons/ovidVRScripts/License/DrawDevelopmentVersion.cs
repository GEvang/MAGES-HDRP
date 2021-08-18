using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawDevelopmentVersion : MonoBehaviour {

    public string Text = "ORamaVR CVRSB Technical Build. V0.1";

    public void OnGUI()
    {        

        if(!Application.isEditor)
        {
            return;
        }

        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(0, Screen.height - h * 4 / 100, w, h * 4 / 100);

        style.alignment = TextAnchor.LowerLeft;
        style.fontSize = h * 4 / 100;
        style.normal.textColor = Color.white;

        GUI.Label(rect, Text, style);
    }
}
