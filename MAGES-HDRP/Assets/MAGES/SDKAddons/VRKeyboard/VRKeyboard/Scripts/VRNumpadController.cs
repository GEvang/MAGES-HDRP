using License;
using License.Models;
using ovidVR.UIManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRNumpadController : MonoBehaviour
{
    private GameObject _header;
    private GameObject _currentDigit;
    private ButtonBehavior _verifyButton;
    private bool[] _digitsFull;
    private bool _verifyActive = false;

    public Sprite KeyboardButton;
    public Sprite KeyboardButtonPressed;

    // Start is called before the first frame update
    void Start()
    {
        _currentDigit = GameObject.Find("Digit1");
        _digitsFull = new bool[4];
        _verifyButton = GameObject.Find("4DigitVerify").GetComponent<ButtonBehavior>();
        _header = GameObject.Find("4DigitUI(Clone)/Header/Title");
    }

    // Update is called once per frame
    void Update()
    {
        if (_digitsFull[0] && _digitsFull[1] && _digitsFull[2] && _digitsFull[3] && !_verifyActive)
        {
            _verifyButton.ButtonActivation(true);
            _verifyActive = true;
        }
    }

    public void SetDigit(GameObject DigitObject)
    {
        _currentDigit = DigitObject;

        SetColors();
    }

    public void WriteDigit(GameObject Number)
    {
        _currentDigit.transform.Find("Placeholder").GetComponent<Text>().text = Number.GetComponent<Text>().text;

        if (_currentDigit.name.Equals("Digit1"))
        {
            _digitsFull[0] = true;
            SetDigit(GameObject.Find("Digit2"));
        }
        else if (_currentDigit.name.Equals("Digit2"))
        {
            _digitsFull[1] = true;
            SetDigit(GameObject.Find("Digit3"));
        }
        else if (_currentDigit.name.Equals("Digit3"))
        {
            _digitsFull[2] = true;
            SetDigit(GameObject.Find("Digit4"));
        }
        else
        {
            _digitsFull[3] = true;
        }

    }

    public void CheckCode()
    {
        string code = GameObject.Find("Digit1/Placeholder").GetComponent<Text>().text + GameObject.Find("Digit2/Placeholder").GetComponent<Text>().text + GameObject.Find("Digit3/Placeholder").GetComponent<Text>().text + GameObject.Find("Digit4/Placeholder").GetComponent<Text>().text;

        ClientConfiguration client = new ClientConfiguration()
        {
            ClientId = "",
            ClientSecret = "",
            AllowedScopes = ""
        };
        var identityUrl = "";
        AuthenticationHandler.Instance.CheckoutUser(client, identityUrl, code, "SDK", result =>
        {
            if (result == LoginStatus.Success)
            {
                Debug.Log("User authenticated");
                LicenseRequest.hasLic = true;
                GameObject.Find("UILicenseRequestSSO(Clone)").GetComponent<LicenseRequest>().cameraRig.enabled = true;
                Destroy(GameObject.Find("UILicenseRequestSSO(Clone)"));
                Destroy(GameObject.Find("4DigitUI(Clone)"));
            }
            else
            {
                Debug.Log("Error authentication");
                Debug.Log(result);
                _header.GetComponent<Text>().text = "The 4-Digit Code was incorrect.";
            }
        });
    }

    public void KeyPressed(GameObject Key)
    {
        Key.transform.parent.GetComponent<Image>().sprite = KeyboardButtonPressed;
    }

    public void KeyReleased(GameObject Key)
    {
        Key.transform.parent.GetComponent<Image>().sprite = KeyboardButton;
    }

    private void SetColors()
    {
        GameObject Digit1, Digit2, Digit3, Digit4;

        Digit1 = GameObject.Find("Digit1");
        Digit2 = GameObject.Find("Digit2");
        Digit3 = GameObject.Find("Digit3");
        Digit4 = GameObject.Find("Digit4");

        Digit1.transform.Find("Background/Foreground").GetComponent<Image>().color = new Color32(56, 56, 56, 255);
        Digit2.transform.Find("Background/Foreground").GetComponent<Image>().color = new Color32(56, 56, 56, 255);
        Digit3.transform.Find("Background/Foreground").GetComponent<Image>().color = new Color32(56, 56, 56, 255);
        Digit4.transform.Find("Background/Foreground").GetComponent<Image>().color = new Color32(56, 56, 56, 255);

        _currentDigit.transform.Find("Background/Foreground").GetComponent<Image>().color = new Color32(0, 113, 188, 255);
    }

}
