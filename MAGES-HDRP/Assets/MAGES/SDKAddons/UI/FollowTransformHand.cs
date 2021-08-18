using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransformHand : MonoBehaviour {

	private new Transform transform;
	private Transform followTransform;

	private Vector3 startOffset;

	private bool allowUpdate;

	// Use this for initialization
	void Awake () {
		transform = GetComponent<Transform>();

		allowUpdate = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!allowUpdate)
			return;

		transform.position = followTransform.position + startOffset;
	}

	public void SetHandTransform(Transform _transform, float _offsetY)
	{
		allowUpdate = true;

		followTransform = _transform;
		transform.position = _transform.position;
		transform.Translate(Vector3.up * _offsetY, Space.World);

		startOffset = transform.position - _transform.position;
	}
}
