using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ovidVR.UIManagement.ExplanationDisplay
{
    public class UIExtraExplanationManagement : MonoBehaviour
    {

        private Canvas screenCanvas;
        private Animation screenStartAnim;
        private Text screenMessageText;

        private List<Image> allScreenImageComponents;

        // true when all canvas components are fading out
        bool isAlphaChanged;
        float alphaTimer;

        private static UIExtraExplanationManagement uiExplanation;

        public static UIExtraExplanationManagement Get
        {
            get
            {
                if (!uiExplanation)
                {
                    uiExplanation = FindObjectOfType(typeof(UIExtraExplanationManagement)) as UIExtraExplanationManagement;

                    if (!uiExplanation)
                        Debug.LogError("Error, No UIExtraEplanationManagement was found");
                }

                return uiExplanation;
            }
        }

        private void Awake()
        {
            screenCanvas = GetComponent<Canvas>();
            screenStartAnim = GetComponent<Animation>();
            screenMessageText = GetComponentInChildren<Text>();

            allScreenImageComponents = new List<Image>();
            foreach (Image i in GetComponentsInChildren<Image>())
            {
                // Black background must not fade with rest
                if (i.gameObject.name != "BackGround")
                    allScreenImageComponents.Add(i);
            }

            isAlphaChanged = false;
            alphaTimer = 0f;
        }

        private void Update()
        {
            if (Get.isAlphaChanged)
            {
                if (Get.alphaTimer <= 0.6f)
                    Get.alphaTimer += Time.deltaTime * 3f;
                else
                    Get.alphaTimer += Time.deltaTime;

                if (Get.alphaTimer >= 1f)
                {
                    Get.alphaTimer = 0f;
                    Get.isAlphaChanged = false;
                    Get.SetAllComponentsAlpha(0f);
                    Get.screenCanvas.enabled = false;
                    return;
                }

                Get.SetAllComponentsAlpha(1f - Get.alphaTimer);
            }
        }

        private void SetAllComponentsAlpha(float _alpha)
        {
            Color componentColor;
            foreach (Image i in Get.allScreenImageComponents)
            {
                componentColor = i.color;
                componentColor.a = _alpha;
                i.color = componentColor;
            }

            componentColor = Get.screenMessageText.color;
            componentColor.a = _alpha;
            Get.screenMessageText.color = componentColor;
        }

        public static void DisplayExplanationMessage(string _text)
        {
            // Reset all components color to original
            Get.isAlphaChanged = false;

            Get.SetAllComponentsAlpha(1f);

            // If ExplanationScreen is already enabled
            if (Get.screenCanvas.enabled)
            {
                Get.screenMessageText.text = _text;
                return;
            }

            // Enable Canvas, add new message to text and play animation
            Get.screenCanvas.enabled = true;
            Get.screenMessageText.text = _text;
            Get.screenStartAnim.Play();
        }

        public static void DisplayExplanationMessage(LanguageTranslator _key)
        {
            // Reset all components color to original
            Get.isAlphaChanged = false;

            Get.SetAllComponentsAlpha(1f);

            // If ExplanationScreen is already enabled
            if (Get.screenCanvas.enabled)
            {
                Get.screenMessageText.text = UIManagement.GetUIMessage(_key);
                return;
            }

            // Enable Canvas, add new message to text and play animation
            Get.screenCanvas.enabled = true;
            Get.screenMessageText.text = UIManagement.GetUIMessage(_key);
            Get.screenStartAnim.Play();
        }

        public static void HideExplanationMessage(bool _hideImmediately = false)
        {
            if (_hideImmediately)
                Get.screenCanvas.enabled = false;
            else
                Get.isAlphaChanged = true;
        }
    }
}
