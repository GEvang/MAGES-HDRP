using System.Collections.Generic;
using UnityEngine;
using ovidVR.CustomStoryBoard.Importer;
using ovidVR.CustomStoryBoard;
using ovidVR.sceneGraphSpace;
using System;
using ovidVR.AnalyticsEngine;

/// <summary>
/// Class to import the alternative Nodes from the Xml or binary file
/// </summary>
public class AlternativePathImporter : MonoBehaviour
{

    #region Singleton Setup
    private static AlternativePathImporter instance;
    private static object _lock = new object();
    public static AlternativePathImporter Get
    {
        get
        {
            lock (_lock)
            {
                if (instance == null)
                {
                    instance = (AlternativePathImporter)FindObjectOfType(typeof(AlternativePathImporter));

                    if (FindObjectsOfType(typeof(AlternativePathImporter)).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopening the scene might fix it.");
                        return instance;
                    }

                    if (instance == null)
                    {
                        GameObject singleton = new GameObject();
                        instance = singleton.AddComponent<AlternativePathImporter>();
                        singleton.name = "(singleton) " + typeof(AlternativePathImporter).ToString();

                        DontDestroyOnLoad(singleton);

                        Debug.Log("[Singleton] An instance of " + typeof(AlternativePathImporter) +
                            " is needed in the scene, so '" + singleton +
                            "' was created with DontDestroyOnLoad.");
                    }
                    //else
                    //{
                    //    Debug.Log("[Singleton] Using instance already created: " +
                    //        instance.gameObject.name);
                    //}
                }

                return instance;
            }
        }
    }
    #endregion

    public void InitializeAlternativePathBucket(string altLessons, string altStages, string altActions)
    {
        if(!string.IsNullOrEmpty(altLessons))
            LoadAlternatives(StoryBoardImporter.ImportMode.AlternativeLessons, 0);

        if (!string.IsNullOrEmpty(altStages))
            LoadAlternatives(StoryBoardImporter.ImportMode.AlternativeStages, 1);

        if (!string.IsNullOrEmpty(altActions))
            LoadAlternatives(StoryBoardImporter.ImportMode.AlternativeActions, 2);

    }

    /// <summary>
    /// _lsaDepth Explanation:
    /// AlternativePathBucket contains 3 children
    /// AlternativeLessons
    /// AlternativeStages
    /// AlternativeActions
    ///
    /// the _lsaDepth is used to know where to save the paths loaded.
    /// For example if the paths loaded are only for stages then they should be saved to the
    /// AlternativeStages which is the 2nd child of the Bucket with the index: 1
    /// so the _lsaDepth equals to 1 to access the Stage Child only and save the paths there
    /// </summary>
    /// <param name="_fileName"></param>
    /// <param name="_lsaDepth"></param>
    private void LoadAlternatives(StoryBoardImporter.ImportMode mode, int _lsaDepth)
    {
        List<Lessons> altLessons = new List<Lessons>();
        StoryBoardImporter imp = new StoryBoardImporter();


        altLessons = imp.ImportSugeryLessonData(mode);


        if (altLessons == null)
        {
            return;
        }

        bool isDemo = false;
        if (sceneGraph.GetLicenseType() == License.LicenseType.Demo)
        {
            isDemo = true;
        }

        bool accessLesson = false;

        foreach (Lessons lesson in altLessons)
        {
            GameObject lessonNode = new GameObject();
            lessonNode.AddComponent<Lesson>().SetLessonName(lesson.lessonName);
            lessonNode.name = lessonNode.GetComponent<Lesson>().GetLessonName();

            // If alternative Lesson is loaded, add to Dictionary
            if (_lsaDepth == 0)
            {
                lessonNode.name = ReplaceNameAndMatchNodesDefaultWithAlternative(lessonNode, _lsaDepth);
                if (string.IsNullOrEmpty(lessonNode.name))
                {
                    Destroy(lessonNode);
                    return;
                }
            }


            foreach (Stages stage in lesson.allStages)
            {
                GameObject stageNode = new GameObject();
                stageNode.AddComponent<Stage>().SetStageName(stage.stageName);
                stageNode.name = stageNode.GetComponent<Stage>().GetStageName();

                // If alternative Stage is loaded, add to Dictionary
                if (_lsaDepth == 1)
                {
                    stageNode.name = ReplaceNameAndMatchNodesDefaultWithAlternative(stageNode, _lsaDepth);
                    if (string.IsNullOrEmpty(stageNode.name))
                    {
                        Destroy(lessonNode);
                        Destroy(stageNode);
                        return;
                    }
                }

                Dictionary<int,List<ScoringFactor>> currentActionAnalytics;
                int multiplier = 1;
                string customScoringFactor = "";
                foreach (Actions action in stage.allActions)
                {
                    GameObject actionNode = new GameObject
                    {
                        name = action.actionName
                    };

                    if (isDemo && action.IsDemoApplicable == "y")
                    {
                        accessLesson = true;
                    }

                    // If alternative Action is loaded, add to Dictionary
                    if (_lsaDepth == 2)
                    {
                        actionNode.name = ReplaceNameAndMatchNodesDefaultWithAlternative(actionNode, _lsaDepth);
                        if (string.IsNullOrEmpty(actionNode.name))
                        {
                            Destroy(lessonNode);
                            Destroy(stageNode);
                            Destroy(actionNode);
                            return;
                        }
                    }


                    var classType = Type.GetType(action.className);

                    IAction classInst = (IAction)actionNode.AddComponent(classType);
                    if (classInst != null)
                    {
                        classInst.ActionNode = actionNode;
                        classInst.ActionName = actionNode.name;
                    }
                    else
                    {
                        Debug.LogError("Error while importing action script: " + action.className + " in AlternativaPathImporter.");
                    }

                    currentActionAnalytics = AnalyticsRuntimeImporter.ImportAnalyticsForAction(ref multiplier,ref customScoringFactor, action.className);
                    
                    // Add Action Properties to actionNode
                    ActionProperties actionProperties = actionNode.AddComponent<ActionProperties>();
                    if (actionProperties != null)
                    {
                        actionProperties.actionType = action.actionType;
                        actionProperties.averageActionTime = action.averageActionTime;
                        actionProperties.isDemoApplicable = action.IsDemoApplicable;
                        //Action Analytics Specific
                        actionProperties.scoringFactors = currentActionAnalytics;
                        actionProperties.multiplier = multiplier;
                        if (customScoringFactor != "")
                        {
                            var scoreType = Type.GetType(customScoringFactor);
                            //actionProperties.scoringFactors.Add((ScoringFactor)scoreType.GetConstructor(Type.EmptyTypes).Invoke(null));
                        }
                    }
                    else
                    {
                        Debug.LogError("Error while importing action script: ActionProperties in AlternativaPathImporter.");
                    }

                    SetChildOfParent(actionNode, stageNode);
                }
                SetChildOfParent(stageNode, lessonNode);
            }
            SetChildOfParent(lessonNode, transform.GetChild(_lsaDepth).gameObject);

            // Set access to Lesson
            if (isDemo)
            {
                lessonNode.GetComponent<Lesson>().accessLesson = accessLesson;
            }
            accessLesson = false;
        }
    }

    /// <summary>
    /// The node that must be replaced alongside it's name contains the information -LSA-
    /// for itself and the node that will replace in its name.
    /// e.g. (action replacement)
    /// DrillKneeWrongDegrees ADD(0|0|2-1|1|2)
    /// 
    /// This means that the action with ID:2 in LessonID:0 and StageID:0 (LSA: 002) can change the
    /// action with the ID:2 in LessonID:1 and StageID:1 (LSA: 112) with this action provided (DrillKneeWrongDegrees).
    ///   
    /// At the Operation there is a dictionary where this function goes to the key NAME of the action
    /// and save a reference of the new node which is already imported into the AlternativePathBucket.
    /// 
    /// We Same the action NAME and not the LSA ID because if we add/remove some nodes then the numeric hierarchy
    /// is totally changed.
    /// 
    /// The KEYS: RPL / ADD / DEL stand for (Replace node, Add node, Delete node)
    /// RPL: Replaces node with another one in the Scenegraph (given the LSA ID)
    /// ADD: Adds a node BELOW another one in the Scenegraph (given the LSA ID)
    /// DEL: Deletes a node in the Scenegraph (given the LSA ID)
    ///
    /// Also before saving to the bucket this function removes the Parenthesis from the Action name and returns the new
    /// string without the extra information.
    /// </summary>
    private string ReplaceNameAndMatchNodesDefaultWithAlternative(GameObject node, int _lsaDepth)
    {
        string[] splitSpaceName = node.name.Split(' ');
        // Last Part of nodename is KEY(L|S|A-L|S|A) : where KEY = RPL || ADD || DEL && L|S|A = 0 -> 99 split by '|'
        string nodeName = splitSpaceName[splitSpaceName.Length - 1];

        string typeOfNode = "";
        int[] defaultNode = new int[3];
        int[] alternativeNode = new int[3];

        // KEY
        typeOfNode = nodeName[0].ToString() + nodeName[1].ToString() + nodeName[2].ToString();
        if (!typeOfNode.Equals("RPL") && !typeOfNode.Equals("ADD") && !typeOfNode.Equals("DEL"))
        {
            Debug.Log("Error while Splitting from node name the KEY before the parenthesis: Key Name Mismatch (is it RPL, ADD or DEL?)");
            return node.name + "XML SYNTAX ERROR";
        }

        // Remove KEY and both parenthesis
        nodeName = nodeName.Remove(0, 4);
        nodeName = nodeName.Remove(nodeName.Length - 1);

        // Now the string contains L|S|A-L|S|A so it is split (for the two numbers) using '-'
        // and then each LSA is split by '|'
        string[] bothLSAs = nodeName.Split('-');
        string[] LSA1 = bothLSAs[0].Split('|');
        string[] LSA2 = bothLSAs[1].Split('|');

        // L S A & L S A
        try
        {
            for (int count = 0; count <= 2; ++count)
            {
                defaultNode[count] = int.Parse(LSA1[count]);
            }

            for (int count = 0; count <= 2; ++count)
            {
                alternativeNode[count] = int.Parse(LSA2[count]);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Exception while Splitting from node the L/S/A/ numbers in the parenthesis: " + ex.Message);
            return node.name + "XML SYNTAX ERROR";
        }

        // Create the original node name (with the spaces) BUT without the KEY(LSA-LSA) at the end
        // instead create the following: -KEY-2ndLSA (where LSA is split with '|')
        // e.g. initial name: Cut Fat RPL(0|0|1-1|2|2) , after this function: Cut Fat -RPL-1|2|2
        node.name = "";
        for (int i = 0; i < splitSpaceName.Length - 1; ++i)
        {
            node.name = node.name + splitSpaceName[i] + " ";
        }


        node.name = node.name + "-" + typeOfNode + "-";
        // using as top value the _lsaaDepth becuase if it's a Lesson we add only the 1st number,
        // a Stage the first two numbers and an Action all three numbers
        for (int i = 0; i <= _lsaDepth; ++i)
        {
            node.name = node.name + alternativeNode[i].ToString();
            if (i < _lsaDepth)
            {
                node.name = node.name + "|";
            }

        }

        GameObject defaultAction = ScenegraphTraverse.GetSpecificAction(defaultNode[0], defaultNode[1], defaultNode[2]);
        GameObject alternativeAction = ScenegraphTraverse.GetSpecificAction(alternativeNode[0], alternativeNode[1], alternativeNode[2]);

        if (defaultAction && alternativeAction)
        {
            AlternativePath.AddAlternativeNodeToDictionary(defaultAction.name, node);
        }
        else
        {
            return null;
        }

        return node.name;
    }


    private void SetChildOfParent(GameObject child, GameObject parent)
    {
        child.transform.parent = parent.transform;
    }
}