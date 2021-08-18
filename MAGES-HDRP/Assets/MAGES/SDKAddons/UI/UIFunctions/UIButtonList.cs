using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.CustomEventManager;
using ovidVR.sceneGraphSpace;
using UnityEngine.UI;
using ovidVR.UIManagement;

public class UIButtonList : MonoBehaviour
{
    public enum AffectedByLicense { None, Demo, Free };

    [SerializeField, Range(0.02f,1f), Tooltip("Set Timer between each buttons fade-in animation")]
    private float buttonSpawnIntervals = 0.2f;

    [SerializeField, Range(0.4f, 6f), Tooltip("Set button's total fade-in animation timer")]
    private float buttonAnimationTimer = 1.4f;

    /// <summary>
    /// Store everything needed for each button that this parent script has to change
    /// </summary>
    class ButtonFeatures
    {
        public Transform bTransform;
        public UIButton bScript;
        public string bName;
        public Image bImageMask;
        public Animator bAnimator;

        // Lerp-Animation only values
        public bool bIsAnimRunning;

        internal Color alpha;
        private float currTimer = 0f, timerSpeedMul = 0f;

        private Vector3 bDefaultPos;
        private Color bDefaultColor;

        private Vector3 startPos, endPos;
        private Color startColor, endColor;

        public ButtonFeatures(Transform _bTransform, int _license, bool _allowAnim, bool _allowSound)
        {
            bTransform = _bTransform;
            bScript = _bTransform.GetComponent<UIButton>();          
            bScript.allowButtonAnimation = _allowAnim;
            //bScript.allowButtonSound = _allowSound;

            bDefaultPos = _bTransform.localPosition;
            bName = _bTransform.name;
            bImageMask = _bTransform.Find("ButtonImageMask").GetComponent<Image>();
            bDefaultColor = bImageMask.color;
            bAnimator = _bTransform.Find("ButtonImageMask").GetComponent<Animator>();
            bAnimator.Rebind();

            bIsAnimRunning = false;

            alpha = bImageMask.color;
            alpha.a = 1f;
            currTimer = 0f;

            bScript.InitializeButton();
        }

        public void SetupButtonForLerp(Vector3 _startPos, Vector3 _endPos, Color _startColor, Color _endColor, float _timerSpeed)
        {
            startPos = _startPos;
            endPos = _endPos;
            startColor = _startColor;
            endColor = _endColor;

            bTransform.localPosition = startPos;
            bImageMask.color = startColor;

            timerSpeedMul = _timerSpeed;

            bIsAnimRunning = true;
            currTimer = 0f;

            bAnimator.enabled = false;
        }

        public void ButtonLerpPositionColor()
        {
            if (!bIsAnimRunning)
                return;

            bTransform.localPosition = Vector3.Lerp(startPos, endPos, currTimer);
            bImageMask.color = Color.Lerp(startColor, endColor, currTimer);

            currTimer += Time.deltaTime * timerSpeedMul;

            if (currTimer >= 1f)
            {
                bTransform.localPosition = bDefaultPos;
                bImageMask.color = endColor;

                bIsAnimRunning = false;
            }
        }

        public void ResetButton()
        {
            bTransform.localPosition = bDefaultPos;
            bImageMask.color = bDefaultColor;

            bIsAnimRunning = false;

            currTimer = 0f;

            bAnimator.enabled = true;        
        }
    }

    private List<ButtonFeatures> allButtonsFeatures;

    struct ButtonExplanation
    {
        public GameObject explanationParent;
        public Text explanationText;
        public Image explanationImage;
        public Animator explanationAnim;
    }

    private ButtonExplanation buttonExp;

    private int currButtonsHovering = 0;

    [HideInInspector]
    public License.LicenseType licenseType = License.LicenseType.None;

    // Initialization --------------------------------------

    private void OnEnable()
    {
        licenseType = sceneGraph.GetLicenseType();

        bool allowAnimation = true;
        bool allowSound = false;

        // All Spawn will become as one
        UISpawn parentScript = null;
        if (transform.parent != null && transform.parent.GetComponent<UISpawn>() != null)
            parentScript = transform.parent.GetComponent<UISpawn>();

        if (parentScript != null)
        {
            allowAnimation = parentScript.allowUIAnimations;
            allowSound = parentScript.allowUISound;
        }

        // Add all childern buttons to the list and initialize them
        if (allButtonsFeatures == null)
        {
            allButtonsFeatures = new List<ButtonFeatures>();

            foreach (Transform t in transform)//GetComponentInChildren<Transform>())
            {
                if(t.gameObject.activeSelf)
                    allButtonsFeatures.Add(new ButtonFeatures(t, (int)licenseType, allowAnimation, allowSound));
            }
        }
        else
        {
            foreach (ButtonFeatures bt in allButtonsFeatures)
                bt.ResetButton();
        }

        // If animation is allowed start buttons fade-in animation
        if (parentScript != null && allowAnimation)
            StartCoroutine(ButtonFadeAnimation(true));   

        // Initialize Text Mask (button explannation). If non-existent, since it's optional, just return
        if (transform.parent.Find("TextMask") == null)
            return;

        buttonExp = new ButtonExplanation();
        buttonExp.explanationParent = transform.parent.Find("TextMask").gameObject;
        buttonExp.explanationText = buttonExp.explanationParent.GetComponentInChildren<Text>();
        buttonExp.explanationImage = buttonExp.explanationParent.GetComponentInChildren<Image>();
        buttonExp.explanationAnim = buttonExp.explanationParent.GetComponentInChildren<Animator>();

        if (buttonExp.explanationParent.activeSelf)
            buttonExp.explanationParent.SetActive(false);
    }

    private void OnDisable()
    {
        currButtonsHovering = 0;

        StopAllCoroutines();
    }

    // Enumerators -----------------------------------------

    IEnumerator ButtonFadeAnimation(bool _fadeIn)
    {
        // wait for button press anim
        if(!_fadeIn)
            yield return new WaitForSeconds(.4f);

        int counter = allButtonsFeatures.Count - 1;
        int listLastIndex = allButtonsFeatures.Count - 1;
        float timer = 0f;

        Vector3 startPos, endPos;
        Color startColor, endColor;
        float lerpTimer, spawnInterval = 0;

        for (int i = 0; i < allButtonsFeatures.Count; ++i)
        {
            if (_fadeIn)
            {
                // set allowInvoke to false to stop buttons from being able to be pressed during fade in anim
                allButtonsFeatures[i].bScript.allowInvoke = false;

                startPos = allButtonsFeatures[i].bTransform.localPosition + new Vector3(4f, 0f, 0f);
                endPos = allButtonsFeatures[i].bTransform.localPosition;
                startColor = allButtonsFeatures[i].bImageMask.color * new Color(1f, 1f, 1f, 0f);
                endColor = allButtonsFeatures[i].bImageMask.color;
                lerpTimer = 1f / buttonAnimationTimer;
                spawnInterval = buttonSpawnIntervals;
            }
            else
            {
                allButtonsFeatures[i].bScript.SetButtonSecondaryExplanation(false);

                startPos = allButtonsFeatures[i].bTransform.localPosition;
                endPos = allButtonsFeatures[i].bTransform.localPosition + new Vector3(-4f, 0f, 0f);
                startColor = allButtonsFeatures[i].bImageMask.color;
                endColor = allButtonsFeatures[i].bImageMask.color * new Color(1f, 1f, 1f, 0f);
                lerpTimer = (1f / buttonAnimationTimer) * 4f;
                spawnInterval = buttonSpawnIntervals / 2f;
            }
          
            allButtonsFeatures[i].SetupButtonForLerp(startPos, endPos, startColor, endColor, lerpTimer);
        }

        // Animate buttons till last one stops animating. Order: Last->First
        while (allButtonsFeatures[0].bIsAnimRunning)
        {
            yield return null;

            for(int i = listLastIndex; i >= counter; --i)
                allButtonsFeatures[i].ButtonLerpPositionColor();

            // After a specific interval start spawning the next button
            if ((timer += Time.deltaTime) > spawnInterval && counter > 0)
            {
                timer = 0f;
                --counter;
            }
        }

        if (_fadeIn)
        {
            foreach (ButtonFeatures bt in allButtonsFeatures)
            {
                bt.bScript.InitializeAfterButtonAnimation();
                bt.ResetButton();
            }
        }
        
    }

    // Public Functions ------------------------------------

    internal void FadeOutButtons()
    {
        StopAllCoroutines();
        StartCoroutine(ButtonFadeAnimation(false));
    }

    /// <summary>
    /// Called ONLY from each button. When user's hand is hovering inside the button's collider,
    /// the button will call this function to trigger the eplanation text with it's respective message
    /// </summary>
    /// <param name="_buttonExp"></param>
    /// <param name="_active"></param>
    /// <param name="_isLicenseAffectedList"></param>
    public void SetExplanationText(LanguageTranslator _buttonExp, bool _active, List<AffectedByLicense> _isLicenseAffectedList)
    {
        if (buttonExp.explanationParent == null)
            return;

        string translatedExp = UIManagement.GetUIMessage(_buttonExp);

        // If no message is found warn developer
        if (string.IsNullOrEmpty(translatedExp))
        {
            Debug.LogError("Empty transsslated message in button");
            buttonExp.explanationText.text = " ";
            return;
        }

        if (_active)
            ++currButtonsHovering;
        else
            --currButtonsHovering;

        if (currButtonsHovering < 0)
            currButtonsHovering = 0;

        if(_active || currButtonsHovering == 0)
            buttonExp.explanationParent.SetActive(_active);

        if (_active)
        {
            if (!buttonExp.explanationAnim.GetCurrentAnimatorStateInfo(0).IsName("ExplanationText"))
                buttonExp.explanationAnim.Play("ExplanationText");

            // Update buttons explanation
            buttonExp.explanationText.text = translatedExp;

            // If license is matched, replace button explanation with the N/A function message
            foreach (AffectedByLicense abl in _isLicenseAffectedList)
            {
                switch (abl)
                {
                    case AffectedByLicense.Demo:
                        if (licenseType == License.LicenseType.Demo)
                            buttonExp.explanationText.text = UIManagement.GetUIMessage(LanguageTranslator.UIExpDemoFreeNA);
                        break;
                    case AffectedByLicense.Free:
                        if (licenseType == License.LicenseType.Free)
                            buttonExp.explanationText.text = UIManagement.GetUIMessage(LanguageTranslator.UIExpDemoFreeNA);
                        break;
                    default:
                        buttonExp.explanationText.text = translatedExp;
                        break;
                }
            }
            
        }

    }


}
