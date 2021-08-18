using System.Collections;
using System.Collections.Generic;
using ovidVR.AnalyticsEngine;
using ovidVR.sceneGraphSpace;
using UnityEngine;

public class CustomScoringFactor : CountDownFactor
{
    public override ScoringFactor Initialize(GameObject g)
    {
        CustomScoringFactor myFactor = g.AddComponent<CustomScoringFactor>();
        Debug.Log("CustomFactor Init was called");
        return myFactor;
    }

    public override float Perform(bool skipped = false)
    {
        Debug.Log("CustomFactor Perform was called");
        return 0;
    }

    public override void Undo()
    {
        Debug.Log("CustomFactor Undo was called");
        
    }

    public override object GetReadableData()
    {
        ScoringFactorData scoringFactorData = new ScoringFactorData();
        
        return scoringFactorData;
    }
}
