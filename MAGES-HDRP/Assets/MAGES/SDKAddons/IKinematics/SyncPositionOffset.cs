using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ovidVR.GameController;

public class SyncPositionOffset: MonoBehaviour
{
    public bool targetCamera;
    public string targetName;
    private Transform _target;

	void Update ()
    {
        while (_target == null)
        {
            if (targetCamera)
            {
                _target = OvidVRControllerClass.Get.GetCameraHead().transform;
                return;
            }
            else
            {
                _target = GameObject.Find(targetName).transform;
            }
        }
        transform.position = _target.position;
        transform.rotation = _target.rotation;
    }
}
