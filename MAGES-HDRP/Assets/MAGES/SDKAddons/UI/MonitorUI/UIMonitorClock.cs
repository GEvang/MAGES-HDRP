using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMonitorClock : MonoBehaviour {

    private Transform minutesTransform;
    private Transform secondsTransform;

    float perSecondRotation = (float)(360f / 60f);

    float delta = 0;
    float seconds = 0;

    void Start () {
        minutesTransform = transform.Find("Minutes");
        secondsTransform = transform.Find("Seconds");
	}
		
	void Update () {
        delta += Time.deltaTime;

        if (delta >= 1f)
        {
            seconds += delta;
            if (seconds >= 60f)
            {
                seconds = 0;
                minutesTransform.Rotate(new Vector3(0, 0, perSecondRotation));
            }             
            secondsTransform.Rotate(new Vector3(0, 0, perSecondRotation));

            delta = 0;
        }
    }
}
