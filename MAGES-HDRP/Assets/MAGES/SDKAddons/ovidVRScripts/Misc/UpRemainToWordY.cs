using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpRemainToWordY : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		#if FINAL_IK
        transform.up = Vector3.up;
		#endif
	}
}
