using UnityEngine;
using ovidVR.Utilities.VoiceActor;
using System.Collections;

public class VoiceActorImporter : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        StartCoroutine(ImportVoiceActor());
    }


    private IEnumerator ImportVoiceActor()
    {
        yield return new WaitForEndOfFrame();

        VoiceActor.SetAudioForActor("excellent", "VoiceActing/excellent");
    }

}
