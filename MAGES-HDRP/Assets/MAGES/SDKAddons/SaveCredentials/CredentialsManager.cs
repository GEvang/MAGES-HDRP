using Encryption;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CredentialsManager : MonoBehaviour {
    // Use this for initialization
    public GameObject Username;
    public GameObject Password;
    private bool _passwordVisible = false;
    private bool _canSwitchPassMode = true;

	void Start () {
        LoadCredentials();
        if (PlayerPrefs.GetInt("SaveCredentials") == 1)
        {
            SetToggle();
            ImageToggleUpdate();
        }
	}
	
	// Update is called once per frame
	void Update () {
   
	}

    public void SaveCredentials()
    {
        if (GetComponent<ButtonBehavior>() && GetComponent<ButtonBehavior>().GetIsButtonToggled())
        {
            string username = Username.GetComponent<InputField>().text;
            string password = Password.GetComponent<InputField>().text;

            username = StringEncryption.Encrypt(username);
            password = StringEncryption.Encrypt(password);

            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.SetString("Password", password);
            PlayerPrefs.SetInt("SaveCredentials", 1);
            PlayerPrefs.Save();
        }
        else
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }

    public void LoadCredentials()
    {
        Username.GetComponent<InputField>().text = StringEncryption.Decrypt(PlayerPrefs.GetString("Username", ""));
        Password.GetComponent<InputField>().text = StringEncryption.Decrypt(PlayerPrefs.GetString("Password", ""));
    }

    public void SetToggle()
    {
        if (GetComponent<ButtonBehavior>())
            GetComponent<ButtonBehavior>().ButtonPress();
    }

    public void ImageToggleUpdate()
    {
        if (GetComponent<ButtonBehavior>())
        {
            Sprite toggleSprite;

            if (GetComponent<ButtonBehavior>().GetIsButtonToggled())
                toggleSprite = Resources.Load("MAGESres/UI/InterfaceMaterial/Images/CheckBoxON", typeof(Sprite)) as Sprite;
            else
                toggleSprite = Resources.Load("MAGESres/UI/InterfaceMaterial/Images/CheckBoxOFF", typeof(Sprite)) as Sprite;

            if(toggleSprite)
                GetComponent<ButtonBehavior>().UpdateButtonSprite(toggleSprite);
        }
    }

    public void ShowHidePassword()
    {
        if (_canSwitchPassMode)
        {
            _passwordVisible = !_passwordVisible;

            Sprite toggleSprite;

            if (!_passwordVisible)
            {
                toggleSprite = Resources.Load("MAGESres/UI/InterfaceMaterial/Images/EyeClosed", typeof(Sprite)) as Sprite;
                Password.GetComponent<InputField>().inputType = InputField.InputType.Password;
            }
            else
            {
                toggleSprite = Resources.Load("MAGESres/UI/InterfaceMaterial/Images/EyeOpened", typeof(Sprite)) as Sprite;
                Password.GetComponent<InputField>().inputType = InputField.InputType.Standard;
            }

            if (toggleSprite)
                GameObject.Find("ShowPassword").GetComponent<ButtonBehavior>().UpdateButtonSprite(toggleSprite);

            string tmpInput = Password.GetComponent<InputField>().text;
            Password.GetComponent<InputField>().text = "";
            Password.GetComponent<InputField>().text = tmpInput;

            _canSwitchPassMode = false;
            StartCoroutine(tempDisablePassViewChange());
        }
    }

    private IEnumerator tempDisablePassViewChange()
    {
        yield return new WaitForSeconds(1.5f);
        _canSwitchPassMode = true;
    }
}
