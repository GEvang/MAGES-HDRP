/**
 * Date: 30th November 2020
 * Status: Stable, Inconsistent Visuals due to lack of finalized UI template
 * 
 * 1. How it works
 * 
 * BLOW UP:
 * >Lerp Updates:
 * - Fade Out Text
 * - Scale Up Backround & Border Image
 * - Move the Animated Image (the selected image) to the right (X) to preserve ratios
 *   as much as the X scale is to the images above
 * - Change the border image color to highlight
 * - Fade In Text
 * 
 * >Static Updates:
 * - Set Width & height of text and text mask to eliminate scrolling on hidden text
 * - Set Text Alignment (to avoid differently created or neglected UIs)
 * - Update the Font Size
 * 
 * BLOW DOWN - RESET:
 * >Lerp Updates:
 * - Fade Out Text
 * - Scale Down Backround & Border Image to default values
 * - Move the Animated Image (the selected image) to the left (X) to preserve ratios
 *   as much as the stored default values
 * - Change the border image color to the default state
 * - Fade In Text
 * 
 * >Static Updates:
 * - Set Width & height of text and text mask to eliminate scrolling on hidden text
 * - Set Text Alignment (to avoid differently created or neglected UIs)
 * - Update the Font Size to default values
 * 
 * 2. Times
 * - 1 second in total
 *   - 0.36: sec waiting from state change (hovering -> not hovered || not hovering -> hovered)
 *     to avoid constant scaling
 *   - 0.64: Scaling Up or Down. Split to 3 sections. 1/3 fade out text, 1/3 blow up, 1/3 fade in text
 *   
 * 3. On Start
 * >Values:
 * - store all values expalined below
 * - one scale selection for images
 * - one font selection for text
 * - one color selection for border image
 *
 * >Custom Changes:
 * - Some UI children were created for the automated scrolling. All the components mentioned below should be eliminated
 *   from the prefabs. But since there are a lot of them, the code expalined below works as a safety net
 *   - ScrollText Script: Deleted
 *     (when hovering it scrolls teh rest of text, redundant)
 *   - Content Size Fitter Component: Updated Values & Deleted
 *     (for the script above to work)
 *     
 * CAUTION - NOTICE
 * A lot of code on this script exists due to UI inconsistencies despite having a template
 * On a new module FIX one UI template first, then remove all excess code on this script
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHoverScale : MonoBehaviour
{
    private Vector2 targetBackgroundImageScale = new Vector2(2f,2.5f);

    private Vector2 targetTextWidthHeightMultiplier = new Vector2(2.5f, 3f);

    private float targetTextFontMultiplier = 1.6f;

    private Color targetBorderColor = Color.white;

    private ButtonBehavior buttonScript;

    //-1 : Not initiated
    // 0 : Static
    // 1 : Scaling Up || Scaled up
    // 2 : Scaling down
    private short animState = -1;

    // Background Images: Scale
    private RectTransform foregroundImageTr;
    private RectTransform borderImageTr;
    private Vector3 imagesDefaultScale;

    // Collider: Scale
    private BoxCollider btnCollider;
    private Vector3 colliderDefaultScale;

    // Background Border Image: Color
    Image borderImage;
    Color defaultColor;

    // Text & mask: Width & Height & Font & Alignment
    private RectTransform textMaskTr;
    private Vector2 textMaskDefaultWidthHeight;
    private RectTransform textChildTr;
    private Vector2 textChildDefaultWidthHeight;
    private Text textText;
    private int textDefaultFontSize;
    private int textDefaultFontMaxSize;
    private TextAnchor textDefaultAlignment;

    // Select image Rectange: Scale & Translate
    // Reason: preserve distance ratios
    private RectTransform animatedImageTr;
    private Vector3 animatedImageDefaultPosition;

    // Arrow Transform: Child Enable/Disable
    private RectTransform arrowTransform;
    private bool arrowDefaultState;

    // the waiting period inside the coroutine does not count
    private bool isCoroutineAnimationRunning;

    IEnumerator Start()
    {
        buttonScript = GetComponent<ButtonBehavior>();

        btnCollider = GetComponent<BoxCollider>();
        colliderDefaultScale = btnCollider.size;

        if (transform.Find("Background/Border"))
        {
            borderImageTr = transform.Find("Background/Border").GetComponent<RectTransform>();
            borderImage = borderImageTr.GetComponent<Image>();
            defaultColor = borderImage.color;
        }
        if (transform.Find("Background/Foreground"))
            foregroundImageTr = transform.Find("Background/Foreground").GetComponent<RectTransform>();

        imagesDefaultScale = borderImageTr.localScale;

        if (transform.Find("ButtonText"))
        {
            textMaskTr = transform.Find("ButtonText").GetComponent<RectTransform>();
            textMaskDefaultWidthHeight = new Vector2(textMaskTr.rect.width, textMaskTr.rect.height);

            // Some UIs are not created the same and do not have as a child the Text or the Arrows
            // Some UIs: ButtonText (as mask for text): Child 0: Text, Child 1: ButtonAnimatedImage - CORRECT
            // Other UIs: ButtonText (as text): ChildCount: 0 - WRONG
            if (transform.Find("ButtonText/Text"))
            {
                textChildTr = transform.Find("ButtonText/Text").GetComponent<RectTransform>();
                textChildDefaultWidthHeight = new Vector2(textChildTr.rect.width, textChildTr.rect.height);

                textText = textChildTr.GetComponent<Text>();
                textDefaultFontSize = textText.fontSize;
                textDefaultFontMaxSize = textText.resizeTextMaxSize;
                textDefaultAlignment = textText.alignment;

                // In case the scrolling text Components are forgotten in prefabs
                // SrollText Script delete + Content Size Fitter unconstrained & delete
                if (textMaskTr.GetComponentInChildren<ScrollText>())
                    Destroy(textMaskTr.GetComponentInChildren<ScrollText>());

                if (textChildTr.GetComponent<ContentSizeFitter>())
                {
                    // If ContentSizeFitter exists then the constrained Width or Height values will be 0
                    if (textChildTr.GetComponent<ContentSizeFitter>().horizontalFit != ContentSizeFitter.FitMode.Unconstrained)
                        textChildDefaultWidthHeight.x = textText.preferredWidth;
                    if (textChildTr.GetComponent<ContentSizeFitter>().verticalFit != ContentSizeFitter.FitMode.Unconstrained)
                        textChildDefaultWidthHeight.y = textText.preferredHeight;

                    textChildTr.sizeDelta = textChildDefaultWidthHeight;
                    Destroy(textChildTr.GetComponent<ContentSizeFitter>());
                }
            }
            else
            {
                textChildDefaultWidthHeight = new Vector2(textMaskTr.rect.width, textMaskTr.rect.height);

                textText = textMaskTr.GetComponent<Text>();
                textDefaultFontSize = textText.fontSize;
                textDefaultFontMaxSize = textText.resizeTextMaxSize;
                textDefaultAlignment = textText.alignment;

                if (textMaskTr.GetComponentInChildren<ScrollText>())
                    Destroy(textMaskTr.GetComponentInChildren<ScrollText>());
            }

            if (textChildTr)
            {
                arrowTransform = transform.Find("ButtonText/ArrowsTransform").GetComponent<RectTransform>();
                arrowDefaultState = arrowTransform.GetChild(0).gameObject.activeSelf;
            }
        }

        if (transform.Find("ButtonAnimatedImage"))
        {
            animatedImageTr = transform.Find("ButtonAnimatedImage").GetComponent<RectTransform>();
            animatedImageDefaultPosition = animatedImageTr.localPosition;
        }

        yield return new WaitForSeconds(2f);
        animState = 0;
        isCoroutineAnimationRunning = false;
    }

    private void OnDisable()
    {
        ResetHoverBlowUpState();
    }

    private void Update()
    {
        if (animState == -1 || isCoroutineAnimationRunning)
            return;

        if (!buttonScript.GetIsButtonInteractive())
        {
            if (animState != 0)
            {
                StopAllCoroutines();
                animState = 0;
                StartCoroutine(BlowUp(false));
            }

            return;
        }

        if (buttonScript.GetIsButtonHovered() && animState != 1)
        {
            StopAllCoroutines();
            animState = 1;
            StartCoroutine(BlowUp(true));
        }

        if (!buttonScript.GetIsButtonHovered() && animState == 1)
        {
            StopAllCoroutines();
            animState = 2;
            StartCoroutine(BlowUp(false));
        }
    }

    IEnumerator BlowUp(bool _scaleUp)
    {
        // not immediately
        yield return new WaitForSeconds(0.62f);

        // coroutine counts as running only during the animation, not the waiting phase
        isCoroutineAnimationRunning = true;

        float timer = 0, lerpTimer = 0.6f, timeStep = 0f;

        // Last child is rendered last, on top of all - visual consistency when scaling up
        if (animState == 1)
            transform.SetSiblingIndex(transform.parent.childCount - 1);

        Vector3 targetImageScaleMul = new Vector3();
        Vector3 targetColliderSize = new Vector3();
        Vector3 targetAnimatedImagePosition = new Vector3();
        Vector2 targetTextMaskWidthHeight = new Vector2();
        Vector2 targetTextChildWidthHeight = new Vector2();
        int targetFontSize = 0;
        Color targetTextColor;

        if (_scaleUp)
        {
            // Image scale must be multiplied on top of default values - z is unchanged
            Vector3 convertV2toV3 = targetBackgroundImageScale;
            convertV2toV3.z = imagesDefaultScale.z;
            targetImageScaleMul = Vector3.Scale(imagesDefaultScale, convertV2toV3);

            // Collider scales as much as the Images (aka the total area of the button) - z is unchanged
            convertV2toV3.z = colliderDefaultScale.z;
            targetColliderSize = Vector3.Scale(colliderDefaultScale, convertV2toV3);

            // Position on X of Selection Image must be multiplied by the same value as the scale on X
            // of itself to preserve it's new position. Scale is same as the parent - backgound image
            targetAnimatedImagePosition = animatedImageDefaultPosition; targetAnimatedImagePosition.x *= targetBackgroundImageScale.x;
            targetTextMaskWidthHeight = new Vector2(textMaskDefaultWidthHeight.x * targetTextWidthHeightMultiplier.x,
                                                            textMaskDefaultWidthHeight.y * targetTextWidthHeightMultiplier.y);

            // Text height is bigger from mask height. Thus when both get larger the text has "free space" and it becomes misaligned
            // Thus when the blow up animation plays teh text scales exactly to the values of it's partent-mask image
            // When it scales down it goes back to it's original-default scale. All are in terms of width & height NOT actual scale
            targetTextChildWidthHeight = targetTextMaskWidthHeight;

            targetFontSize = (int)(textDefaultFontSize * targetTextFontMultiplier);
        }

        // Show / Hide Arrow button depending on it's default state (if it was enabled by default)
        if (arrowDefaultState && textChildTr)
            arrowTransform.GetChild(0).gameObject.SetActive(!_scaleUp);
     
        // STEP 1: Fade Out Text
        targetTextColor = textText.color;
        while (timer <= lerpTimer / 3f)
        {
            targetTextColor.a = Mathf.Lerp(1f, 0f, timer / (lerpTimer / 3f));
            textText.color = targetTextColor;

            timer += Time.deltaTime;
            yield return null;
        }
        targetTextColor.a = 0f;
        textText.color = targetTextColor;

        // Now that text is faded change all about text
        if (_scaleUp)
        {
            textText.resizeTextMaxSize = targetFontSize;
            textText.alignment = TextAnchor.MiddleLeft;

            textMaskTr.sizeDelta = targetTextMaskWidthHeight;

            if (textChildTr)
                textChildTr.sizeDelta = targetTextChildWidthHeight;

            textText.fontSize = targetFontSize;
        }
        else
        {
            textText.resizeTextMaxSize = textDefaultFontMaxSize;
            textText.alignment = textDefaultAlignment;

            textMaskTr.sizeDelta = textMaskDefaultWidthHeight;

            if (textChildTr)
                textChildTr.sizeDelta = textChildDefaultWidthHeight;

            textText.fontSize = textDefaultFontSize;
        }

        // STEP 2: Blow Up Animation
        timer = 0f;
        while (timer <= lerpTimer / 3)
        {
            // Smoother version of smoothstep
            timeStep = timer / (lerpTimer / 3);
            //timeStep = timeStep * timeStep * timeStep * (timeStep * (6f * timeStep - 15f) + 10f);

            if (_scaleUp)
            {
                borderImageTr.localScale = Vector3.Lerp(borderImageTr.localScale, targetImageScaleMul, timeStep);
                foregroundImageTr.localScale = Vector3.Lerp(foregroundImageTr.localScale, targetImageScaleMul, timeStep);
                borderImage.color = Color.Lerp(borderImage.color, targetBorderColor, timeStep);

                btnCollider.size = Vector3.Lerp(btnCollider.size, targetColliderSize, timeStep);

                if(animatedImageTr)
                    animatedImageTr.localPosition = Vector3.Lerp(animatedImageTr.localPosition, targetAnimatedImagePosition, timeStep);               
            }
            else
            {
                borderImageTr.localScale = Vector3.Lerp(borderImageTr.localScale, imagesDefaultScale, timeStep);
                foregroundImageTr.localScale = Vector3.Lerp(foregroundImageTr.localScale, imagesDefaultScale, timeStep);
                borderImage.color = Color.Lerp(borderImage.color, defaultColor, timeStep);

                btnCollider.size = Vector3.Lerp(btnCollider.size, colliderDefaultScale, timeStep);

                if (animatedImageTr)
                    animatedImageTr.localPosition = Vector3.Lerp(animatedImageTr.localPosition, animatedImageDefaultPosition, timeStep);              
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // STEP 3: Fade In Text
        timer = 0f;
        targetTextColor = textText.color;
        while (timer <= lerpTimer / 3f)
        {
            targetTextColor.a = Mathf.Lerp(0f, 1f, timer / (lerpTimer / 3f));
            textText.color = targetTextColor;

            timer += Time.deltaTime;
            yield return null;
        }
        targetTextColor.a = 1f;
        textText.color = targetTextColor;

        if (!_scaleUp)
            animState = 0;

        isCoroutineAnimationRunning = false;
    }

    // Public Functions -----------------------------

    public void ResetHoverBlowUpState()
    {
        StopAllCoroutines();
        isCoroutineAnimationRunning = false;
        animState = 0;

        borderImageTr.localScale = imagesDefaultScale;
        foregroundImageTr.localScale = imagesDefaultScale;
        borderImage.color = defaultColor;

        btnCollider.size = colliderDefaultScale;

        if (animatedImageTr)
            animatedImageTr.localPosition = animatedImageDefaultPosition;

        textMaskTr.sizeDelta = textMaskDefaultWidthHeight;
        if (textChildTr)
            textChildTr.sizeDelta = textChildDefaultWidthHeight;

        textText.fontSize = textDefaultFontSize;
        textText.resizeTextMaxSize = textDefaultFontMaxSize;
        textText.alignment = textDefaultAlignment;
    }
}
