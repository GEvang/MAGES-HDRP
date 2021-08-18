using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.toolManager;
using ovidVR.toolManager.tool;
using ovidVR.GameController;
using OvidVRPhysX;

public class MalletAudioOnCollision : MonoBehaviour {

    AudioSource audioSource;
    AudioClip defaultAudioClip;
    AudioClip pinsAudioClip;
    Rigidbody malletRigidBody;
    OvidVRInteractableItem toolInteract;

    void Start(){
        audioSource = GetComponent<AudioSource>();
        defaultAudioClip = audioSource.clip;
        pinsAudioClip = Resources.Load("MAGESres\\Sounds\\MalletOnPins") as AudioClip;

        malletRigidBody = transform.parent.gameObject.GetComponent<Rigidbody>();
        toolInteract = transform.parent.gameObject.GetComponent<OvidVRInteractableItem>();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name.StartsWith("Cloth"))
            return;
        if (collider.gameObject.tag.Equals("LeftPalm") || collider.gameObject.tag.Equals("RightPalm"))
            return;

        float volumeMagnitudeMul = 1f;
        if(malletRigidBody != null)
            volumeMagnitudeMul = malletRigidBody.velocity.magnitude;

        if (volumeMagnitudeMul < 0.1f)
            return;
        else if (volumeMagnitudeMul > 1f)
            volumeMagnitudeMul = 1f;

        if (collider.gameObject.name.StartsWith("Pin"))
        {
            audioSource.clip = pinsAudioClip;
            audioSource.volume = 1f * volumeMagnitudeMul;
            audioSource.pitch = 1f;
        }
        else
        {
            audioSource.clip = defaultAudioClip;
            audioSource.volume = 0.1f * volumeMagnitudeMul;
            audioSource.pitch = 1.5f;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            if(toolInteract.IsAttached && toolInteract != null)
                OvidVRControllerClass.Get.ControllerHapticPulse(ToolsManager.GetTool(ToolsEnum.Mallet.ToString()).ToolRightAttachment ? OvidVRControllerClass.OvidVRHand.right : OvidVRControllerClass.OvidVRHand.left, volumeMagnitudeMul);
            StartCoroutine(StopControllerVibration());
        }
        
    }

    void OnTriggerExit(Collider collider)
    {
        StopControllerVibration();
    }

    public void StopAudio()
    {
        audioSource.Stop();

        StopControllerVibration();
    }

    private IEnumerator StopControllerVibration()
    {
        yield return new WaitForSeconds(0.02f);
        OvidVRControllerClass.Get.ControllerHapticPulse(ToolsManager.GetTool(ToolsEnum.Mallet.ToString()).ToolRightAttachment ? OvidVRControllerClass.OvidVRHand.right : OvidVRControllerClass.OvidVRHand.left, 0.0f);
    }
}
