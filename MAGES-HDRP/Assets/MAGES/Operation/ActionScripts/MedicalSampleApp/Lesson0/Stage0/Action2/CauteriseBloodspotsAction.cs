using ovidVR.ActionPrototypes;
using ovidVR.toolManager.tool;
using ovidVR.UIManagement;
using UnityEngine;
using ovidVR.Utilities;

public class CauteriseBloodSpotsAction : ToolAction
{
    private GameObject notification;
    private GameObject fasciaPhysicalCollider;
    public override void Initialize()
    {
        SetToolActionPrefab("MedicalSampleApp/Lesson0/Stage0/Action2/BloodSpotsColliders", ToolsEnum.Cauterizer);

        fasciaPhysicalCollider = Spawn("MedicalSampleApp/Lesson0/Stage0/Action2/FasciaPhysicalCollider");

        if(!notification)
            notification = InterfaceManagement.Get.SpawnCustomExtraExplanationNotification("LessonPrefabs/MedicalSampleApp/Lesson0/Stage0/Action2/CauteryExplanationUI", null, false);
        
        base.Initialize();
    }

    public override void Perform()
    {
        DestroyUtilities.RemoteDestroy(notification);

        base.Perform();
    }

    public override void Undo()
    {
        ovidVR.CharacterController.CharacterControllerTKA.instance.ResetCutAnimation();
        DestroyUtilities.RemoteDestroy(notification);
        DestroyUtilities.RemoteDestroy(fasciaPhysicalCollider);

        base.Undo();
    }

}

