using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioExtentions  {

    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime, float maxVolume = 1.0f)
    {
        float startVolume = maxVolume/10.0f;

        audioSource.volume = 0;
        audioSource.Play();

        while (audioSource.isPlaying &&
            audioSource.volume + startVolume * Time.deltaTime / FadeTime < maxVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.volume = maxVolume;
    }
}
