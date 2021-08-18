using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginFieldsHighlight : MonoBehaviour
{
    AudioClip Hover;

    private void Start()
    {
        Hover = Resources.Load("MAGESres/UI/InterfaceMaterial/Sounds/Hover") as AudioClip;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(transform.Find("Border") != null)
        {
            if (GetComponent<AudioSource>() != null)
            {
                GetComponent<AudioSource>().PlayOneShot(Hover);
            }
            if (transform.Find("Border").GetComponent<SpriteRenderer>().color.a != 1.0f)
            {
                Color fullBorder = new Color(1, 1, 1);
                fullBorder.a = 0.5f;
                transform.Find("Border").GetComponent<SpriteRenderer>().color = fullBorder;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (transform.Find("Border") != null)
        {
            if (transform.Find("Border").GetComponent<SpriteRenderer>().color.a == 0.5f)
            {
                Color fullBorder = new Color(1, 1, 1);
                fullBorder.a = 0.0f;
                transform.Find("Border").GetComponent<SpriteRenderer>().color = fullBorder;
            }
        }
    }
}
