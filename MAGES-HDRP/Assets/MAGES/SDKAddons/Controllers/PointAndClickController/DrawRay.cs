using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawRay : MonoBehaviour
{
    #region VariableDeclarations
    public LineRenderer ControllerLineRenderer;
    public GameObject RaycastEnd;
    private bool _isRight, _animate, _wasClose, _hasVibratedSmall, _hasVibratedBig, _wasClosePressed;
    private OvidVRControllerClass _deviceController;
    private Animator _animatorRight, _animatorLeft;
    private GameObject _lastButtonHit;
    private bool _releasedAfter;
    #endregion

    #region Initialization

    private void Start()
    {
        Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        ControllerLineRenderer.SetPositions(initLaserPositions);
        _deviceController = GameObject.Find("OvidVRDeviceController").GetComponent<OvidVRControllerClass>();
        if (name.Equals("RaycastRightHand"))
            _isRight = true;
        else
            _isRight = false;
        _animatorRight = GameObject.Find("OvidVRDeviceController").GetComponent<OvidVRControllerClass>().rightHand.GetComponent<Animator>();
        _animatorLeft = GameObject.Find("OvidVRDeviceController").GetComponent<OvidVRControllerClass>().leftHand.GetComponent<Animator>();
        _animate = false;
        _wasClose = false;
        _hasVibratedBig = false;
        _hasVibratedSmall = false;
        _lastButtonHit = null;
        _wasClosePressed = false;
        _releasedAfter = true;
    }
    #endregion

    #region RaycastLoop
    // Update is called once per frame
    void Update()
    {

        LayerMask UserHandsLayer = 1 << LayerMask.NameToLayer("UserHands");
        LayerMask DefaultLayer = 1 << LayerMask.NameToLayer("Default");

        LayerMask ignoreLayer = UserHandsLayer | DefaultLayer;
        ignoreLayer = ~ignoreLayer;
        RaycastHit hit;

        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, ignoreLayer, QueryTriggerInteraction.Collide))
        {
            UpdateLineRendererOnHit(hit);

            if (hit.collider.gameObject.GetComponent<ButtonBehavior>() != null) //The raycast hits a button
                OnRaycastHitButton(hit);
            else //Raycast does not hit a button.
                RaycastUINotButton();

            //Cases where the line renderer is disabled -- close hand interaction and line renderer starting point being in front of the respective end point.
            DisableLineRendererCloseInteraction();
        }
        //Raycast does not hit a UI in general.
        else
            ResetRaycast();

    }
    #endregion

    #region RaycastControlFunctions
    private void DisableLineRendererCloseInteraction()
    {
        if ((Vector3.Distance(transform.position, ControllerLineRenderer.GetPosition(1)) <= 0.15f) || (Vector3.Distance(ControllerLineRenderer.GetPosition(0), ControllerLineRenderer.GetPosition(1)) <= 0.15f))
        {
            ControllerLineRenderer.enabled = false;
            RaycastEnd.SetActive(false);
        }
        else
        {
            ControllerLineRenderer.enabled = true;
            RaycastEnd.SetActive(true);
        }
    }

    private void OnRaycastHitButton(RaycastHit hit)
    {
        //Change hand to pointing animation.
        if (_isRight)
            _animatorRight.SetFloat(("BlendGrip"), 1);
        else
            _animatorLeft.SetFloat("BlendGrip", 1);

        if (hit.collider.gameObject.GetComponent<ButtonBehavior>().GetIsButtonInteractive() == false)
            return;

        //Hover and small vibration
        if (!_hasVibratedSmall)
        {
            hit.collider.gameObject.GetComponent<ButtonBehavior>().ButtonHover(true);
            OvidVRControllerClass.DeviceController.ControllerHapticPulse(_isRight ? OvidVRControllerClass.OvidVRHand.right : OvidVRControllerClass.OvidVRHand.left, 0.5f);
            _hasVibratedSmall = true;
        }

        if (!hit.collider.gameObject.GetComponent<ButtonBehavior>().GetIsButtonHovered())
            hit.collider.gameObject.GetComponent<ButtonBehavior>().ButtonHover(true);

        // Raycast - Button press and big vibration
        if (OvidVRControllerClass.DeviceController.GetTriggerStrength(_isRight ? OvidVRControllerClass.OvidVRHand.right : OvidVRControllerClass.OvidVRHand.left) > 0.6f)
        {
            if (_releasedAfter)
            {
                hit.collider.gameObject.GetComponent<ButtonBehavior>().ButtonPress();
                _releasedAfter = false;
                OvidVRControllerClass.DeviceController.ControllerHapticPulse(_isRight ? OvidVRControllerClass.OvidVRHand.right : OvidVRControllerClass.OvidVRHand.left, 1f);
            }
        }

        // Close Interaction
        else
        {
            //Raycast hits a button and the hand touches the button --> ButtonPress!
            if ((Vector3.Distance(RaycastEnd.transform.position, transform.position) < 0.1f))
            {
                if (!_wasClosePressed)
                {
                    hit.collider.gameObject.GetComponent<ButtonBehavior>().ButtonPress();
                    if (!_hasVibratedBig)
                    {
                        OvidVRControllerClass.DeviceController.ControllerHapticPulse(_isRight ? OvidVRControllerClass.OvidVRHand.right : OvidVRControllerClass.OvidVRHand.left, 1f);
                        _hasVibratedBig = true;
                    }
                    _wasClosePressed = true;
                }
                _wasClose = true;
            }
            //Hand moved away from the button but raycast still hits it.
            else
            {
                if (_wasClose)
                {
                    _wasClose = false;
                }
                _wasClosePressed = false;

            }
            _releasedAfter = true;
        }

        //If raycast hits a different button than the last one, disable its hover animation.
        if (_lastButtonHit != null && (_lastButtonHit != hit.collider.gameObject))
        {
            _lastButtonHit.GetComponent<ButtonBehavior>().ButtonHover(false);
            _hasVibratedSmall = false;
        }

        _lastButtonHit = hit.collider.gameObject;
    }

    private void RaycastUINotButton()
    {
        if (_lastButtonHit != null)
            _lastButtonHit.GetComponent<ButtonBehavior>().ButtonHover(false);
        _lastButtonHit = null;
        _hasVibratedSmall = false;
        _hasVibratedBig = false;
        _wasClosePressed = false;
    }

    private void ResetRaycast()
    {
        if (_lastButtonHit != null)
        {
            _lastButtonHit.GetComponent<ButtonBehavior>().ButtonHover(false);
            _lastButtonHit = null;
        }
        _hasVibratedSmall = false;
        _hasVibratedBig = false;
        _wasClosePressed = false;

        ControllerLineRenderer.SetPosition(0, transform.position + transform.TransformDirection(Vector3.forward) * 0.3f);
        ControllerLineRenderer.SetPosition(1, transform.position + transform.TransformDirection(Vector3.forward) * 3);
        RaycastEnd.transform.position = transform.position + transform.TransformDirection(Vector3.forward) * 3.5f;
        if (!_animate)
        {
            ControllerLineRenderer.enabled = true;
            RaycastEnd.SetActive(true);
        }
    }

    private void UpdateLineRendererOnHit(RaycastHit hit)
    {
        ControllerLineRenderer.SetPosition(0, transform.position + transform.TransformDirection(Vector3.forward) * 0.3f);
        ControllerLineRenderer.SetPosition(1, hit.point - transform.TransformDirection(Vector3.forward) * 0.15f);
        RaycastEnd.transform.position = hit.point;
    }
    #endregion
}
