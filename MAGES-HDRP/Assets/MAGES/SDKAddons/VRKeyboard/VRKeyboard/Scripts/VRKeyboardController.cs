using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * MAY 2020 - BEWARE COLORS ARE CHANGED FOR DARK THEMED
 **/
public class VRKeyboardController : MonoBehaviour {

    [HideInInspector]
    public GameObject Field;

    public GameObject Username;
    public GameObject Password;
    public GameObject SearchField;

    public Sprite KeyboardButton;
    public Sprite KeyboardButtonPressed;

    private AudioClip Click;

    public void Start()
    {
        Click = Resources.Load("MAGESres/UI/InterfaceMaterial/Sounds/Press") as AudioClip;
    }


    public void IniatializeField(GameObject field)
    {
        InputField f = field.GetComponent<InputField>();
        ColorBlock cb = f.colors;

        // Set this field pressed
        cb.normalColor = new Color32(112, 112, 112, 255);
        f.colors = cb;
    }
    
    public void SetField(GameObject field)
    {
        Field = field;
        InputField f = Field.GetComponent<InputField>();
        ColorBlock cb = f.colors;

        // Set this field pressed
        cb.normalColor = new Color32(200, 200, 200, 255);
        f.colors = cb;

        // Set the other field not pressed
        if (Field.name == "Username")
        {
            if (field.transform.Find("Border") != null)
            {
                Color fullBorder = new Color(1, 1, 1);
                field.transform.Find("Border").GetComponent<SpriteRenderer>().color = fullBorder;

                if (Password.transform.Find("Border") != null)
                {
                    fullBorder.a = 0.0f;
                    Password.transform.Find("Border").GetComponent<SpriteRenderer>().color = fullBorder;
                }
            }
            f = Password.GetComponent<InputField>();
        }
        else if (Field.name == "Password")
        {
            if (field.transform.Find("Border") != null)
            {
                Color fullBorder = new Color(1, 1, 1);
                field.transform.Find("Border").GetComponent<SpriteRenderer>().color = fullBorder;

                if (Username.transform.Find("Border") != null)
                {
                    fullBorder.a = 0.0f;
                    Username.transform.Find("Border").GetComponent<SpriteRenderer>().color = fullBorder;
                }
            }
            f = Username.GetComponent<InputField>();
        }
        else if (Field.name == "Search")
            f = SearchField.GetComponent<InputField>();

        if (Field.name != "Search")
        {
            cb = f.colors;
            cb.normalColor = new Color32(112, 112, 112, 255);
            f.colors = cb;
        }
    }

    public void VRKeyPressed(GameObject Text)
    {
        string currentText;

        if (Field != null)
        {
            currentText = Field.GetComponent<InputField>().text;
            currentText = currentText + Text.GetComponent<Text>().text;

            Field.GetComponent<InputField>().text = currentText;

            if (GameObject.Find("AvailableSessions(Clone)"))
            {
                StartCoroutine(StartSessionsRefresh());
            }
        }
    }

    public void CapsLock()
    {
        if (GameObject.Find("K1R4").transform.GetChild(0).GetComponent<Text>().text != "@")
        {
            GameObject KR2, KR3, KR4;

            KR2 = GameObject.Find("KR2");
            KR3 = GameObject.Find("KR3");
            KR4 = GameObject.Find("KR4");

            int i = 0;
            if (GameObject.Find("K1R2").transform.GetChild(0).GetComponent<Text>().text == "q")
            {
                foreach (Transform child in KR2.transform)
                {
                    child.transform.GetChild(0).GetComponent<Text>().text = child.transform.GetChild(0).GetComponent<Text>().text.ToUpper();
                }

                foreach (Transform child in KR3.transform)
                {
                    if (i != 0)
                        child.transform.GetChild(0).GetComponent<Text>().text = child.transform.GetChild(0).GetComponent<Text>().text.ToUpper();
                    i++;
                }

                i = 0;

                foreach (Transform child in KR4.transform)
                {
                    if ((i != 0) && (i != 8))
                        child.transform.GetChild(0).GetComponent<Text>().text = child.transform.GetChild(0).GetComponent<Text>().text.ToUpper();
                    i++;
                }
            }
            else
            {
                foreach (Transform child in KR2.transform)
                {
                    child.transform.GetChild(0).GetComponent<Text>().text = child.transform.GetChild(0).GetComponent<Text>().text.ToLower();
                }

                foreach (Transform child in KR3.transform)
                {
                    if (i != 0)
                        child.transform.GetChild(0).GetComponent<Text>().text = child.transform.GetChild(0).GetComponent<Text>().text.ToLower();
                    i++;
                }

                i = 0;

                foreach (Transform child in KR4.transform)
                {
                    if ((i != 0) && (i != 8))
                        child.transform.GetChild(0).GetComponent<Text>().text = child.transform.GetChild(0).GetComponent<Text>().text.ToLower();
                    i++;
                }
            }
        }
        else
        {
            VRKeyPressed(GameObject.Find("K1R4").transform.GetChild(0).gameObject);
        }
    }

    public void Symbols()
    {
        if (GameObject.Find("K1R2").transform.GetChild(0).GetComponent<Text>().text == "q" || GameObject.Find("K1R2").transform.GetChild(0).GetComponent<Text>().text == "Q")
        {

            GameObject.Find("K1R2").transform.GetChild(0).GetComponent<Text>().text = "!";
            GameObject.Find("K2R2").transform.GetChild(0).GetComponent<Text>().text = "~";
            GameObject.Find("K3R2").transform.GetChild(0).GetComponent<Text>().text = "#";
            GameObject.Find("K4R2").transform.GetChild(0).GetComponent<Text>().text = "$";
            GameObject.Find("K5R2").transform.GetChild(0).GetComponent<Text>().text = "%";
            GameObject.Find("K6R2").transform.GetChild(0).GetComponent<Text>().text = "^";
            GameObject.Find("K7R2").transform.GetChild(0).GetComponent<Text>().text = "&";
            GameObject.Find("K8R2").transform.GetChild(0).GetComponent<Text>().text = "*";
            GameObject.Find("K9R2").transform.GetChild(0).GetComponent<Text>().text = "(";
            GameObject.Find("K10R2").transform.GetChild(0).GetComponent<Text>().text = ")";

            GameObject.Find("K1R3").transform.GetChild(0).GetComponent<Text>().text = "abc";
            GameObject.Find("K2R3").transform.GetChild(0).GetComponent<Text>().text = "-";
            GameObject.Find("K3R3").transform.GetChild(0).GetComponent<Text>().text = "_";
            GameObject.Find("K4R3").transform.GetChild(0).GetComponent<Text>().text = "+";
            GameObject.Find("K5R3").transform.GetChild(0).GetComponent<Text>().text = "=";
            GameObject.Find("K6R3").transform.GetChild(0).GetComponent<Text>().text = "\\";
            GameObject.Find("K7R3").transform.GetChild(0).GetComponent<Text>().text = ";";
            GameObject.Find("K8R3").transform.GetChild(0).GetComponent<Text>().text = ":";
            GameObject.Find("K9R3").transform.GetChild(0).GetComponent<Text>().text = "'";
            GameObject.Find("K10R3").transform.GetChild(0).GetComponent<Text>().text = "\"";

            GameObject.Find("K1R4").transform.GetChild(0).GetComponent<Text>().text = "@";
            GameObject.Find("K2R4").transform.GetChild(0).GetComponent<Text>().text = "{";
            GameObject.Find("K3R4").transform.GetChild(0).GetComponent<Text>().text = "}";
            GameObject.Find("K4R4").transform.GetChild(0).GetComponent<Text>().text = "<";
            GameObject.Find("K5R4").transform.GetChild(0).GetComponent<Text>().text = ">";
            GameObject.Find("K6R4").transform.GetChild(0).GetComponent<Text>().text = ",";
            GameObject.Find("K7R4").transform.GetChild(0).GetComponent<Text>().text = "/";
            GameObject.Find("K8R4").transform.GetChild(0).GetComponent<Text>().text = "?";
        }
        else
        {
            GameObject.Find("K1R2").transform.GetChild(0).GetComponent<Text>().text = "q";
            GameObject.Find("K2R2").transform.GetChild(0).GetComponent<Text>().text = "w";
            GameObject.Find("K3R2").transform.GetChild(0).GetComponent<Text>().text = "e";
            GameObject.Find("K4R2").transform.GetChild(0).GetComponent<Text>().text = "r";
            GameObject.Find("K5R2").transform.GetChild(0).GetComponent<Text>().text = "t";
            GameObject.Find("K6R2").transform.GetChild(0).GetComponent<Text>().text = "y";
            GameObject.Find("K7R2").transform.GetChild(0).GetComponent<Text>().text = "u";
            GameObject.Find("K8R2").transform.GetChild(0).GetComponent<Text>().text = "i";
            GameObject.Find("K9R2").transform.GetChild(0).GetComponent<Text>().text = "o";
            GameObject.Find("K10R2").transform.GetChild(0).GetComponent<Text>().text = "p";

            GameObject.Find("K1R3").transform.GetChild(0).GetComponent<Text>().text = "@#%";
            GameObject.Find("K2R3").transform.GetChild(0).GetComponent<Text>().text = "a";
            GameObject.Find("K3R3").transform.GetChild(0).GetComponent<Text>().text = "s";
            GameObject.Find("K4R3").transform.GetChild(0).GetComponent<Text>().text = "d";
            GameObject.Find("K5R3").transform.GetChild(0).GetComponent<Text>().text = "f";
            GameObject.Find("K6R3").transform.GetChild(0).GetComponent<Text>().text = "g";
            GameObject.Find("K7R3").transform.GetChild(0).GetComponent<Text>().text = "h";
            GameObject.Find("K8R3").transform.GetChild(0).GetComponent<Text>().text = "j";
            GameObject.Find("K9R3").transform.GetChild(0).GetComponent<Text>().text = "k";
            GameObject.Find("K10R3").transform.GetChild(0).GetComponent<Text>().text = "l";

            GameObject.Find("K1R4").transform.GetChild(0).GetComponent<Text>().text = "CAPS LOCK";
            GameObject.Find("K2R4").transform.GetChild(0).GetComponent<Text>().text = "z";
            GameObject.Find("K3R4").transform.GetChild(0).GetComponent<Text>().text = "x";
            GameObject.Find("K4R4").transform.GetChild(0).GetComponent<Text>().text = "c";
            GameObject.Find("K5R4").transform.GetChild(0).GetComponent<Text>().text = "v";
            GameObject.Find("K6R4").transform.GetChild(0).GetComponent<Text>().text = "b";
            GameObject.Find("K7R4").transform.GetChild(0).GetComponent<Text>().text = "n";
            GameObject.Find("K8R4").transform.GetChild(0).GetComponent<Text>().text = "m";
        }

    }

    public void backSpace()
    {
        if (Field.GetComponent<InputField>().text != "")
        {
            string value = Field.GetComponent<InputField>().text;
            value = value.Substring(0, value.Length - 1);

            Field.GetComponent<InputField>().text = value;

            if (GameObject.Find("AvailableSessions(Clone)"))
            {
                StartCoroutine(StartSessionsRefresh());
            }
        }
    }

    public void Enter()
    {
        if(Field.name == "Username")
        {
            SetField(Password);
        }
        else if(Field.name == "Search")
        {
            GameObject.Find("VRKeyboardFull").SetActive(false);
            ColorBlock cb;
            cb = Field.GetComponent<InputField>().colors;
            cb.normalColor = new Color32(112, 112, 112, 255);
            Field.GetComponent<InputField>().colors = cb;
        }
        else
        {
            GameObject.Find("UILicenseRequest").GetComponent<LicenseRequest>().Login();
            GameObject.Find("VRKeyboardFull").SetActive(false);
        }
    }

    public void Tab()
    {
        if (Field.name == "Search")
            return;

        if (Field == null)
        {
            SetField(Username);
        }
        else
        {
            if (Field.name == "Username")
            {
                SetField(Password);
            }
            else
            {
                SetField(Username);
            }
        }
    }

    public void KeyPressed(GameObject Key)
    {
        Key.transform.parent.GetComponent<Image>().sprite = KeyboardButtonPressed;
        gameObject.GetComponent<AudioSource>().PlayOneShot(Click);
    }

    public void KeyReleased(GameObject Key)
    {
        Key.transform.parent.GetComponent<Image>().sprite = KeyboardButton;
    }

    private IEnumerator StartSessionsRefresh()
    {
        yield return new WaitForSeconds(1);
        if (!GameObject.Find("AvailableSessions(Clone)").GetComponent<UIFunctionsCoop>().RefreshNow)
        {
            GameObject.Find("AvailableSessions(Clone)").GetComponent<UIFunctionsCoop>().RefreshNow = true;
            GameObject.Find("AvailableSessions(Clone)").GetComponent<UIFunctionsCoop>().UpdateServerMatches(GameObject.Find("AvailableSessions(Clone)"));
        }
    }
}
