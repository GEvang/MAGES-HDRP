using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginDropdownHandler : MonoBehaviour {
    private bool isOpen = false;

    public void ReplaceWithChosen()
    {
        GameObject.Find("CoolLicenseFiller/Username").GetComponent<InputField>().text = gameObject.transform.GetChild(2).GetComponent<Text>().text;
        gameObject.transform.GetChild(1).gameObject.SetActive(true);
        GameObject.Find("Dropdown").GetComponent<Dropdown>().Hide();
        isOpen = false;
        GameObject.Find("UILicenseRequest").transform.GetChild(3).GetChild(3).gameObject.SetActive(true);
        GameObject.Find("LoginButton").GetComponent<BoxCollider>().enabled = true;
        GameObject.Find("Dropdown").transform.GetChild(0).gameObject.GetComponent<Text>().text = gameObject.transform.GetChild(2).GetComponent<Text>().text;
    }

    private void FixListSortingLayer()
    {
        GameObject.Find("Dropdown List").GetComponent<Canvas>().sortingOrder = 1;
        GameObject.Find("Dropdown List").SetActive(false);
        GameObject.Find("Dropdown").transform.GetChild(3).gameObject.SetActive(true);
    }

    public void ResetCheckmarks()
    {
        GameObject DList = GameObject.Find("Dropdown List/Viewport/Content");
        foreach (Transform child in DList.transform)
        {
            child.GetChild(1).gameObject.SetActive(false);
        }
    }

    public void ShowList()
    {
        Dropdown DropdownList = GameObject.Find("Dropdown").GetComponent<Dropdown>();
        if (!isOpen)
        {
            if (DropdownList.options.Count != 0)
            {
                DropdownList.Show();
                FixListSortingLayer();
                GameObject.Find("Password").SetActive(false);
                GameObject.Find("LoginButton").GetComponent<BoxCollider>().enabled = false;
            }
            isOpen = true;
        }
        else
        {
            DropdownList.Hide();
            isOpen = false;
            GameObject.Find("UILicenseRequest").transform.GetChild(3).GetChild(3).gameObject.SetActive(true);
            GameObject.Find("LoginButton").GetComponent<BoxCollider>().enabled = true;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!gameObject.name.Equals("RaycastRightHand") && (other.gameObject.layer == 8))
        {
            ColorBlock cb = gameObject.GetComponent<Toggle>().colors;
            cb.normalColor = new Color(0f, 0.443f, 0.737f);
            gameObject.GetComponent<Toggle>().colors = cb;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!gameObject.name.Equals("RaycastRightHand") && (other.gameObject.layer == 8))
        {
            ColorBlock cb = gameObject.GetComponent<Toggle>().colors;
            cb.normalColor = Color.white;
            gameObject.GetComponent<Toggle>().colors = cb;
        }
    }
}
