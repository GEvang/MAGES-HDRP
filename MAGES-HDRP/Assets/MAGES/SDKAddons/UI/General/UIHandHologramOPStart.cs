using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.UIManagement;

public class UIHandHologramOPStart : MonoBehaviour {

    List<Collider> allColliders;

    UIButton buttonStript;
    Animator anim;

    int handCollisionCounter;
    float entryTimer;

    bool collisionOver;

    void Start () {
        allColliders = new List<Collider>();
        allColliders.AddRange(GetComponentsInChildren<Collider>());
        anim = GetComponentInChildren<Animator>();
        anim.enabled = false;
        handCollisionCounter = 0;
        collisionOver = false;
        entryTimer = 0f;

        StartCoroutine("SetButtonLateState");
	}

    private void OnDisable()
    {
        StopAllCoroutines();

        if(buttonStript != null)
        {
            buttonStript.SetButtonActivity(true);
            buttonStript.ResetButton();
        }
    }

    IEnumerator SetButtonLateState()
    {
        yield return new WaitForSeconds(1f);      

        GameObject ui = UIManagement.GetCurrentActiveUserUI();

        if (ui == null || ui.name != "UIOperationStart")
            yield break;

        ui = ui.transform.Find("ButtonList/Button0").gameObject;
        if (ui == null || ui.GetComponent<UIButton>() == null)
            yield break;

        buttonStript = ui.GetComponent<UIButton>();
        buttonStript.SetButtonActivity(true, LanguageTranslator.SqueezeFingerSpecialCase);

        anim.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collisionOver)
            return;

        if (other.gameObject.layer == LayerMask.NameToLayer("UserHands"))
        {
            string parentTag = OvidVRControllerClass.Get.GetHandTag(other.gameObject);

            if (parentTag.Equals("RightHand") || parentTag.Equals("RightPalm")
                || parentTag.Equals("LeftHand") || parentTag.Equals("LeftPalm"))
            {
                if (handCollisionCounter == 0)
                    entryTimer = Time.time;

                ++handCollisionCounter;         
            }
        }
    }

    private void OnTriggerStay()
    {
        if (collisionOver)
            return;

        if (handCollisionCounter > 0 && (Time.time - entryTimer >= 0.4f))
        {
            collisionOver = true;
            foreach (Collider c in allColliders) { c.enabled = false; }
            Destroy(this.gameObject, 0.4f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (collisionOver)
            return;

        if (other.gameObject.layer == LayerMask.NameToLayer("UserHands"))
        {
            string parentTag = OvidVRControllerClass.Get.GetHandTag(other.gameObject);

            if (parentTag.Equals("RightHand") || parentTag.Equals("RightPalm")
                || parentTag.Equals("LeftHand") || parentTag.Equals("LeftPalm"))
            {
                --handCollisionCounter;
                if (handCollisionCounter <= 0)
                {
                    entryTimer = 0f;
                    handCollisionCounter = 0;
                }
            }
        }
    }
}
