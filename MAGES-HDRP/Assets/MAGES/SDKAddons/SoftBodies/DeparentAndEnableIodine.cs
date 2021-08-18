using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeparentAndEnableIodine : MonoBehaviour {

    private GameObject Iodine;
    IEnumerator Start ()
    {
        Iodine = transform.Find("Iodine").gameObject;
        Iodine.transform.parent = null;
        yield return new WaitForEndOfFrame();
        Iodine.SetActive(true);
    }
}
