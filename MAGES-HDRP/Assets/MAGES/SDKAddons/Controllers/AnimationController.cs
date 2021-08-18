using UnityEngine;
using ovidVR.GameController;

public class AnimationController : MonoBehaviour
{
    Animator anim;
    
    OvidVRControllerClass.OvidVRHand currentHand; 

    private bool OverrideDefaultState = false;
    private string OverrideLayerName = "";
    // Use this for initialization
    void Start()
    {
        currentHand = gameObject.transform.tag == "LeftPalm" ?
            OvidVRControllerClass.OvidVRHand.left : OvidVRControllerClass.OvidVRHand.right;
        anim = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (OverrideDefaultState)
        {
            anim.SetFloat(("BlendGrip"),0);
            anim.SetFloat(("BlendTrigger"), 0);
        }
        else
        {
            anim.SetFloat(("BlendGrip"), OvidVRControllerClass.Get.GetControllerGrabStrength(currentHand));
            anim.SetFloat(("BlendTrigger"), OvidVRControllerClass.Get.GetTriggerStrength(currentHand));
        }
    }

    public void OverrideDefaultWithLayer(string layer)
    {
        OverrideDefaultState = true;
        OverrideLayerName = layer;
        int layerIndex = anim.GetLayerIndex(layer);
        anim.SetLayerWeight(layerIndex, 1.0f);
        anim.Play("Idle", layerIndex, 0f);
    }

    public void ResetAnimator()
    {
        OverrideDefaultState = false;
        int layerIndex = anim.GetLayerIndex(OverrideLayerName);
        anim.SetLayerWeight(layerIndex, 0.0f);
    }

}
