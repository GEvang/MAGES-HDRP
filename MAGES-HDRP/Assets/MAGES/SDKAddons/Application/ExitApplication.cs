using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.UIManagement;
using ovidVR.OperationAnalytics;
using UnityEngine.SceneManagement;

namespace ovidVR.Exit
{
	public class ExitApplication : MonoBehaviour
	{
		[SerializeField, HideInInspector]
		private AudioSource[] allAudioSources;

		private bool hasApplicationExited;

		private static ExitApplication exitApplication;

		public static ExitApplication Get
		{
			get
			{
				if (!exitApplication)
				{
					exitApplication = FindObjectOfType(typeof(ExitApplication)) as ExitApplication;

					if (!exitApplication) { Debug.LogError("Error, No ExitApplication was found"); }
				}

				return exitApplication;
			}
		}

		void Awake()
		{
			hasApplicationExited = false;
		}

		void OnDisable()
		{
			StopAllCoroutines();
		}

		void Update()
		{
			if (hasApplicationExited)
				return;

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				hasApplicationExited = true;

				StartCoroutine(ExitApplicationDelay(false));
			}
		}

		private IEnumerator ExitApplicationDelay(bool _restart)
		{
			InterfaceManagement.Get.ResetInterfaceManagement();
			InterfaceManagement.Get.SpawnDynamicNotificationUI(NotificationUITypes.UINotification, "Session is over," + System.Environment.NewLine + "Goodbye!", 2f);

			allAudioSources = FindObjectsOfType<AudioSource>();
			foreach (AudioSource a in allAudioSources)
			{
				a.volume = 0f;
				a.Stop();
			}		

			AudioClip exitClip = Resources.Load("MAGESres/Sounds/ExitSound", typeof(AudioClip)) as AudioClip;
			if(exitClip != null)
			{
				AudioSource exitAudio = gameObject.AddComponent<AudioSource>();
				exitAudio.spatialBlend = 0f;
				exitAudio.loop = false;
				exitAudio.volume = 1f;
				exitAudio.clip = exitClip;
				exitAudio.Play();
			}

			yield return new WaitForSeconds(1.6f);

			yield return new WaitForSeconds(0.4f);

			if (_restart)
			{
				//UserAccountManager.IncreaseUserSession();

				Scene scene = SceneManager.GetActiveScene();
				if (scene != null)
					SceneManager.LoadScene(scene.name);
			}
			else
			{
				#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
				#else
				Application.Quit();
				#endif
			}
		}

		private GameObject[] FindGameObjectsInLayer(int layer)
		{
			var goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
			var goList = new List<GameObject>();
			for (int i = 0; i < goArray.Length; i++)
			{
				if (goArray[i].layer == layer)
				{
					goList.Add(goArray[i]);
				}
			}
			if (goList.Count == 0)
			{
				return null;
			}
			return goList.ToArray();
		}

		public static void Exit()
		{
			if (Get.hasApplicationExited)
				return;

			Get.hasApplicationExited = true;

			Get.StartCoroutine(Get.ExitApplicationDelay(false));
		}

		public static void Restart()
		{
			if (Get.hasApplicationExited)
				return;

			Get.hasApplicationExited = true;

			Get.StartCoroutine(Get.ExitApplicationDelay(true));
		}
	}
}
