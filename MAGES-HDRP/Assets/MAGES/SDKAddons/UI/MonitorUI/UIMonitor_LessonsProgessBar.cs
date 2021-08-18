using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMonitor_LessonsProgessBar : MonoBehaviour {

    //indicates this lesson's number
    private int thisLessonNumber = 0;

    //glow sprite image
    public Sprite glow;
    private Sprite simple;

    //get the image
    private Image image;

    // Use this for initialization
    void Start () {
        image = GetComponent<Image>();

        //keep simple
        simple = image.sprite;

        //set glow
        image.sprite = glow;
    }

    // Update is called once per frame
    void Update () {
        //constantly get current lesson 
        int currentLesson = GetComponentInParent<UIMonitor_GenerateLessonBars>().getCurrentLesson();

        if(currentLesson == thisLessonNumber)
        {
            image.sprite = glow;
        }
        else
        {
            image.sprite = simple;
        }
    }

    //-------------------------------------//

    public void setNumber(int number)
    {
        thisLessonNumber = number;
    }

}
