using ovidVR.ActionPrototypes;
using UnityEngine;

public class DrillKneeAction : ToolAction
{
    public override void Initialize()
    {
        //Enable drill hole texture in case they are not active (from Undo) 
        GameObject.Find("Bone_drilled_cylinder").GetComponent<BoneDrillSmallPiece>().ToggleRenderer(true);

        SetToolActionPrefab("MedicalSampleApp/Lesson1/Stage0/Action0/DrillCollider", ovidVR.toolManager.tool.ToolsEnum.Drill);

        SetHoloObject("MedicalSampleApp/Lesson1/Stage0/Action0/Hologram/DrillHolo");

        base.Initialize();
    }

    public override void Perform()
    {
        //Disable femur bone pieces
        GameObject.Find("Bone_drilled_cylinder").GetComponent<BoneDrillSmallPiece>().ToggleRenderer(false);
        base.Perform();
    }

    public override void Undo()
    {
        //Reset leg animations
        ovidVR.CharacterController.CharacterControllerTKA.instance.ResetRiseLeg();
        ovidVR.CharacterController.CharacterControllerTKA.instance.ResetMovePattela();

        base.Undo();
    }
}
