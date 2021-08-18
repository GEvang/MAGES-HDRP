using System.Collections;
using System.Collections.Generic;
using ovidVR.GameController;
using ovidVR.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class ExtraExpNotificationUI : MonoBehaviour {

	private Transform animatorTR, camTr, startSphere, endSphere, followTrPos;
	private Animator anim;

	private bool isDestroyed;

	private Text text;
	private List<Image> allImg;

	private LineRenderer lineRend;

	void Awake () {
		animatorTR = transform.Find("Animator");

        camTr = OvidVRControllerClass.Get.GetCameraHead().transform;

		startSphere = transform.Find("Animator/StartSphere");
		endSphere = transform.Find("EndSphere");
		anim = GetComponentInChildren<Animator>();

		isDestroyed = false;
		
		text = GetComponentInChildren<Text>();
		allImg = new List<Image>();
		allImg.AddRange(GetComponentsInChildren<Image>());

		lineRend = GetComponentInChildren<LineRenderer>();
	}

	void Update()
	{
		animatorTR.rotation = Quaternion.LookRotation(animatorTR.position - camTr.position);

		if (followTrPos)
			endSphere.position = followTrPos.position;

		lineRend.SetPosition(0, startSphere.position);
		lineRend.SetPosition(1, endSphere.position);
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}
	
	IEnumerator DestroyAnimation()
	{
		lineRend.enabled = false;

		float timer = 0f, duration = 0.24f;
		float startX = transform.localScale.x;
		float startY = transform.localScale.y;
		float startZ = transform.localScale.z;

		while (timer < duration)
		{
			transform.localScale = new Vector3(Mathf.SmoothStep(startX, 0f, timer/duration),
												Mathf.SmoothStep(startY, 0f, timer / duration),
												Mathf.SmoothStep(startZ, 0f, timer / duration));

			timer += Time.deltaTime;
			yield return null;
		}

		DestroyUtilities.RemoteDestroy(this.gameObject);
	}

	public void SetUpExtraExplanationNotification(string _text, Transform _endSpherePos, bool _followConstantly, float _scaleMul)
	{
		if (!string.IsNullOrEmpty(_text))
			text.text = _text;

		if (_endSpherePos)
			endSphere.position = _endSpherePos.position;

		lineRend.SetPosition(0, startSphere.position);
		lineRend.SetPosition(1, endSphere.position);

		if (_followConstantly)
			followTrPos = _endSpherePos;

		_scaleMul = Mathf.Clamp(_scaleMul, 0f,6f);

		animatorTR.localScale *= _scaleMul;
	}

	public void DestroyThis()
	{
		if (isDestroyed)
			return;

		isDestroyed = true;

		anim.StopPlayback();
		anim.enabled = false;

		StartCoroutine("DestroyAnimation");
	}
}
