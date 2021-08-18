using ovidVR.sceneGraphSpace;
using ovidVR.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ovidVR.UIManagement;

public class CurriculumListManager : MonoBehaviour
{
    [SerializeField]
    UIButton nextLessonButton;

    private int activeLesson, lessonCounter, totalNumberOfLessons;

    private Transform lessonElement;
    
    struct LessonCurriculumElement
    {
        public Sprite lessonImage;
        public string lessonTitle;
        public List<string> allObjectiveTitles;
    }

    List<LessonCurriculumElement> allLessonObjectives;

    void OnEnable()
    {
        // demo license: 10
        if (sceneGraph.GetLicenseType() == License.LicenseType.Demo)
        {
            StartOperation();
            Destroy(this.gameObject);
            return;
        }

        activeLesson = 0;
        lessonCounter = 0;
        totalNumberOfLessons = Operation.Get.GetNumberOfLessons();

        allLessonObjectives = new List<LessonCurriculumElement>();

        if (totalNumberOfLessons == 1 && nextLessonButton != null)
            nextLessonButton.SetButtonActivity(false, LanguageTranslator.EndObjectivesButExp);

        lessonElement = transform.Find("LessonCurriculumElement");

        InitializeLessons();
        SetLessonElement(0);
    }

    private void InitializeLessons()
    {
        Lesson[] operationLessons = Operation.Get.gameObject.GetComponentsInChildren<Lesson>();

        foreach (Lesson lesson in operationLessons)
        {
            LessonCurriculumElement lce = new LessonCurriculumElement();

            lce.lessonTitle = (lessonCounter + 1) + "/" + totalNumberOfLessons + ": " + lesson.name;
            lce.lessonImage = Resources.Load("MAGESres/UI/ApplicationSpecific/LessonImages/Lesson" + lessonCounter, typeof(Sprite)) as Sprite;

            lce.allObjectiveTitles = new List<string>();
            
            Stage[] operationStages = lesson.GetComponentsInChildren<Stage>();

            foreach (Stage stage in operationStages)
            {
                if (stage.transform.GetSiblingIndex() > 3)
                    break;

                lce.allObjectiveTitles.Add(stage.name);
            }

            allLessonObjectives.Add(lce);

            lessonCounter++;
        }

    }

    public void NextLesson()
    {
        if (activeLesson == totalNumberOfLessons - 1 || !nextLessonButton.isActive)
        {
            if (nextLessonButton != null)
                nextLessonButton.SetButtonActivity(false, LanguageTranslator.EndObjectivesButExp);

            StartOperation();
        }

        if (allLessonObjectives == null)
            return;
        
        activeLesson++;

        if (activeLesson < totalNumberOfLessons)
            SetLessonElement(activeLesson);

        if (totalNumberOfLessons > 1)
        {
            if (activeLesson == totalNumberOfLessons - 1)
            {
                if (nextLessonButton != null)
                    nextLessonButton.SetButtonActivity(false, LanguageTranslator.EndObjectivesButExp);
            }
        }
    }

    public void StartOperation()
    {
        Operation.Get.Perform();
    }

    private void SetLessonElement(int _index)
    {
        lessonElement.Find("LessonImage").GetComponent<Image>().sprite = allLessonObjectives[_index].lessonImage;
        lessonElement.Find("LessonTitle").GetComponent<Text>().text = allLessonObjectives[_index].lessonTitle;

        Transform objList = lessonElement.Find("ObjectiveList");
        if(objList.childCount > 0)
        {
            foreach (Transform child in objList)
                Destroy(child.gameObject);
        }

        int childIndex = 0;
        // If a string is two lines, the next objective will spawn even lower
        float twoLineString = 0;    

        foreach (string str in allLessonObjectives[_index].allObjectiveTitles)
        {
            GameObject stageNode = PrefabImporter.SpawnGenericPrefab("MAGESres/UI/ApplicationSpecific/CurriculumPrefabs/ObjectiveElement", objList.gameObject);
            stageNode.name = str;
            stageNode.GetComponent<Text>().text = str;
            stageNode.transform.Translate(new Vector3(0, -0.027f * (childIndex + twoLineString), 0));

            if (str.Length > 31)
                twoLineString += 1.2f;

            ++childIndex;
        }

    }
}
