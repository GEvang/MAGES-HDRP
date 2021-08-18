using ovidVR.GameController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplainControlsUI : MonoBehaviour {

	IEnumerator Start () {

		// have Controller Class have the time to set up the controllers
		yield return null;
		yield return null;

		bool isOculus = true;

		if (GameObject.Find("WaveVR"))
			isOculus = false;
		else if (OvidVRControllerClass.Get.controllerType == OvidVRControllerClass.ControllerTypes.HTCViveController)
			isOculus = false;
		else if (OvidVRControllerClass.Get.controllerType == OvidVRControllerClass.ControllerTypes.WindowsMixedRealityController)
			isOculus = false;

		// First child is Background
		for (int i = 1; i < transform.childCount; ++i)
		{
			transform.GetChild(i).Find("OculusType").gameObject.SetActive(isOculus);
			transform.GetChild(i).Find("ViveType").gameObject.SetActive(!isOculus);
		}
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}
}
