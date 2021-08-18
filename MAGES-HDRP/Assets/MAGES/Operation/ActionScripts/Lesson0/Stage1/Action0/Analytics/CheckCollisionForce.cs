using System;
using System.Collections;
using System.Collections.Generic;
using ovidVR.UIManagement;
using UnityEngine;
using UnityEngine.Rendering;

public class UpdateCollisionForce : MonoBehaviour
{
    private float collisionForce;
    private float maxCollisionForce;
    private int numOfErrors;
    private bool restarting;
    private bool showUI;
    
    private void OnCollisionEnter(Collision other)
    {
        if (restarting) return;
        if (other.gameObject.name != "MalletPivot") return;
        
        var currCollisionForce = other.impulse.magnitude / Time.fixedDeltaTime;
        
        if (currCollisionForce > maxCollisionForce * 1000)
        {
            collisionForce = currCollisionForce;
            if (currCollisionForce > collisionForce)
            {
                
            }

            CountError();
            StartCoroutine(Restart());
        }
    }

    public float GetCollisionForce()
    {
        return collisionForce;
    }
    public int GetErrorsCounter()
    {
        return numOfErrors;
    }
    
    public void Init(float _maxForce, bool _showUI)
    {
        maxCollisionForce = _maxForce;
        showUI = _showUI;
    }

    private  IEnumerator Restart()
    {
        yield return new WaitForSeconds(0.5f);
        restarting = false;
    }

    private void CountError()
    {
        numOfErrors++;
        if (showUI)
        {
            InterfaceManagement.Get.SpawnDynamicNotificationUI(NotificationUITypes.UICriticalError,
                LanguageTranslator.ForceMalletError, 2.0f);
        }
    }
    
}
