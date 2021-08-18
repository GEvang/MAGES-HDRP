using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressUIDynamicReposition : MonoBehaviour {

    [SerializeField, Tooltip("Drag here the object that the Progress UI will follow ONLY regarding its position. SinglePlayer ONLY")]
    private Transform followObject;

    [SerializeField, Tooltip("Add manually the position that the ProgressUIDynamicReposition must have when Coop is selected")]
    private Vector3 CoopPosition;
    [SerializeField, Tooltip("Add manually the rotation that the ProgressUIDynamicReposition must have when Coop is selected")]
    private Quaternion CoopRotation;

    private Vector3 prevPos, offset;
    private bool allowUpdate = false;

    void Start () {

        if (followObject != null)
        {
            prevPos = followObject.position;
            offset = transform.localPosition - prevPos;

            allowUpdate = true;
        }
    }

    void LateUpdate()
    {
        if (!allowUpdate)
            return;

        if (followObject.position == prevPos)
            return;

        prevPos = followObject.position;
        transform.localPosition = prevPos + offset;
    }

    public void UpdateGameStatus(bool _isSinglePlayer)
    {
        if (!_isSinglePlayer)
        {
            transform.position = CoopPosition;
            transform.rotation = CoopRotation;

            // alwyas disable in on Coop
            enabled = false;
            return;
        }

        if(!allowUpdate || followObject == null)
            enabled = false;
    }
}
