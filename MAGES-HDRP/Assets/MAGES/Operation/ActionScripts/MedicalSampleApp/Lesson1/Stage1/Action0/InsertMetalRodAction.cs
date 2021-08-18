using ovidVR.ActionPrototypes;

public class InsertMetalRodAction : InsertAction
{
    public override void Initialize()
    {
        SetInsertPrefab("MedicalSampleApp/Lesson1/Stage1/Action0/MetalRodInteractable", "MedicalSampleApp/Lesson1/Stage1/Action0/MetalRodFinal");
        SetHoloObject("MedicalSampleApp/Lesson1/Stage1/Action0/MetalRodHologram");

        base.Initialize();
    }
}
