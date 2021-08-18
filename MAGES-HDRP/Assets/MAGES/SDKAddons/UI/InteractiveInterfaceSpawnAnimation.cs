using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ovidVR.UIManagement;
using ovidVR.Utilities.prefabSpawnManager.prefabSpawnConstructor;

public class InteractiveInterfaceSpawnAnimation : MonoBehaviour {

	[HideInInspector]
	public float spawnInitFadeDur = 0.2f;
	[HideInInspector]
	public float spawnHeaderImageFadeDur = 0.6f;
	[HideInInspector]
	public float observeHeaderImageDur = 0.6f;
	[HideInInspector]
	public float spawnButtonsDur = 0.8f;
	[HideInInspector]
	public float totalFadeOutDur = 0.8f;

	public UnityEvent eventsAfterInterfaceInit;

	private Transform leftCorner, leftCornerFinalPos;
	private Transform rightCorner, rightCornerFinalPos;

	private Image interfaceHeaderImage;

	private Image leftImage, rightImage;

	private Transform contentTransform;

	private AudioSource audioSource;

	// As long as the animation plays - disable button itneractivity (for already interactive buttons only)
	private Dictionary<ButtonBehavior, bool> allButtons;

	private bool isSpawned, isDestroyed;

	void Start () {
		if (!transform.Find("InterfaceSpawn") || !transform.Find("InterfaceContent"))
		{ Destroy(this); return; }

		InterfaceManagement.Get.UpdateSpanwedInterfaceCount();

		// Initialize all
		leftCorner = transform.Find("InterfaceSpawn/Corners/LeftUpCorner");
		rightCorner = transform.Find("InterfaceSpawn/Corners/RightDownCorner");

		leftCornerFinalPos = transform.Find("InterfaceSpawn/CornersFinalPos/LeftFinal");
		rightCornerFinalPos = transform.Find("InterfaceSpawn/CornersFinalPos/RightFinal");

		interfaceHeaderImage = transform.Find("InterfaceSpawn/CenterInterfaceHeader").GetComponent<Image>();

        if(leftCorner)
		    leftImage = leftCorner.GetComponent<Image>();

        if(rightCorner)
		    rightImage = rightCorner.GetComponent<Image>();

		contentTransform = transform.Find("InterfaceContent");
        //contentTransform.gameObject.SetActive(false);
        if (leftCornerFinalPos)
        {
            if (leftCornerFinalPos.GetComponent<Image>())
                leftCornerFinalPos.GetComponent<Image>().enabled = false;
        }


        if (rightCornerFinalPos)
        {
            if (rightCornerFinalPos.GetComponent<Image>())
                rightCornerFinalPos.GetComponent<Image>().enabled = false;
        }
		

		audioSource = GetComponent<AudioSource>();
		if (audioSource)
		{
			audioSource.clip = InterfaceManagement.Get.spawnUISound;
			audioSource.Play();
		}

		allButtons = new Dictionary<ButtonBehavior, bool>();	

		isSpawned = false;
		isDestroyed = false;

		if(!gameObject.GetComponent<QuestionPrefabConstructor>())
			StartCoroutine("AnimateCreation");
	}
	
	void OnDisable()
	{
		InterfaceManagement.Get.UpdateSpanwedInterfaceCount(false);
		StopAllCoroutines();
	}

	IEnumerator AnimateCreation()
	{
		isSpawned = true;

		Color headerColor = interfaceHeaderImage.color;
		interfaceHeaderImage.color = headerColor;

		Transform imageTransform = interfaceHeaderImage.transform;
		imageTransform.localScale = new Vector3();

		Vector3 LC_pos1 = leftCorner.localPosition;
		Vector3 RC_pos1 = rightCorner.localPosition;

		leftCorner.localPosition = new Vector3();
		rightCorner.localPosition = new Vector3();

		contentTransform.localScale = new Vector3();

		float timer = 0f;
		float duration = spawnInitFadeDur;

		Color imgColors = leftImage.color;
		imgColors.a = 0f;
		leftImage.color = imgColors;
		rightImage.color = imgColors;

		while (timer <= duration)
		{
			float newAlpha = Mathf.Lerp(0f, 1f, timer / duration);
			imgColors.a = newAlpha;
			leftImage.color = imgColors;
			rightImage.color = imgColors;

			timer += Time.deltaTime;
			yield return null;
		}

		timer = 0f;
		duration = spawnHeaderImageFadeDur;

		// Disable Buttons Before Scaling up
		foreach (ButtonBehavior bb in GetComponentsInChildren<ButtonBehavior>())
		{
			allButtons.Add(bb, bb.GetIsButtonInteractive());
			bb.ButtonInteractivity(false);
		}


		while (timer <= duration)
		{

			leftCorner.localPosition = new Vector3(Mathf.SmoothStep(0f, LC_pos1.x, timer / duration),
													Mathf.SmoothStep(0f, LC_pos1.y, timer / duration), 0f);

			rightCorner.localPosition = new Vector3(Mathf.SmoothStep(0f, RC_pos1.x, timer / duration),
													Mathf.SmoothStep(0f, RC_pos1.y, timer / duration), 0f);

			imageTransform.localScale = new Vector3(Mathf.SmoothStep(0f, 1f, timer / duration),
													Mathf.SmoothStep(0f, 1f, timer / duration), 1f);
			

			timer += Time.deltaTime;
			yield return null;
		}

		yield return new WaitForSeconds(observeHeaderImageDur);

		timer = 0f;
		duration = spawnButtonsDur;

		// Enable Content and create a small depth effect
		//contentTransform.gameObject.SetActive(true);
		contentTransform.localPosition += new Vector3(0f, 0f, 0.2f);

		while (timer <= duration)
		{
			leftCorner.localPosition = new Vector3(Mathf.SmoothStep(LC_pos1.x, leftCornerFinalPos.localPosition.x, timer / duration),
													Mathf.SmoothStep(LC_pos1.y, leftCornerFinalPos.localPosition.y, timer / duration), 0f);

			rightCorner.localPosition = new Vector3(Mathf.SmoothStep(RC_pos1.x, rightCornerFinalPos.localPosition.x, timer / duration),
													Mathf.SmoothStep(RC_pos1.y, rightCornerFinalPos.localPosition.y, timer / duration), 0f);

			contentTransform.localPosition = new Vector3(0f, 0f, 
														 Mathf.SmoothStep(0.2f, 0f, timer / duration));

			contentTransform.localScale = new Vector3(Mathf.SmoothStep(0f, 1f, timer / duration),
													  Mathf.SmoothStep(0f, 1f, timer / duration),
													  Mathf.SmoothStep(0f, 1f, timer / duration));

			if(headerColor.a > 0f)
			{
				headerColor.a = Mathf.Lerp(1f, 0f, timer / (duration / 4f));
				interfaceHeaderImage.color = headerColor;
			}

			timer += Time.deltaTime;
			yield return null;
		}

		yield return null;

		if(allButtons.Count != 0)
		{
			// return button interactive to it's previous state
			foreach (KeyValuePair<ButtonBehavior, bool> entry in allButtons)		
				entry.Key.ButtonInteractivity(entry.Value);		
		}
		

		if (eventsAfterInterfaceInit != null)
			eventsAfterInterfaceInit.Invoke();

		isSpawned = false;
	}

	IEnumerator AnimateDestruction()
	{
		isDestroyed = true;

		Vector3 currLeftPos = leftCorner.localPosition;
		Vector3 currRightPos = rightCorner.localPosition;

		// Fade out is split into two aniamtions. 1: corners are centered, 2: corners are faded out
		float timer = 0f, duration = (totalFadeOutDur / 2f);

		Color headerColor = interfaceHeaderImage.color;

		while (timer <= duration)
		{

			leftCorner.localPosition = new Vector3(Mathf.SmoothStep(currLeftPos.x, 0f, timer / duration),
													Mathf.SmoothStep(currLeftPos.y, 0f, timer / duration), 0f);

			rightCorner.localPosition = new Vector3(Mathf.SmoothStep(currRightPos.x, 0f, timer / duration),
													Mathf.SmoothStep(currRightPos.y, 0f, timer / duration), 0f);

			contentTransform.localScale = new Vector3(Mathf.SmoothStep(contentTransform.localScale.x, 0f, timer / duration),
													Mathf.SmoothStep(contentTransform.localScale.y, 0f, timer / duration), 1f);

			if (headerColor.a > 0f)
			{
				headerColor.a = Mathf.Lerp(headerColor.a, 0f, timer / (duration / 4f));
				interfaceHeaderImage.color = headerColor;
			}

			timer += Time.deltaTime;
			yield return null;
		}

		timer = 0f;

		Color imgColors = leftImage.color;

		while (timer <= duration)
		{
			float newAlpha = Mathf.Lerp(leftImage.color.a, 0f, timer / duration);
			imgColors.a = newAlpha;
			leftImage.color = imgColors;
			rightImage.color = imgColors;

			timer += Time.deltaTime;
			yield return null;
		}

		Destroy(this.gameObject);

		isDestroyed = false;
	}

	public void DestroyInterface()
	{
		StopAllCoroutines();

		if (audioSource)
		{
			// If while spawning AudioSpeech was playing the prefab will have volume of 0.2
			if (!InterfaceManagement.Get.GetIsSpeechPlaying())
				audioSource.volume = 1f;

			audioSource.clip = InterfaceManagement.Get.destroyUISound;
			audioSource.Play();
		}

		StartCoroutine("AnimateDestruction");
	}

	public bool GetIsInterfaceCurrentlySpawned()
	{
		return isSpawned;
	}

	public bool GetIsInterfaceCurrentlyDestroyed()
	{
		return isDestroyed;
	}

	public float GetSpawnAnimationTime()
	{
		return spawnInitFadeDur + spawnHeaderImageFadeDur + observeHeaderImageDur + spawnButtonsDur;
}

	public float GetDestructionAnimationTime()
	{
		return 1f;
	}
}
