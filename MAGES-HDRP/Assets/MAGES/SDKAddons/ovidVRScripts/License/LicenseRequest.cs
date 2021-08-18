using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ovidVR.UIManagement;
using ovidVR.Utilities.Keyboard;
using ovidVR.Exit;
using UnityEngine.Networking;
using License.Models;
using License;
using ovidVR.Utilities;
using ovidVR.GameController;

public class LicenseRequest : MonoBehaviour
{
#if UNITY_EDITOR
    public static bool hasLic = true;
#else
    public static bool hasLic = false;
#endif
    public static string user, pass;

    private InputField userField, passwordField;
    private ButtonBehavior loginButton;
    private Text versionText, titleText;
    public StringBuilder region = new StringBuilder("Auto");

    private GameObject vrKeyboard, optionsPanel, loginUsernamePanel, loadingPanel, newVersionPanel;
    List<ButtonBehavior> allOptionsButtons;
    public UniversalCameraMovement cameraRig;

    private ApplicationLanguages selectedLang = ApplicationLanguages.ENG;

    private bool reEnterCredentials = false;
     
    private void OnEnable()
    {
#if !UNITY_EDITOR
        hasLic = !Configuration.Get.UserLogin;
#endif
    }
    private void Awake()
    {       
        vrKeyboard = transform.Find("VRKeyboardFull").gameObject;       
        newVersionPanel = transform.Find("NewVersion").gameObject;
        loadingPanel = transform.Find("Loading").gameObject;
        optionsPanel = transform.Find("Options").gameObject;
        allOptionsButtons = new List<ButtonBehavior>();
        allOptionsButtons.AddRange(optionsPanel.GetComponentsInChildren<ButtonBehavior>());
        loginUsernamePanel = transform.Find("LoginUsername").gameObject;

        userField = loginUsernamePanel.transform.Find("Username").GetComponent<InputField>();
        passwordField = loginUsernamePanel.transform.Find("Password").GetComponent<InputField>();
        loginButton = loginUsernamePanel.transform.Find("LoginButton").GetComponent<ButtonBehavior>();

        versionText = transform.Find("VersionText").GetComponent<Text>();
        titleText = transform.Find("NewVersion/DefaultText").GetComponent<Text>();

        GameObject cameraRigObject = OvidVRControllerClass.Get.GetCameraRig();

        if (cameraRigObject)
            cameraRig = cameraRigObject.GetComponentInChildren<UniversalCameraMovement>();
        else
            cameraRig = null;        

        if (cameraRig != null)
            cameraRig.enabled = false;

        Init();
    }

    private void OnDisable()
    {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        KeyboardController.SetKeyboardState(true);
#endif
        StopAllCoroutines();
    }

    public void Init(bool _reEnterCredentials = false)
    {
        /*#if UNITY_EDITOR
                this.gameObject.SetActive(false);

                return;
        #endif*/

        StartCoroutine(FadeMusic(true));

        if (_reEnterCredentials)
        {
            titleText.text = "Wrong Credentials"+ Environment.NewLine + "Please try again";
        }

        // In case credentials failed, loading & info must be re-disabled
        vrKeyboard.SetActive(false);

        loadingPanel.SetActive(false);
        loginUsernamePanel.SetActive(true);

        foreach (ButtonBehavior bb in allOptionsButtons)
            bb.ButtonInteractivity(false);
        optionsPanel?.GetComponentInChildren<Animator>().SetBool("OpenOptions", false);

        hasLic = false;
        user = "";
        pass = "";
        userField.text = "";
        passwordField.text = "";

        reEnterCredentials = _reEnterCredentials;
        //Login();

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        KeyboardController.SetKeyboardState(false);
#endif   
    }

    private void Start()
    {
        InterfaceManagement.Get.InterfaceRaycastActivation(true);
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(userField.text) || !string.IsNullOrEmpty(passwordField.text))
        {
            if (!loginButton.isActive)
                loginButton.ButtonActivation(true);

            if (reEnterCredentials)
            {
                reEnterCredentials = false;
                userField.text = "";
            }
        }
        else
        {
            if (loginButton.isActive)
                loginButton.ButtonActivation(false);
        }
    }

    // Applied to Button as Trigger Event
    public void Login()
    {

        if ((!string.IsNullOrEmpty(userField.text) && !string.IsNullOrEmpty(passwordField.text)))
        {
            loginButton.ButtonActivation(false);
            StartCoroutine(FadeMusic(false));
            StartCoroutine(DelayLogin());
        }

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        KeyboardController.SetKeyboardState(true);
#endif
        user = userField.text;
        pass = passwordField.text;

#if !UNITY_EDITOR           
        // Login Flow and Checkout User License
        ClientConfiguration client = new ClientConfiguration()
        {
            ClientId = "",
            ClientSecret = "",
        };
        var identityUrl = "";
        AuthenticationHandler.Instance.LoginUserWithoutSSO(client, identityUrl, user, pass, "SDK", result =>
        {
            if (result == LoginStatus.Success)
            {
                Debug.Log("User authenticated");
                hasLic = true;
                if(cameraRig)
                    cameraRig.enabled = true;
                Destroy(this.gameObject);
            }
            else
            {
                Debug.Log("Error authentication");
                Debug.Log(result);
                Init(true);
            }
        });
#endif
        InterfaceManagement.Get.applicationLanguage = selectedLang;    
    }

    private IEnumerator DelayLogin()
    {
        loginUsernamePanel.SetActive(false);
        optionsPanel.SetActive(false);

        loadingPanel.SetActive(true);

        yield return new WaitForSeconds(1.4f);

        // Fade -> Block vision to avoid tremble
        //InterfaceManagement.Get.CameraFade(true, true);

        yield return null;
        yield return null;

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        KeyboardController.SetKeyboardState(true);
#endif

        user = userField.text;
        pass = passwordField.text;

        InterfaceManagement.Get.applicationLanguage = selectedLang;
    }

    private IEnumerator FadeMusic(bool _play)
    {
        AudioSource audio = GetComponent<AudioSource>();

        float start, end, timer = 0f;
        if (_play)
            start = 0f;
        else
            start = 1f;

        end = 1 - start;

        audio.volume = start;

        if (_play)
            audio.Play();

        while (timer < 0.8f)
        {
            audio.volume = Mathf.Lerp(start, end, timer / 0.8f);

            timer += Time.deltaTime;
            yield return null;
        }

        if (!_play)
            audio.Stop();
    }

    // Called from LoadLSA in sceneGraph (Enumerator => until)
    public static bool HasLicense()
    {
        return hasLic;
    }

    public void CompareVersions(string version)
    {
        if (string.IsNullOrEmpty(version))
            return;

        if (versionText) { versionText.text = "Version: " + version; }

        StartCoroutine(CheckForNewVersion(version));
    }

    private IEnumerator CheckForNewVersion(string currentVersion)
    {
        string uri = "";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            var operation = webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("Error while checking product version.");
            }
            yield return new WaitUntil(() => operation.isDone);

            if (!currentVersion.Equals(webRequest.downloadHandler.text))
                newVersionPanel?.GetComponent<Animator>().SetBool("ShowUpdate", true);
        }
    }

    public void SetMobileRegion(GameObject selectedRegion)
    {
        string newRegion = selectedRegion.name;

        if (newRegion.Equals("US") || newRegion.Equals("EU") || newRegion.Equals("ASIA") || newRegion.Equals("Auto"))
        {
            region = new StringBuilder(newRegion);
            NetworkControllerPhoton.Instance.Region = region.ToString();
        }
        List<ButtonBehavior> allRegionButtons = new List<ButtonBehavior>();
        allRegionButtons.AddRange(selectedRegion.transform.parent.GetComponentsInChildren<ButtonBehavior>());

        foreach (ButtonBehavior bb in allRegionButtons)
        {
            if (bb.gameObject.name != selectedRegion.name)
                bb.SetIsButtonToggled(false);
        }
    }

    public void SetLanguage(GameObject orderLanugage)
    {
        // TEMP
        if(orderLanugage.name == "ENG")
            selectedLang = ApplicationLanguages.ENG;
        else if (orderLanugage.name == "GER")
            selectedLang = ApplicationLanguages.GER;
        else if (orderLanugage.name == "CHN")
            selectedLang = ApplicationLanguages.CHN;

        List<ButtonBehavior> allRegionButtons = new List<ButtonBehavior>();
        allRegionButtons.AddRange(orderLanugage.transform.parent.GetComponentsInChildren<ButtonBehavior>());

        foreach (ButtonBehavior bb in allRegionButtons)
        {
            if (bb.gameObject.name != orderLanugage.name)
                bb.SetIsButtonToggled(false);
        }
    }

    // If Keyboard has Future Animation Add Code Here
    public void SetKeyboard(bool _active)
    {
        vrKeyboard.SetActive(_active);
    }

    // For Options Button ONLY
    public void SetOptions(GameObject _optionsBtn)
    {

        if (_optionsBtn.GetComponent<ButtonBehavior>().GetIsButtonToggled())
        {
            foreach (ButtonBehavior bb in allOptionsButtons)
                bb.ButtonInteractivity(true);

            optionsPanel?.GetComponentInChildren<Animator>().SetFloat("SpeedMul", 1f);
            optionsPanel?.GetComponentInChildren<Animator>().SetBool("OpenOptions", true);
        }
        else
        {
            foreach (ButtonBehavior bb in allOptionsButtons)
                bb.ButtonInteractivity(false);

            optionsPanel?.GetComponentInChildren<Animator>().SetFloat("SpeedMul", 1f);
            optionsPanel?.GetComponentInChildren<Animator>().SetBool("OpenOptions", false);
        }
    }

    // For Input Field Only
    public void InputFieldDisableOptions(GameObject _optionsBtn)
    {
        _optionsBtn.GetComponent<ButtonBehavior>().SetIsButtonToggled(false);

        foreach (ButtonBehavior bb in allOptionsButtons)
            bb.ButtonInteractivity(false);

        optionsPanel?.GetComponentInChildren<Animator>().SetFloat("SpeedMul", 6f);
        optionsPanel?.GetComponentInChildren<Animator>().SetBool("OpenOptions", false);
    }
    public void LoginSSO()
    {
        // Login Flow and Checkout User License
        ClientConfiguration client = new ClientConfiguration()
        {
            ClientId = "",
            ClientSecret = "",
            AllowedScopes = ""
        };
        var identityUrl = "";
        var loopbackUrl = "";
        AuthenticationHandler.Instance.LoginUserBrowser(client, identityUrl, loopbackUrl, result =>
        {
        });
        Spawn4DigitUI();
    }

    public void Spawn4DigitUI()
    {
        loginUsernamePanel.SetActive(false);
        optionsPanel.SetActive(false);
        newVersionPanel.SetActive(false);

        if ((GameObject.Find("4DigitUI") == null) && (GameObject.Find("4DigitUI(Clone)") == null))
        {
            Configuration SceneManagement = GameObject.Find("SCENE_MANAGEMENT").GetComponent<Configuration>();
            if (SceneManagement.VerificationCodeUI == null)
                PrefabImporter.SpawnGenericPrefab("MAGESres/UI/License/4DigitUI");
            else
                Instantiate(SceneManagement.VerificationCodeUI);
        }
    }

    public void RegisterUser()
    {
        Application.OpenURL("https://login.oramavr.com/Account/Register");
#if UNITY_STANDALONE_WIN
        InterfaceManagement.Get.SpawnDynamicNotificationUI(NotificationUITypes.UINotification, "Please remove your headset and create a new account from the browser window. " , 3f);
#endif
    }
}
