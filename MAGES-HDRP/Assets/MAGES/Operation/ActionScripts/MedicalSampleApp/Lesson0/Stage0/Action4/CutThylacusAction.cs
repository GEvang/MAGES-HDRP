using ovidVR.ActionPrototypes;
using ovidVR.toolManager.tool;

public class CutThylacusAction : ToolAction
{
    public override void Initialize()
    {
        SetToolActionPrefab("MedicalSampleApp/Lesson0/Stage0/Action4/CutThylacusColliders", ToolsEnum.Scalpel);
        SetHoloObject("MedicalSampleApp/Lesson0/Stage0/Action4/Hologram/HologramThylacus");

        base.Initialize();
    }

    public override void Perform()
    {
        ovidVR.CharacterController.CharacterControllerTKA.instance.PlayMovePattela();
        ovidVR.CharacterController.CharacterControllerTKA.instance.PlayRiseLeg();

        base.Perform();
    }

    public override void Undo()
    {
        ovidVR.CharacterController.CharacterControllerTKA.instance.ResetOpenSkinFull();

        base.Undo();
    }
}
