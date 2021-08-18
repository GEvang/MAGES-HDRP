using System.Collections;
using System.Collections.Generic;
using ovidVR.UIManagement;
using ovidVR.Utilities;
using UnityEngine;
using UnityEngine.UI;


public class CustomizationActions : MonoBehaviour
{
    private string _selectionsPath;
    private string _optionsPath;
    private CustomizationManager _manager;

    private bool _inverse=false;
    public bool initialized;

    public void Start()
    {
        _selectionsPath = "MAGESres/AvatarCustomization/Selections/";
        _optionsPath = "MAGESres/AvatarCustomization/Options/";
        _manager = GetComponent<CustomizationManager>();
        initialized = true;
    }

    #region Actions

    #region Initialize
    public void InitializeStart()
    {
        _manager.ChangeDescription(InterfaceManagement.Get.GetUIMessage(LanguageTranslator.NewAvatarHeader));

        StartCoroutine(LatePerform());
    }

    public void InitializeGender()
    {
        _manager.ChangeDescription(InterfaceManagement.Get.GetUIMessage(LanguageTranslator.GenderAvatarHeader));

        var selections = Resources.LoadAll(_selectionsPath + "Gender", typeof(object));
        var globalCounter=0;
        foreach (var sel in selections)
        {
            var counter = globalCounter;
            var ui = Instantiate(sel, _manager._selectionsUI.transform) as GameObject;
            ui?.GetComponent<ButtonBehavior>().buttonFunction.AddListener(delegate { _manager.SelectOption(counter); });
            if (ui == null) continue;
            globalCounter = counter + 1;

            Vector2 uiPos = ui.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(ui.gameObject, true));
        }

        if (!_inverse)
        {
            var next = Resources.Load(_optionsPath + "Next", typeof(object));
            var nextUI = Instantiate(next, _manager._nextButtonUI.transform) as GameObject;
            nextUI?.GetComponent<ButtonBehavior>().buttonFunction.AddListener(delegate { _manager.NextState(); });
            if (nextUI == null) return;

            Vector2 uiPos = nextUI.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(nextUI.gameObject, true));

            var prev = Resources.Load(_optionsPath + "Prev", typeof(object));
            var prevUI = Instantiate(prev, _manager._prevButtonUI.transform) as GameObject;
            prevUI?.GetComponent<ButtonBehavior>().buttonFunction.AddListener(delegate { _manager.PrevState(); });
            prevUI?.GetComponent<ButtonBehavior>().ButtonActivation(false);
            if (prevUI == null) return;

            uiPos = prevUI.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(prevUI.gameObject, true));
        }

        _inverse = false;
    }

    public void InitializeSkin()
    {
        _manager.ChangeDescription(InterfaceManagement.Get.GetUIMessage(LanguageTranslator.SkinColorAvatarHeader));
        var selections = Resources.LoadAll(_selectionsPath + "Skin", typeof(object));
        var globalCounter = 0;
        foreach (var sel in selections)
        {
            var counter = globalCounter;
            var ui = Instantiate(sel, _manager._selectionsUI.transform) as GameObject;
            ui?.GetComponent<ButtonBehavior>().buttonFunction.AddListener(delegate { _manager.SelectOption(counter); });
            if (ui == null) continue;
            globalCounter = counter + 1;

            Vector2 uiPos = ui.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(ui.gameObject, true));
        }

        if (!_inverse)
        {
            Transform prevUI = transform.Find("LeftOption").GetChild(0);
            if (prevUI)
                prevUI?.GetComponent<ButtonBehavior>().ButtonActivation(true);
        }

        _inverse = false;

    }

    public void InitializeSuit()
    {
        _manager.ChangeDescription(InterfaceManagement.Get.GetUIMessage(LanguageTranslator.SuitColorAvatarHeader));
        var selections = Resources.LoadAll(_selectionsPath + "Suit", typeof(object));
        var globalCounter = 0;
        foreach (var sel in selections)
        {
            var counter = globalCounter;
            var ui = Instantiate(sel, _manager._selectionsUI.transform) as GameObject;
            ui?.GetComponent<ButtonBehavior>().buttonFunction.AddListener(delegate { _manager.SelectOption(counter); });
            if (ui == null) continue;
            globalCounter = counter + 1;

            Vector2 uiPos = ui.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(ui.gameObject, true));
        }
        _inverse = false;
    }

    public void InitializeFinal()
    {
        _manager.ChangeDescription(InterfaceManagement.Get.GetUIMessage(LanguageTranslator.AreYouSureButton));
        /*var option = Resources.Load(_optionsPath + "Restart", typeof(object));
        var optionUI = Instantiate(option, _manager._prevButtonUI.transform) as GameObject;
        optionUI?.GetComponent<ButtonBehavior>().buttonFunction.AddListener(delegate { _manager.Restart(); });
        if (optionUI != null)
        {
            Vector2 uiPos = optionUI.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(optionUI.gameObject, true));
        }*/

        var option = Resources.Load(_optionsPath + "Done", typeof(object));
        var optionUI = Instantiate(option, _manager._nextButtonUI.transform) as GameObject;
        optionUI?.GetComponent<ButtonBehavior>().buttonFunction.AddListener(delegate { _manager.NextState(); });
        if (optionUI != null)
        {
            Vector2 uiPos = optionUI.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(optionUI.gameObject, true));
        }
    }
    #endregion

    #region Performs
    public void PerformStart()
    {
    }

    public void PerformGender()
    {

        foreach (Transform child in _manager._selectionsUI.transform)
        {
            //StartCoroutine(SelectionSpawnDelete(child.gameObject, false, _inverse));

            Vector2 uiPos = child.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(child.gameObject, false));
        }
    }

    public void PerformSkin()
    {
        foreach (Transform child in _manager._selectionsUI.transform)
        {
            //StartCoroutine(SelectionSpawnDelete(child.gameObject, false, _inverse));

            Vector2 uiPos = child.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(child.gameObject, false));
        }
    }

    public void PerformSuit()
    {
        foreach (Transform child in _manager._selectionsUI.transform)
        {
            //StartCoroutine(SelectionSpawnDelete(child.gameObject, false, _inverse));

            Vector2 uiPos = child.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(child.gameObject, false));
        }

        var optionUI = _manager._nextButtonUI.transform.Find("Next(Clone)");
        if (optionUI != null)
        {
            Vector2 uiPos = optionUI.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(optionUI.gameObject, false));
        }

        /*optionUI = _manager._prevButtonUI.transform.Find("Prev(Clone)");
        if (optionUI != null)
        {
            Vector2 uiPos = optionUI.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(optionUI.gameObject, false));
        }*/
    }

    public void PerformFinal()
    {
        // Change Path
        Configuration SceneManagement = GameObject.Find("SCENE_MANAGEMENT").GetComponent<Configuration>();

        if (SceneManagement.OperationStartUI == null)
            PrefabImporter.SpawnActionPrefab("Lesson0/Stage0/OperationStart/OperationStart");
        else
            Instantiate(SceneManagement.OperationStartUI);

        AvatarManager.Instance.SetCustomizationData(_manager.currentCreation);
        
        _manager.InitializeAvatarReference();

        _manager.SaveFile();
        Destroy(GameObject.Find("DestroyThisObjectAtEnd"));
        Destroy(_manager._customizationUI);
        Destroy(gameObject);
    }

    #endregion

    #region Undos
    public void UndoStart() { }

    public void UndoGender() { }

    public void UndoSkin()
    {
        _inverse = true;
        foreach (Transform child in _manager._selectionsUI.transform)
        {
            Vector2 uiPos = child.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(child.gameObject, false));
        }

        var optionUI = _manager._prevButtonUI.transform.Find("Prev(Clone)");
        if (optionUI != null)       
            optionUI?.GetComponent<ButtonBehavior>().ButtonActivation(false);
    }

    public void UndoSuit()
    {
        _inverse = true;
        foreach (Transform child in _manager._selectionsUI.transform)
        {
            Vector2 uiPos = child.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(child.gameObject, false));
        }
    }

    public void UndoFinal()
    {
        _inverse = true;
        var option = Resources.Load(_optionsPath + "Next", typeof(object));
        var optionUI = Instantiate(option, _manager._nextButtonUI.transform) as GameObject;
        optionUI?.GetComponent<ButtonBehavior>().buttonFunction.AddListener(delegate { _manager.NextState(); });
        if (optionUI != null)
        {
            Vector2 uiPos = optionUI.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(optionUI.gameObject, true));
        }

        /*option = Resources.Load(_optionsPath + "Prev", typeof(object));
        optionUI = Instantiate(option, _manager._prevButtonUI.transform) as GameObject;
        if (optionUI != null)
        {
            Vector2 uiPos = optionUI.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(optionUI.gameObject, true));
        }*/

        var optionDel = _manager._nextButtonUI.transform.Find("Done(Clone)");
        if (optionDel != null)
        {
            Vector2 uiPos = optionUI.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(optionDel.gameObject, false));
        }

        /*optionDel = _manager._prevButtonUI.transform.Find("Restart(Clone)");
        if (optionDel != null)
        {
            Vector2 uiPos = optionUI.GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(SelectionSpawnDelete(optionDel.gameObject, false));
        }*/
    }

    #endregion

    #endregion

    #region HelperFunctions
    private IEnumerator LatePerform()
    {
        yield return new WaitForSeconds(2);
        _manager.NextState();
    }

    private IEnumerator SelectionSpawnDelete(GameObject ui, bool spawn)
    {
        if (!ui)
            yield break;

        ui?.GetComponent<ButtonBehavior>().ButtonInteractivity(false);

        List<Image> btnImages = new List<Image>();
        Text btnText;

        btnImages.AddRange(ui.GetComponentsInChildren<Image>());
        btnText = ui.GetComponentInChildren<Text>();

        float timer = 0f, duration = 0.5f;
        float startA, endA;

        if (spawn)
        {          
            foreach (Image i in btnImages)
                i.color = new Color(i.color.r, i.color.g, i.color.b, 0f);

            if(btnText)
                btnText.color = new Color(btnText.color.r, btnText.color.g, btnText.color.b, 0f);

            // if spawning wait for previous to be half disappeared
            yield return new WaitForSeconds(duration);
        }

        startA = btnImages[0].color.a;
        if (startA > 0)
            endA = 0f;
        else
            endA = 1f;

        while (timer < duration)
        {
            foreach (Image i in btnImages)
                i.color = new Color(i.color.r, i.color.g, i.color.b, Mathf.SmoothStep(startA, endA, timer / duration));

            if (btnText)
                btnText.color = new Color(btnText.color.r, btnText.color.g, btnText.color.b, Mathf.SmoothStep(startA, endA, timer / duration));

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (!spawn)
            Destroy(ui);
        else
            ui?.GetComponent<ButtonBehavior>().ButtonInteractivity(true);
    }
    
    

    #endregion
}
