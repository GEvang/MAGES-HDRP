using ovidVR.toolManager.tool;
using ovidVR.ActionPrototypes;

public class CutFatAction : ToolAction
{
    public override void Initialize()
    {
        SetToolActionPrefab("MedicalSampleApp/Lesson0/Stage0/Action3/CutFatColliders", ToolsEnum.Scalpel);
        SetHoloObject("MedicalSampleApp/Lesson0/Stage0/Action3/Hologram/HologramScalpelFat");

        SetPerformAction(ovidVR.CharacterController.CharacterControllerTKA.instance.PlayOpenSkinFull);

        base.Initialize();
    }

}
