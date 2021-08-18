using ovidVR.sceneGraphSpace;
using ovidVR.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LessonUISelection : MonoBehaviour {

    private int activeLesson, lessonCounter, twoLiners, totalNumberOfLessons;
    GameObject lessonList;
    Transform nextButton, prevButton;

    void Start () {
        activeLesson = 0;
        lessonCounter = 0;
        totalNumberOfLessons = Operation.Get.GetNumberOfLessons();
        nextButton = transform.Find("ButtonList/Button1");
        prevButton = transform.Find("ButtonList/Button2");
        lessonList = transform.Find("LessonList").gameObject;

        if (totalNumberOfLessons == 1)
        {
            nextButton.gameObject.SetActive(false);
            prevButton.gameObject.SetActive(false);
            this.enabled = false;
        }

        StartCoroutine("SpawnLessons");
    }

    private IEnumerator SpawnLessons()
    {
        yield return new WaitForSeconds(0.2f);

        Lesson[] operationLessons = Operation.Get.gameObject.GetComponentsInChildren<Lesson>();



        //If lessonList is filled enable first lesson and return
        if (lessonList.transform.childCount > 0)
        {
            foreach (Transform child in lessonList.transform)
            {
                child.gameObject.SetActive(false);
            }

            lessonList.transform.GetChild(0).gameObject.SetActive(true);

        }
        else
        {
            foreach (Lesson lesson in operationLessons)
            {
                GameObject lessonNode = PrefabImporter.SpawnGenericPrefab("MAGESres/Gamification/NewMonitorUI/Options/LessonElement", lessonList);
                lessonNode.name = lesson.name;

                //Lesson Title + ID Setup
                lessonNode.transform.Find("LessonTitle").GetComponent<Text>().text = "Lesson "+ (lessonCounter + 1) + ": " + lesson.name;

                //Lesson Image Setup
                //Check lesson accessability for Demo
                if (lesson.accessLesson)
                {
                    lessonNode.transform.Find("LessonImage").GetComponent<Image>().material =
                        Resources.Load("MAGESres/Curriculum/CurriculumImages/Lesson" + lessonCounter, typeof(Material)) as Material;
                }
                else
                {
                    lessonNode.transform.Find("LessonImage").GetComponent<Image>().material =
                        Resources.Load("MAGESres/Curriculum/CurriculumImages/B&W/Lesson" + lessonCounter, typeof(Material)) as Material;
                }

                //Only 1st Lesson Active
                if (lessonNode.transform.GetSiblingIndex() != 0)
                {
                    lessonNode.SetActive(false);
                }

                lessonCounter++;
            }
        }

    }

    public void ChangeLesson(bool _next)
    {
        if (!lessonList)
            return;

        int currLesson = activeLesson;

        if (_next && activeLesson < totalNumberOfLessons - 1)
            ++activeLesson;
        else if (!_next && activeLesson > 0)
            --activeLesson;
        else
            return;

        lessonList.transform.GetChild(currLesson).gameObject.SetActive(false);
        lessonList.transform.GetChild(activeLesson).gameObject.SetActive(true);

        // Change color of Select Lesson Button according Lesson's accessibility
        GameObject selectLessonGO = GameObject.Find("UI_Lessons/ButtonList/Button");
        if (selectLessonGO)
        {
            Transform text = selectLessonGO.transform.Find("Text");
            if (text) text.GetComponent<Text>().text =
                    ScenegraphTraverse.GetSpecificLesson(activeLesson).GetComponent<Lesson>().accessLesson ? "Select Previewed Lesson" : "Unavailable Lesson";
        }
    }

    public void JumpToSelectedLesson()
    {
        JumpLessons jump = new JumpLessons(activeLesson);
    }
}
