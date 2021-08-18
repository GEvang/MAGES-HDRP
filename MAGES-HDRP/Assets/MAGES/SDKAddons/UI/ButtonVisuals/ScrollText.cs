using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollText : MonoBehaviour
{
    private bool isHovering = false;
    private ScrollRect scrollRect;
    private GameObject arrowAnimation;
    private GameObject buttonText;
    private bool isScrolled = false;
    //[Header("Set speed of scrolling")]
    private float scrollSpeed = 1.0f;
    private float textLength = 0f;
    private float buttonLength = 0f;
    private float distance = 0;
    private float scrollingDelay = 200.0f;


    // Start is called before the first frame update
    void Start()
    {

        scrollRect = GetComponent<ScrollRect>();

        arrowAnimation = gameObject.transform.Find("ArrowsTransform").gameObject;
        buttonText = gameObject.transform.Find("Text").gameObject;



        buttonLength = GetComponent<RectTransform>().rect.height;
        textLength = buttonText.GetComponent<RectTransform>().rect.height;
        distance = textLength - buttonLength;
    }

    // Update is called once per frame
    void Update()
    {
        ScrollOnHover();


    }



    public void ScrollOnHover()
    {
        isHovering = GetComponentInParent<ButtonBehavior>().GetIsButtonHovered();


        if (scrollRect)
        {
            if (!isScrolled)
            {
                if (scrollRect)
                    scrollRect.verticalNormalizedPosition = 1f;
            }


            if (isHovering)
            {
                DisableArrowAnimation();
                //scroll to bottom
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, 0f, scrollSpeed * Time.deltaTime / (distance / scrollingDelay));
                isScrolled = true;

            }

            if (!isHovering && isScrolled)
            {
                EnableArrowAnimation();
                //scroll to up
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, 1f, scrollSpeed * Time.deltaTime / (distance / scrollingDelay));

            }
        }

    }

    private void DisableArrowAnimation()
    {

        if (arrowAnimation)
        {
            arrowAnimation.SetActive(false);

        }
    }

    private void EnableArrowAnimation()
    {
        if (arrowAnimation)
        {
            arrowAnimation.SetActive(true);
        }
    }


}
