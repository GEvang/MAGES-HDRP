using ovidVR.GameController;
using ovidVR.sceneGraphSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 **/
public class UIEnableRaycast : MonoBehaviour {

    private Transform _currentActiveUI;
    private GameObject _raycastRight, _raycastLeft;

	// Use this for initialization
	void Awake () {
        _currentActiveUI = GameObject.Find("CurrentActiveUserUI").transform;
        _raycastRight = GameObject.Find("RaycastRightHand");
        _raycastLeft = GameObject.Find("RaycastLeftHand");
        if ((_raycastLeft != null) && (_raycastRight != null))
        {
            _raycastRight.SetActive(false);
            _raycastLeft.SetActive(false);
        }
	}

    // Update is called once per frame
    void Update () {
        if ((_raycastLeft != null) && (_raycastRight != null))
        {
            if (_currentActiveUI.childCount != 0)
            {
                //ovidVR.UIManagement.UIManagement.CameraFade(true);
                _raycastRight.SetActive(true);
                _raycastLeft.SetActive(true);
            }
            else
            {
                //ovidVR.UIManagement.UIManagement.CameraFade(false);
                _raycastRight.SetActive(false);
                _raycastLeft.SetActive(false);
            }
        }
    }
}
