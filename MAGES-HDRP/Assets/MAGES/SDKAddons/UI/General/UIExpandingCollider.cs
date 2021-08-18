using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIExpandingCollider : MonoBehaviour {

    UICollisionAvoidance originalColliderParentScript;

    int colliderID;

    new Collider collider;

    string thisGameObjectName = "";
    int currCollisionCounter;

    public void Initialize(UICollisionAvoidance _script, int _indexID)
    {
        // name of expanding collder: OriginalUIName_ColliderExpansion_(int)Counter
        thisGameObjectName = gameObject.name.Split('_')[0];

        currCollisionCounter = 0;

        collider = transform.Find("ExpandingCollider").GetComponent<Collider>();

        originalColliderParentScript = _script;

        if(originalColliderParentScript == null)
        {
            Debug.LogError("No Instance to UICollisionAvoidance was given. Destroying script");
            Destroy(this);
        }

        colliderID = _indexID;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null)
            return;

        string name = other.attachedRigidbody.gameObject.name.Split('_')[0];

        if (name == thisGameObjectName)
            return;

        ++currCollisionCounter;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody == null)
            return;

        string name = other.attachedRigidbody.gameObject.name.Split('_')[0];

        if (name == thisGameObjectName)
            return;

        --currCollisionCounter;

        if (currCollisionCounter <= 0)
        {
            collider.enabled = false;
            originalColliderParentScript.StartLerping(transform.localPosition);
        }

    }
}
