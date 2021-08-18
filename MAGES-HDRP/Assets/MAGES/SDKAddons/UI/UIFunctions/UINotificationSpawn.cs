using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.UIManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using ovidVR.sceneGraphSpace;

public class UINotificationSpawn : MonoBehaviour {

    public NotificationUITypes UIType;

    public string NotificationUniqueID;

    // The destruction of the UI is delayed if the animations are enabled When the DestroyUI()
    // is bool bcomes true so the UImanagement won't call it's destruction again
    [HideInInspector]
    public bool isUIDestroyed = false;

    private float lifeTimeUI;
    private float deltaTimer;

    private Image lifeTimeImage;
    private Text notificationText;
    private Text closedUserUIText;

    private Animator anim;

    private bool allowCountdown = false;

    // For respawn purposes, save if the UI was spawned as dynamic or static
    internal bool isDynamicAtSpawn = true;

    [SerializeField]
    private bool UseTimer = true;

    void Update () {
        if (!allowCountdown)
            return;

        if (UseTimer)
        {
            deltaTimer += Time.deltaTime; 
        }

        lifeTimeImage.fillAmount = 1f - (deltaTimer / lifeTimeUI);

        if (deltaTimer >= lifeTimeUI)
        {
            allowCountdown = false;
            StartCoroutine("WaitForFadeAnim");
        }
    }

    IEnumerator WaitForFadeAnim()
    {
        anim.SetBool("FadeOut", true);

        // Animation lasts 3/4 of a sec + animator delay
        yield return new WaitForSeconds(1.2f);

        if(UIType == NotificationUITypes.UICriticalError)
        {
            if (Operation.Get.GetOperationDifficulty() == Difficulty.Easy)
                UIManagement.SpawnApplicationSpecificUI("UI_ProceedOperation", true);
            else if (Operation.Get.GetOperationDifficulty() == Difficulty.Medium || Operation.Get.GetOperationDifficulty() == Difficulty.Hard)
                UIManagement.SpawnUserUI(UserUITypes.UIOperationExit);
        }     

        Destroy(gameObject);
    }

    public void DestroyNotification()
    {
        isUIDestroyed = true;

        allowCountdown = false;
        lifeTimeImage.fillAmount = 0;

        StartCoroutine("WaitForFadeAnim");
    }

    public void InitializeNotification(string _message, string _closedUI, float _time, string _uniqueID)
    {
        if (string.IsNullOrEmpty(_uniqueID))
            NotificationUniqueID = _message;
        else
            NotificationUniqueID = _uniqueID;

        switch (gameObject.name)
        {
            case "UINotification":
                UIType = NotificationUITypes.UINotification;
                UIManagement.PlaySound(UISounds.Notification);
                break;
            case "UIWarning":
                UIType = NotificationUITypes.UIWarning;
                UIManagement.PlaySound(UISounds.Warning);
                break;
            case "UIError":
                UIType = NotificationUITypes.UIError;
                UIManagement.PlaySound(UISounds.Error);
                break;
            case "UICriticalError":
                UIType = NotificationUITypes.UICriticalError;
                UIManagement.PlaySound(UISounds.Error);
                break;
        }       

        lifeTimeImage = transform.Find("FadeTransform/LifeTime").GetComponent<Image>();
        lifeTimeImage.fillAmount = 1f;

        notificationText = transform.Find("FadeTransform/NotificationText").GetComponent<Text>();
        if(_message != null)
            notificationText.text = _message;

        // Not all Notifications are going to have this feature
        if(transform.Find("FadeTransform/ClosedUserUIText") != null)
        {
            closedUserUIText = transform.Find("FadeTransform/ClosedUserUIText").GetComponent<Text>();
            if (_closedUI != null)
                closedUserUIText.text = _closedUI.Remove(0, 2) + UIManagement.GetUIMessage(LanguageTranslator.FooterMessageAppearShortlyNotification);   //Remove first 2 letter of name: 'U''I'
        }

        lifeTimeUI = _time;
        if (lifeTimeUI == 0f)
            allowCountdown = false;
        else
            allowCountdown = true;

        deltaTimer = 0f;

        anim = GetComponent<Animator>();
    }

    public void SetNotificationText(string _text)
    {
        notificationText.text = _text;
    }
}
