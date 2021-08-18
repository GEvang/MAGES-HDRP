using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ovidVR.UIManagement;

public class UISpawn : MonoBehaviour
{

    private RectTransform headerTransfrom;
    private Vector3 startPos, endPos;
    private Animator headerAnim;
    private GameObject buttonList;
    private UIButtonList buttonListScript;

    private Transform customBoard;

    [SerializeField, Tooltip("Check if on new UI spawn, all components animations are desired")]
    internal bool allowUIAnimations = true;
    [SerializeField, Tooltip("Check if UI sounds are desired")]
    internal bool allowUISound = true;

    // For respawn purposes, save if the UI was spawned as dynamic or static
    internal bool isDynamicAtSpawn = true;

    // ApplicationSpecific UI is actually the same as UserUI but not for all Applications
    internal bool isAppSpecific = false;

    // The destruction of the UI is delayed if the animations are enabled When the DestroyUI()
    // is bool bcomes true so the UImanagement won't call it's destruction again
    [HideInInspector]
    public bool isUIDestroyed = false;

    private void Awake()
    {
        if (allowUISound)
            UIManagement.PlaySound(UISounds.SpawnUI);

        buttonList = transform.Find("ButtonList").gameObject;
        if (buttonList != null)
        {
            buttonListScript = buttonList.GetComponent<UIButtonList>();
            buttonList.SetActive(false);
        }

        // Just a precaution - TextMask must always be disabled on start
        if (transform.Find("TextMask") != null)
            transform.Find("TextMask").gameObject.SetActive(false);

        customBoard = transform.Find("UIChildCustomBoard");


        // no animation code is needed to run if: header: null || disabled, animation: not allowed to run
        Transform header = transform.Find("Header");

        if (header == null || !header.gameObject.activeSelf || !allowUIAnimations)
        {
            if (buttonList != null)
                buttonList.SetActive(true);
            return;
        }

        headerTransfrom = header.GetComponent<RectTransform>();
        if (headerTransfrom.GetComponent<Animator>() != null)
            headerAnim = headerTransfrom.GetComponent<Animator>();

        if (buttonList != null)
            buttonList.SetActive(true);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator DisableUIAnimation()
    {
        if (headerAnim != null)
            headerAnim.SetBool("PopAnimReverse", true);

        if (customBoard != null && customBoard.GetComponent<Animator>() != null)
            customBoard.GetComponent<Animator>().SetBool("FadeOut", true);

        if (buttonListScript != null)
            buttonListScript.FadeOutButtons();

        yield return new WaitForSeconds(1.2f);

        Destroy(gameObject);
    }

    public void DestroyUI(bool _isApplicationSpecific = false)
    {
        isUIDestroyed = true;

        isAppSpecific = _isApplicationSpecific;

        if (allowUISound)
            UIManagement.PlaySound(UISounds.DestroyUI);

        if (allowUIAnimations)
        {
            // while buttons are fading out, disable invocation to prevent user from triggering again the buttons
            UIButton[] btnScript = GetComponentsInChildren<UIButton>();
            if (btnScript != null || btnScript.Length != 0)
            {
                foreach (UIButton b in btnScript)
                    b.allowInvoke = false;
            }

            StartCoroutine("DisableUIAnimation");
        }
        else
            Destroy(gameObject);
    }
}
