using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.UIManagement;
using System;
using ovidVR.GameController;

public class UICollisionAvoidance : MonoBehaviour {

    public enum LerpType { linear, logarithmic, smoothStep };

    private new Transform transform;

    private new Collider collider;
    private Transform colliderParent;

    private List<Transform> expandingColliders;

    private bool isExpanding = false, isLerping = false;
    // stop collision avoidance (a.k.a. successful lerp) after some attempts (see StartLerping)
    private int timesLerped = 0;

    [SerializeField, Range(0.1f, 2f), Tooltip("Set how long the colliders should expand to find a free space for the UI")]
    private float maxExpansionTimer = 1f;

    [SerializeField, Range(0.5f, 4f), Tooltip("Set how long the UI should lerp to it's new unobstructed position")]
    private float lerpTimer = 2f;

    private float lerpSpeedMul = 0.25f;

    [SerializeField]
    private LerpType lerpType = LerpType.logarithmic;

    private float timer = 0;

    Vector3 startPos, endPos;

    private void Start () {
        transform = GetComponent<Transform>();

        colliderParent = transform.Find("ExpandingCollider");
        if(colliderParent == null)
        {
            Debug.LogError("No child ExpandingCollider found in: " + gameObject.name);
            Destroy(this);
            return;
        }

        collider = colliderParent.GetComponent<Collider>();

        expandingColliders = new List<Transform>();

        startPos = transform.localPosition;
        endPos = new Vector3();
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        DestroyExpansionColliders();
    }

    private void FixedUpdate ()
    {
        if (!isExpanding && !isLerping)
            return;

        timer += Time.deltaTime;     

        if (isLerping)
        {
            float timeStep = timer / lerpTimer;

            if(lerpType == LerpType.logarithmic)
                timeStep = Mathf.Sin(timeStep * Mathf.PI * 0.5f);
            else if (lerpType == LerpType.smoothStep)
                timeStep = timeStep * timeStep * timeStep * (timeStep * (6f * timeStep - 15f) + 10f);

           transform.localPosition = Vector3.Lerp(startPos, endPos, timeStep);

            if(timeStep >= 1f)
                ResetScript();

            return;
        }
        
        if(isExpanding)
            ExpandColliders(timer <= maxExpansionTimer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isExpanding || isLerping)
            return;

        // Ignore users hand from collision
        if(other.gameObject.layer == LayerMask.NameToLayer("UserHands"))
            return;

        // UISpawn sends a message on Start to UIManagement to play a sound. Stop it for the colldier expansions
        UISpawn uiSpawnScript = null;
        bool storeUISoundState = true, storeUIAnimState = true;

        if (GetComponent<UISpawn>() != null)
        {
            uiSpawnScript = GetComponent<UISpawn>();
            storeUISoundState = uiSpawnScript.allowUISound;
            storeUIAnimState = uiSpawnScript.allowUIAnimations;
            uiSpawnScript.allowUISound = false;
            uiSpawnScript.allowUIAnimations = false;
        }

        GameObject newCollider = null;

        if (expandingColliders != null)
            expandingColliders.Clear();
        else
            expandingColliders = new List<Transform>();

        for(int i = 0; i < 5; ++i)
        {           
            newCollider = Instantiate(this.gameObject, transform.parent);
            newCollider.name = gameObject.name + "_ColliderExpansion_" + (i + 1).ToString();  

            // Delete All expansion UIs children expect the collider
            foreach (Transform child in newCollider.transform)
            {
                if(child.gameObject.name != "ExpandingCollider")
                    Destroy(child.gameObject);
            }

            Physics.IgnoreCollision(collider, newCollider.GetComponentInChildren<Collider>());

            // Disable or Destroy unwanted components
            if (newCollider.GetComponent<Canvas>() != null)
                newCollider.GetComponent<Canvas>().enabled = false;

            MonoBehaviour[] allColliderScripts = newCollider.GetComponents<MonoBehaviour>();
            if(allColliderScripts != null)
            {
                foreach (MonoBehaviour m in allColliderScripts)
                    Destroy(m);
            }        

            UIExpandingCollider uec = newCollider.AddComponent<UIExpandingCollider>();
            uec.Initialize(this, i);     

            expandingColliders.Add(newCollider.transform);
        }

        timer = 0f;
        isExpanding = true;

        // Reset It's own UIspawn state, if it has one
        if(uiSpawnScript != null)
        {
            uiSpawnScript.allowUISound = storeUISoundState;
            uiSpawnScript.allowUIAnimations = storeUIAnimState;
        }
    }

    // Private Functions -----------------------------------------

    private void ExpandColliders(bool _expand)
    {
        if (!_expand)
        {
            DestroyExpansionColliders();

            // 1st attempt: if free space not found go to 2nd attempt with x2 range
            // 2nd attempt: if free space not found selfdestroy (script) and inform user
            if (lerpSpeedMul <= 0.25f)
                ResetScript(lerpSpeedMul + 0.25f);
            else
            {
                if(UIManagement.Get.transform.Find("CurrentActiveSpecialCaseUI").childCount == 0)
                    UIManagement.SpawnSpecialCases("UISpaceWarning", UIManagement.Get.rightController);

                Destroy(this);
            }

            return;
        }

        expandingColliders[0].Translate((Vector3.forward + Vector3.right) * Time.deltaTime * lerpSpeedMul, Space.Self);
        expandingColliders[1].Translate((Vector3.forward - Vector3.right) * Time.deltaTime * lerpSpeedMul, Space.Self);
        expandingColliders[2].Translate(Vector3.forward * Time.deltaTime * lerpSpeedMul, Space.Self);
        expandingColliders[3].Translate((Vector3.forward + Vector3.up) * Time.deltaTime * lerpSpeedMul, Space.Self);
        expandingColliders[4].Translate((Vector3.forward - Vector3.up) * Time.deltaTime * lerpSpeedMul, Space.Self);
    }

    // Public Functions ------------------------------------------

    public void StartLerping(Vector3 _expandColliderPosition)
    {
        /*if(timesLerped == 4)
        {
            Destroy(this);
            return;
        }*/

        if (isLerping)
            return;

        ++timesLerped;

        isLerping = true;
        isExpanding = false;
        collider.enabled = false;
        timer = 0;

        endPos = _expandColliderPosition;

        DestroyExpansionColliders();
    }
    
    public void DestroyExpansionColliders()
    {
        if (expandingColliders == null || expandingColliders.Count == 0)
            return;

        foreach (Transform expCol in expandingColliders)
        {
            if(expCol != null)
                Destroy(expCol.gameObject);
        }

        expandingColliders.Clear();
    }

    public void ResetScript(float _speed = 0.25f)
    {
        if(endPos != new Vector3())
            transform.position = endPos;

        startPos = transform.localPosition;
        endPos = new Vector3();
        isLerping = false;
        isExpanding = false;
        collider.enabled = true;

        timer = 0f;

        lerpSpeedMul = _speed;

        expandingColliders = new List<Transform>();
    }
}
