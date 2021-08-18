using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsSessionOffset : MonoBehaviour {

    static AnalyticsSessionOffset _instance = null;

    public static int analyticsOffset = 0;

    public static AnalyticsSessionOffset Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<AnalyticsSessionOffset>();

                if (_instance == null)
                {
                    GameObject container = new GameObject("AnalyticsSessionOffset");
                    _instance = container.AddComponent<AnalyticsSessionOffset>();
                    DontDestroyOnLoad(container);
                }
            }

            return _instance;
        }

    }

    public int GetAndIncrementAnalyricsOffset()
    {
        int offset_val = analyticsOffset;
        analyticsOffset++;
        return offset_val;

    }
}
