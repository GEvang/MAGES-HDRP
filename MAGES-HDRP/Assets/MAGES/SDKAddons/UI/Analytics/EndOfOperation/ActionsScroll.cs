using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsScroll : MonoBehaviour {

    [Header("Step by each button press"), Range(0f,1f)]
    public float offset = 0.3f;

    float startY, endY;

    RectTransform thisRectTr;

    void Start () {
        thisRectTr = GetComponent<RectTransform>();

        // Allow Scroll up till the position is the opposite of the last child on start
        endY = - transform.GetChild(transform.childCount - 1).GetComponent<RectTransform>().anchoredPosition.y;

        // Allow Scroll Down till the original position
        startY = thisRectTr.anchoredPosition.y;
    }
	
    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void ActionsNext()
    {
        StopAllCoroutines();
        StartCoroutine(UpdatePosition(offset));
    }

    public void ActionsPrev()
    {
        StopAllCoroutines();
        StartCoroutine(UpdatePosition(-offset));
    }

    IEnumerator UpdatePosition(float _newY)
    {
        float timer = 0f, duration = 0.4f;

        float currY = thisRectTr.anchoredPosition.y;
        float newY = currY + _newY;

        newY = Mathf.Clamp(newY, startY, endY);

        while (timer < duration)
        {
            thisRectTr.anchoredPosition = new Vector3(0,  Mathf.SmoothStep(currY, newY, timer/duration), 0);

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
