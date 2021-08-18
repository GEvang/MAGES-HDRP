using ovidVR.ActionPrototypes;
using ovidVR.UIManagement;

/// <summary>
/// This is an example of Question Action
/// In this Action users are asked a question and they need to answer to complete the Action 
/// The developer can set multiple answers by modifying the question prefab. In this example we have two answers
/// </summary>
public class QuestionActionExample : QuestionAction
{
    /// <summary>
    /// Initialize() method overrides base.Initialize and sets the question prefab 
    /// </summary>
    public override void Initialize()
    {
        //Sets the question prefab that will spawn
        SetQuestionPrefab("Lesson0/Stage0/Action1/QuestionPrefabExample");

        //This method enables the raycast so users can answer using their controllers
        InterfaceManagement.Get.InterfaceRaycastActivation(true);

        base.Initialize();
    }

}
