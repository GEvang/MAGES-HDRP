using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ovidVR.AmbientSounds
{
    public class ApplicationAmbientSounds : MonoBehaviour
    {
        [Header("For: Login - Remove Headset - Finish & Analytics")]
        public AudioClip ambientMusic;
        [SerializeField, Range(0f,1f)]
        private float ambientMusicSoundLevel = 0.2f;

        [Header("For: Gameplay as Background Sound")]
        public AudioClip ambientNoise;
        [SerializeField, Range(0f, 1f)]
        private float ambientNoiseSoundLevel = 0.11f;

        private AudioSource audioSource;

        private static ApplicationAmbientSounds ambientManager;

        public static ApplicationAmbientSounds Get
        {
            get
            {
                if (!ambientManager)
                {
                    ambientManager = FindObjectOfType(typeof(ApplicationAmbientSounds)) as ApplicationAmbientSounds;

                    if (!ambientManager) { Debug.LogError("Error, No ApplicationAmbientSounds was found"); }
                }

                return ambientManager;
            }
        }

        void Start()
        {
            if (GetComponent<AudioSource>() == null)
            {
                Debug.LogError("No AudioSOurce found in: " + gameObject.name);
                Destroy(this);
                return;
            }

            audioSource = GetComponent<AudioSource>();
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }
   
        public static void PlayAmbientMusic()
        {
            Get.audioSource.Stop();
            Get.audioSource.clip = Get.ambientMusic;
            Get.audioSource.volume = Get.ambientMusicSoundLevel;
            Get.audioSource.loop = true;

            Get.audioSource.Play();
        }

        public static void PlayAmbientNoise()
        {
            Get.audioSource.Stop();
            Get.audioSource.clip = Get.ambientNoise;
            Get.audioSource.volume = Get.ambientNoiseSoundLevel;
            Get.audioSource.loop = true;

            Get.audioSource.Play();
        }

        public static void StopAmbientAudio()
        {
            Get.audioSource.Stop();
        }

        public static void PlayCustomAudio(AudioClip _clip, float _volume, bool _loop)
        {
            if (_clip == null)
                return;

            Get.audioSource.Stop();
            Get.audioSource.clip = _clip;
            Get.audioSource.volume = Mathf.Clamp(_volume, 0f, 1f);
            Get.audioSource.loop = _loop;

            Get.audioSource.Play();
        }

        // Enumerators --------------------------------------

        /// <summary>
        /// Future Fade in - out sounds in case volume popping is not ideal
        /// </summary>
        /// <param name="_start"></param>
        /// <param name="_end"></param>
        /// <returns></returns>
        private IEnumerator UpdateAudioVolume(float _start, float _end)
        {
            float timer = 0f, duration = 1f;

            // If rise volume from zero -> start sound as well
            if (audioSource.volume == 0)
                audioSource.Play();

            while (timer <= duration)
            {
                audioSource.volume = Mathf.Lerp(_start, _end, timer / duration);

                timer += Time.deltaTime;
                yield return null;
            }

            // If lower volume to zero -> stop sound as well
            if (audioSource.volume == 0)
                audioSource.Stop();
        }
    }
}
