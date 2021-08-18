using System.Collections;
using System.Collections.Generic;
using ovidVR.AnalyticsEngine;
using ovidVR.sceneGraphSpace;
using ovidVR.UIManagement;
using UnityEngine;

public class ForceScoringFactor : ScoringFactor
{
    public float maximumForce = 12;
    
    UpdateCollisionForce _forceScript;
    float _currentForce;
    GameObject _knossosBackPart;
    ForceScoringFactor _forceSf;
    int _score;
    LanguageTranslator _errorMsg;

    public override ScoringFactor Initialize(GameObject g)
    {
        _forceSf = g.AddComponent<ForceScoringFactor>();
        _knossosBackPart = GameObject.Find("BackPartHitMallet(Clone)");
        _forceScript = _knossosBackPart.AddComponent<UpdateCollisionForce>();
        _forceScript.Init(maximumForce,true);
        return _forceSf;
    }

    public override float Perform(bool skipped = false)
    {
        Destroy(_forceSf);
        Destroy(_forceScript);
        if (skipped) return 0;
        int errors = _forceScript.GetErrorsCounter();
        _score = 100 - (errors * 20);
        
        return Mathf.Clamp(_score,0,100);
    }

    public override void Undo()
    {
        _score = 0;
        Destroy(_forceSf);
        Destroy(_forceScript);
    }

    public override object GetReadableData()
    {
        ScoringFactorData sfData = new ScoringFactorData();
        sfData.score = _score;
        sfData.outOF = (int) maximumForce;
        sfData.type = "Force Scoring Factor";
        sfData.scoreSpecific = (int) _forceScript.GetCollisionForce();
        sfData.errorMessage =  InterfaceManagement.Get.GetUIMessage(_errorMsg);

        return sfData;
    }
    
    
}
