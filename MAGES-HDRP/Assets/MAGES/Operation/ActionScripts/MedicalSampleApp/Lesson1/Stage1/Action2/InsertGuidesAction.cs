using ovidVR.ActionPrototypes;
using UnityEngine;
using static ovidVR.ActionPrototypes.InsertAction;

public class InsertGuidesAction : CombinedAction
{
    private InsertGroup alingment, cuttingBlock;
    public override void Initialize()
    {
        //InsertAction sub-Action
        InsertAction insertAllignmentGuide = gameObject.AddComponent<InsertAction>();
        alingment = insertAllignmentGuide.SetInsertPrefab("MedicalSampleApp/Lesson1/Stage1/Action2/FemoralGuideInteractable", "MedicalSampleApp/Lesson1/Stage1/Action2/FemoralGuideFinal");
        insertAllignmentGuide.SetHoloObject("MedicalSampleApp/Lesson1/Stage1/Action2/FemoralGuideHologram");
        insertAllignmentGuide.SetPerformAction(()=>alingment.finalPrefab.GetComponent<Animation>().Play("AlignmentAnimation"));
        //--------------------------------------------------------------------------------------------
        //InsertAction sub - Action
        InsertAction insertCuttingBlock = gameObject.AddComponent<InsertAction>();
        cuttingBlock = insertCuttingBlock.SetInsertPrefab("MedicalSampleApp/Lesson1/Stage1/Action2/CuttingBlockInteractable", "MedicalSampleApp/Lesson1/Stage1/Action2/CuttingBlockFinal");
        insertCuttingBlock.SetHoloObject("MedicalSampleApp/Lesson1/Stage1/Action2/CuttingBlockHologram");
        insertCuttingBlock.SetPerformAction(() => cuttingBlock.finalPrefab.GetComponent<Animation>().Play("CuttingBlockAnimation"));

        InsertIActions(insertAllignmentGuide, insertCuttingBlock);

        base.Initialize();
    }
}
