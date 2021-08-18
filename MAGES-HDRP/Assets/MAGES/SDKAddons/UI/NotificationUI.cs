using System.Collections;
using UnityEngine;
using ovidVR.UIManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class NotificationUI : MonoBehaviour {

	[SerializeField, Header("Set life time in seconds. Set to 0 for inf life time"), Range(0f, 16f)]
	private float notificationLifetime = 8f;

	[SerializeField, Header("If required every Notification can have a unique ID"), Tooltip("Leave Empty. Must be filled via code")]
	private string ID = "";

	[SerializeField, HideInInspector]
	private string notificationMessage = "";

	private new Transform transform;

	private Canvas uiCanvas;

	private float lerpDuration, intervalDuration;
	private float timerCheck, timerLerp, intervals;

	private float preserveHeight;

	private bool allowUpdate, isDynamic;

	GameObject followUserHead;

	void Awake()
	{
		transform = GetComponent<Transform>();

		preserveHeight = transform.position.y;

		lerpDuration = 2f;
		intervalDuration = 2f;

		uiCanvas = GetComponent<Canvas>();
		uiCanvas.enabled = false;

		allowUpdate = false;
		isDynamic = true;	
	}

	void OnDisable()
	{
		StopAllCoroutines();

		if (followUserHead)
			DestroyImmediate(followUserHead);

		InterfaceManagement.Get.SpawnNextNotificationUIFromQueue();
	}

	void Update () {
		if (!allowUpdate || !isDynamic)
			return;

		timerCheck += Time.deltaTime;
		if (timerCheck < intervalDuration)
			return;

		timerCheck = 0f;

		if (Vector3.Distance(transform.position, followUserHead.transform.position) > 0.26f)
		{
			intervalDuration = 4f;
			StartCoroutine("SmoothStepTowardsHead");
		}
		else
			intervalDuration = 2f;
}

	// ENUMERATORS -------------------------------------------

	IEnumerator SmoothStepTowardsHead()
	{
		InterfaceManagement.Get.FacingUserInterface(followUserHead, false, 2f);

		Vector3 uiCurrPos = transform.position;
		Vector3 headCurrPos = followUserHead.transform.position;

		Quaternion uiCurrRot = transform.rotation;
		Quaternion headCurrRot = followUserHead.transform.rotation;

		timerLerp = 0f;

		while (timerLerp <= lerpDuration)
		{
			timerLerp += Time.deltaTime;
			transform.position = new Vector3(Mathf.SmoothStep(uiCurrPos.x, headCurrPos.x, timerLerp / lerpDuration),
				preserveHeight,
				Mathf.SmoothStep(uiCurrPos.z, headCurrPos.z, timerLerp / lerpDuration));

			// Rotation is done faster & Ignore X azis
			if(timerLerp <= 1f)
			{
				float x = uiCurrRot.x;
				float z = uiCurrRot.z;
				Quaternion newRot = Quaternion.Lerp(uiCurrRot, headCurrRot, timerLerp);
				newRot.x = x;
				newRot.z = z;
				transform.rotation = newRot;
			}

			yield return null;
		}
	}

	IEnumerator DestroyDelayedNotification()
	{
		yield return new WaitForSeconds(notificationLifetime);

		allowUpdate = false;

		List<Image> allImg = new List<Image>();
		allImg.AddRange(GetComponentsInChildren<Image>());

		Text txt = GetComponentInChildren<Text>();

		float timer = 0f;

		while (timer <= 0.5f)
		{
			timer += Time.deltaTime;

			float newAlpha = Mathf.Lerp(1, 0, timer / 0.5f);
			Color c;

			foreach(Image img in allImg)
			{
				c = img.color;
				c.a = newAlpha;
				img.color = c;
			}

			c = txt.color;
			c.a = newAlpha;
			txt.color = c;

			yield return null;
		}

		Destroy(this.gameObject);
	}

	// PUBLIC FUNCTIONS --------------------------------------

	/// <summary>
	/// Notifications are stores in a FIFO to get rendered.
	/// When spawned the Notification will remain static and with the Canvas
	/// disabled till this function gets called
	/// </summary>
	/// <param name="_spawnSoundVolume">if virtual speaker is playing to lower spawn sound volume</param>
	public void StartNotification(float _spawnSoundVolume = 1f)
	{
		if (followUserHead == null)
			followUserHead = new GameObject("NotificationRuntimePrefab_followHeadTransform");

		allowUpdate = true;
		uiCanvas.enabled = true;

		Animator[] allAnim = GetComponentsInChildren<Animator>();
		foreach (Animator anim in allAnim)
			anim.enabled = true;

		float preserveX = transform.rotation.x;
		float preserveZ = transform.rotation.z;

		InterfaceManagement.Get.FacingUserInterface(gameObject, false, 2f);
		transform.position = new Vector3(transform.position.x, preserveHeight, transform.position.z);

		Quaternion newRot = transform.rotation;
		newRot.x = preserveX;
		newRot.z = preserveZ;
		transform.rotation = newRot;

		InterfaceManagement.Get.FacingUserInterface(followUserHead, true, 2f);

		if (GetComponent<AudioSource>())
		{
			GetComponent<AudioSource>().volume = _spawnSoundVolume;
			GetComponent<AudioSource>().Play();
		}

		if (notificationLifetime != 0f)
			StartCoroutine("DestroyDelayedNotification");
	}

	/// <summary>
	/// Every notification is the same prefab spawned with different message.
	/// To identify a specific notification, a unique ID is stored
	/// </summary>
	/// <param name="_uniqueID">dev defined ID as a string</param>
	public void SetUniqueID(string _uniqueID)
	{
		if (!string.IsNullOrEmpty(_uniqueID))
			ID = _uniqueID;
	}

	/// <summary>
	/// Set if Notification is dynamic and follows user head
	/// or if it is static
	/// </summary>
	/// <param name="_isDynamic">define if it is dynamic or not</param>
	public void SetDynamicAttibutes(bool _isDynamic)
	{
		isDynamic = _isDynamic;
	}

	/// <summary>
	/// </summary>
	/// <returns>The ID of the notification</returns>
	public string GetUniqueID()
	{
		return ID;
	}

	/// <summary>
	/// Set the notification message
	/// </summary>
	/// <param name="_message">message displayed as string</param>
	public void SetMessage(string _message)
	{
		// If called upon instantiation the component might not be applied
		// Thus save the message to be applied on Start
		notificationMessage = _message;

		Text txt = GetComponentInChildren<Text>();
		if (txt && !string.IsNullOrEmpty(_message))
			txt.text = notificationMessage;
	}

	/// <summary>
	/// Set life duration. Set 0 for infinite
	/// </summary>
	/// <param name="_lifeTime">UI duration</param>
	public void SetLifetime(float _lifeTime)
	{
		notificationLifetime = Mathf.Clamp(_lifeTime, 0f, 16f);
	}

	/// <summary>
	/// Destory by code the Notification. It will fade before deing destroyed
	/// </summary>
	/// <param name="_immediate">If set to true, it is destroyed w/o the fade</param>
	public void DestroyManually(bool _immediate = false)
	{
		if (_immediate)
		{
			Destroy(this.gameObject);
			return;
		}

		StopAllCoroutines();
		StartCoroutine("DestroyDelayedNotification");
	}
}
