using ovidVR.toolManager.tool;
using ovidVR.toolManager;
using OvidVRPhysX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScissorsGestureHands : GestureHands {

    //public bool grabStrength = false;

    //private Animation anim;
    //private float strength = 0f;
    //private AudioSource audioSource;
    //private Tool scissors;
    //private bool playAudio = true;
    //private OvidVRInteractableItem toolInteract;
    //private List<Transform> allToolsChildTransforms;

    //public override void SetUpTool(Tool _tool)
    //{
    //    anim = GetComponent<Animation>();
    //    audioSource = GetComponent<AudioSource>();

    //    scissors = ToolsManager.GetTool(ToolsEnum.Scissors.ToString());

    //    // Scissors -> ScissorsPivot
    //    toolInteract = transform.parent.gameObject.GetComponent<OvidVRInteractableItem>();
    //    if (toolInteract == null)
    //        Debug.LogError("NO OvidVRInteractableItem found in: " + this.tag);

    //    // Ignore tool action collider with all other tool's colliders
    //    BoxCollider toolActionCollider = GetComponent<BoxCollider>();
    //    List<Collider> allSameToolColliders = new List<Collider>();
    //    allSameToolColliders.AddRange(scissors.toolGameobject.GetComponents<Collider>());
    //    allSameToolColliders.AddRange(scissors.toolGameobject.GetComponentsInChildren<Collider>());
    //    foreach (Collider col in allSameToolColliders)
    //        Physics.IgnoreCollision(toolActionCollider, col);

    //    // SetUp tool layer (and ALL it's children)
    //    allToolsChildTransforms = new List<Transform>();
    //    allToolsChildTransforms.AddRange(scissors.toolGameobject.GetComponentsInChildren<Transform>());
    //    ChangeToolLayer(false);
    //}

    //public override void UpdateToolGesture()
    //{
    //    if (toolInteract == null || scissors == null)
    //        return;

    //    anim.Play();
    //    strength = 0f;

    //    if (scissors.ToolRightAttachment)
    //        strength = ovidVR.GameController.OvidVRControllerClass.Get.getControllerGrabStrength("right");
    //    else
    //        strength = ovidVR.GameController.OvidVRControllerClass.Get.getControllerGrabStrength("left");

    //    anim["ScissorsAnim"].time = strength;

    //    if (strength >= 0.9f && playAudio)
    //    {
    //        audioSource.Play();
    //        playAudio = false;
    //    }

    //    if (strength >= .6f)
    //    {
    //        ChangeToolLayer(true);
    //        grabStrength = true;
    //    }
    //    else
    //    {
    //        ChangeToolLayer(false);
    //        grabStrength = false;

    //        if (!playAudio)
    //            playAudio = true;
    //    }
    //}

    //public override void EndToolGesture()
    //{
    //    ChangeToolLayer(false);
    //    grabStrength = false;
    //    strength = 0f;
    //    anim["ScissorsAnim"].time = strength;

    //    if (anim.isPlaying)
    //        anim.Stop();
    //}

    //public override void ChangeToolLayer(bool _isToolActive)
    //{
    //    int newLayer;
    //    if (_isToolActive)
    //        newLayer = scissors.toolGameobject.GetComponent<ToolConstructor>().activeToolLayer;
    //    else
    //        newLayer = scissors.toolGameobject.GetComponent<ToolConstructor>().inactiveToolLayer;

    //    foreach (Transform t in allToolsChildTransforms)
    //        t.gameObject.layer = newLayer;
    //    scissors.toolGameobject.layer = newLayer;
    //}
}
