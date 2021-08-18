//#define OLD_UI

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ovidVR.UIManagement;
using System.Linq;

public class UILanguageSwap : MonoBehaviour {

    [Header("Careful, it replaces text inside Text Component at runtime (if bool set to true)")]
    [SerializeField, Tooltip("Select the message for the Text Component to retrieve")]
    private LanguageTranslator messageKey;

    [SerializeField, Tooltip("Set to TRUE to replace text in Text Component, Else set to FALSE")]
    private bool allowTextReplace = true;

    void Start () {   
        Text thisText = GetComponent<Text>();

        if (thisText != null && allowTextReplace) 
        {
#if OLD_UI
            thisText.text = UIManagement.GetUIMessage(messageKey);
#else
            thisText.text = InterfaceManagement.Get.GetUIMessage(messageKey);
#endif
        }

        Destroy(this);
	}
}
