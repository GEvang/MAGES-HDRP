using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ovidVR.CustomStoryBoard;
using ovidVR.sceneGraphSpace;

public class UIMonitor_GenerateLessonBars : MonoBehaviour {

    public GameObject LessonPrefab;

    private int numberOfLessons = -1;

    private float spaceBetweenLessons = 1.0f;

    private int currentLesson = 1;
    private int prevLesson = 0;

    // Use this for initialization
    void Start()
    {
        //late start to w8 for scenegraph to intialize
        StartCoroutine(LateStart(0.1f));
    }
    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        getNumberOfLessons();
    }

    // Update is called once per frame
    void Update () {
        getCurrentLesson();

        if(currentLesson > prevLesson)
        {
            //instantiate prefab
            GameObject obj = Instantiate(LessonPrefab, transform, false);

            //change position of Lesson Bar on Canvas
            obj.transform.localPosition = new Vector3(spaceBetweenLessons, 0, 0);
            spaceBetweenLessons -= 1.5f;

            //set the lesson's number to current lesson number
            obj.GetComponent<UIMonitor_LessonsProgessBar>().setNumber(currentLesson);

            prevLesson = currentLesson;
        }
        else if(currentLesson < prevLesson)
        {
            //destroy lesson bar
            Destroy(transform.GetChild(prevLesson-1).gameObject);

            //reset space
            spaceBetweenLessons += 1.5f;

            prevLesson = currentLesson;
        }
    }

    //---------------------------------------------//

    public int getNumberOfLessons()
    {
        return numberOfLessons = Operation.Get.GetOperationNode().transform.childCount;
    }

    //returns the current lesson number
    public int getCurrentLesson()
    {
        return currentLesson = Operation.Get.GetLessonID() + 1;
    }
}
