using ovidVR.CustomEventManager;
using ovidVR.sceneGraphSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    private Vector3 screenPoint;
    private Vector3 offset;

    private void Start()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        GetComponent<Rigidbody>().isKinematic = true;
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    void OnMouseDown()
    {

    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        GetComponent<Rigidbody>().MovePosition(curPosition);

    }

    private void OnMouseUp()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().useGravity = true;
        EventManager.TriggerEvent(ScenegraphTraverse.GetCurrentAction().name);
    }

}
