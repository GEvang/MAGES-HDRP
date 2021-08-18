using ovidVR.ActionPrototypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveHandleAction : RemoveAction
{
    public override void Initialize()
    {
        SetRemovePrefab("MedicalSampleApp/Lesson1/Stage1/Action1/HandleRemove");

        base.Initialize();
    }
}
