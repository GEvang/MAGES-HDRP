using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OvidVRPhysX;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class ConfigureChildMovment : MonoBehaviour {
    
    private ConfigurableJoint _cfj;

    [SerializeField]
    private OvidVRInteractableItem _interactableItem;
    [SerializeField]
    private OvidVRInteractableItem _parentInteractableItem;

    public bool InteractOnlyWhenParentIsAttached = true;

    [Header("Translation Limit")]
    public float limitTranslationOffset;
    public bool allowAxisXMovement;
    public bool allowAxisYMovement;
    public bool allowAxisZMovement;

    public bool TranslationOnlyOneWay = true;

    [Header("Angular X Limits")]

    [SerializeField, Range(-180, 0)]
    private float xLower;
    [SerializeField, Range(0, 180)]
    private float xUpper;


    [Header("Angular Y Limits")]
    [SerializeField, Range(-180, 0)]
    private float yLower;
    [SerializeField, Range(0, 180)]
    private float yUpper;

    [Header("Angular Z Limits")]
    [SerializeField, Range(-180, 0)]
    private float zLower;
    [SerializeField, Range(0, 180)]
    private float zUpper;


    [Header("Translation - Rotation Motions")]
    public ConfigurableJointMotion XMovmentMotion = ConfigurableJointMotion.Limited;
    public ConfigurableJointMotion YMovmentMotion = ConfigurableJointMotion.Limited;
    public ConfigurableJointMotion ZMovmentMotion = ConfigurableJointMotion.Limited;

    public ConfigurableJointMotion XRotationMotion = ConfigurableJointMotion.Limited;
    public ConfigurableJointMotion YRotationMotion = ConfigurableJointMotion.Limited;
    public ConfigurableJointMotion ZRotationMotion = ConfigurableJointMotion.Limited;

    public bool EnalbeKinematicOnDetach = true;

    IEnumerator Start()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        if(_interactableItem == null)
            _interactableItem = gameObject.AddComponent<OvidVRInteractableItem>();
        _interactableItem.EnableKinematicOnDetach = EnalbeKinematicOnDetach;

        var parent = transform.parent;


        if(InteractOnlyWhenParentIsAttached)
        {
            if(_parentInteractableItem == null)
                _parentInteractableItem = parent.root.GetComponent<OvidVRInteractableItem>();
            _parentInteractableItem.OnBeginInteraction.AddListener(() => { _interactableItem.CanAttach = true; });
            _parentInteractableItem.OnEndInteraction.AddListener(() => { _interactableItem.CanAttach = false; });
            _interactableItem.CanAttach = _parentInteractableItem.IsAttached;
            yield return null;
        }

        _cfj = gameObject.AddComponent<ConfigurableJoint>();
        _cfj.anchor = Vector3.zero;
        GameObject jointRbTarget = new GameObject("ConfigurableJointTarget");
        jointRbTarget.transform.parent = parent;
        yield return null;

        Rigidbody rbTarget = jointRbTarget.AddComponent<Rigidbody>();
        rbTarget.isKinematic = true;
        yield return null;

        var currentTransform = transform;
        jointRbTarget.transform.position = currentTransform.position;
        jointRbTarget.transform.rotation = currentTransform.rotation;
        jointRbTarget.transform.localScale = currentTransform.localScale;
        _cfj.connectedBody = rbTarget; 
        yield return null;

        SoftJointLimit sjl = _cfj.linearLimit;
        sjl.limit = limitTranslationOffset;
        _cfj.linearLimit = sjl;
        yield return null;

        _cfj.autoConfigureConnectedAnchor = false;
        //Translation Limits
        if(allowAxisXMovement&& TranslationOnlyOneWay) 
            _cfj.connectedAnchor += new Vector3(limitTranslationOffset / transform.lossyScale.x, 0, 0);

        if (allowAxisYMovement && TranslationOnlyOneWay)
            _cfj.connectedAnchor += new Vector3(0, limitTranslationOffset / transform.lossyScale.y,0);

        if (allowAxisZMovement && TranslationOnlyOneWay)
            _cfj.connectedAnchor += new Vector3(0, 0, limitTranslationOffset / transform.lossyScale.z);
        yield return null;

        //Roation Limits
        float xAbsAvg = (xUpper - yLower) / 2;
        float yAbsAvg = (yUpper - yLower) / 2;
        float zAbsAvg = (zUpper - zLower) / 2;

        sjl.limit = xAbsAvg;
        _cfj.highAngularXLimit = sjl;

        sjl.limit = -xAbsAvg;
        _cfj.lowAngularXLimit = sjl;

        sjl.limit = yAbsAvg;
        _cfj.angularYLimit = sjl;

        sjl.limit = zAbsAvg;
        _cfj.angularZLimit = sjl;



        //Motions Limits
        _cfj.xMotion = XMovmentMotion;
        _cfj.yMotion = YMovmentMotion;
        _cfj.zMotion = ZMovmentMotion;

        _cfj.angularXMotion = XRotationMotion;
        _cfj.angularYMotion = YRotationMotion;
        _cfj.angularZMotion = ZRotationMotion;
        
        yield return null;

    }





}
