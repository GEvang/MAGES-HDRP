using ovidVR.ActionPrototypes;

public class SkinIncisionAction : ToolAction
{

    public override void Initialize()
    {
        SetToolActionPrefab("MedicalSampleApp/Lesson0/Stage0/Action1/CutCollider", ovidVR.toolManager.tool.ToolsEnum.Scalpel);
        SetHoloObject("MedicalSampleApp/Lesson0/Stage0/Action1/Hologram/HologramScalpel");

        SetPerformAction(ovidVR.CharacterController.CharacterControllerTKA.instance.PlayCut1Animation, 1);
        SetPerformAction(ovidVR.CharacterController.CharacterControllerTKA.instance.PlayCut2Animation, 2);
        SetPerformAction(ovidVR.CharacterController.CharacterControllerTKA.instance.PlayCut3Animation, 3);
        SetPerformAction(ovidVR.CharacterController.CharacterControllerTKA.instance.PlayCut4Animation, 4);

        base.Initialize();
    }

}
