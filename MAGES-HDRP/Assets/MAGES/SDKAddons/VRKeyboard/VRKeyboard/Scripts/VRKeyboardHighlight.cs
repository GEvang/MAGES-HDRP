using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRKeyboardHighlight : MonoBehaviour
{

    private Animator _animatorRight, _animatorLeft;
    private bool _isRight, _hovering;

    private AudioSource _HoverSource;
    private AudioClip _Hover;

    private void Start()
    {
        if(GameObject.Find("Username"))
            _HoverSource = GameObject.Find("Username").GetComponent<AudioSource>();

        _Hover = Resources.Load("MAGESres/UI/InterfaceMaterial/Sounds/Hover") as AudioClip;
        _animatorRight = GameObject.Find("OvidVRDeviceController").GetComponent<OvidVRControllerClass>().rightHand.GetComponent<Animator>();
        _animatorLeft = GameObject.Find("OvidVRDeviceController").GetComponent<OvidVRControllerClass>().leftHand.GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Equals("RaycastRightHandLogin") || other.name.Equals("Sphere"))
        {
            _hovering = true;
            gameObject.transform.parent.GetComponent<Image>().color = new Color(0f, 0.443f, 0.737f);
            if (other.gameObject.transform.parent.name.Equals("RaycastRightHand"))
                _isRight = true;
            else
                _isRight = false;

            OvidVRControllerClass.DeviceController.ControllerHapticPulse(_isRight ? OvidVRControllerClass.OvidVRHand.right : OvidVRControllerClass.OvidVRHand.left, 0.1f);

            if(_HoverSource != null)
                _HoverSource.PlayOneShot(_Hover);


        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.name.Equals("RaycastRightHandLogin") || other.name.Equals("Sphere"))
        {
            if (OvidVRControllerClass.DeviceController.GetTriggerStrength(_isRight ? OvidVRControllerClass.OvidVRHand.right : OvidVRControllerClass.OvidVRHand.left) > 0.6f)
            {
                OvidVRControllerClass.DeviceController.ControllerHapticPulse(_isRight ? OvidVRControllerClass.OvidVRHand.right : OvidVRControllerClass.OvidVRHand.left, 1f);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name.Equals("RaycastRightHandLogin") || other.name.Equals("Sphere"))
        {
            gameObject.transform.parent.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f);
            _hovering = false;
        }
    }

    private void Update()
    {
        if (_isRight & _hovering)
            _animatorRight.SetFloat(("BlendGrip"), 1);
        if (!_isRight & _hovering)
            _animatorLeft.SetFloat(("BlendGrip"), 1);
    }
}
