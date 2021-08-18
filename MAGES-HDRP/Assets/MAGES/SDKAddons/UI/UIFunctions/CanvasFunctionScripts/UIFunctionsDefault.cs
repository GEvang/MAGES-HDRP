using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.UIManagement;
using System;
using UnityEngine.UI;

public class UIFunctionsDefault : MonoBehaviour {

    /// <summary>
    /// UI is spawned in front of the users head at the time it is called
    /// then it remains static
    /// 
    /// The parameter is not the Enum itself because Unity does not recognize
    /// it as a visual parameter for the invoke function inside the editor, Thanks Unity!
    /// </summary>
    /// <param name="_type"></param>
    public void SpawnUserUI(string _type)
    {
        UserUITypes typeValue = (UserUITypes) Enum.Parse(typeof(UserUITypes), _type);

        if (Enum.IsDefined(typeof(UserUITypes), typeValue))
            UIManagement.SpawnUserUI(typeValue);
        else
            Debug.LogError("UI name does not appear in UserUITypes enum");
    }

    /// <summary>
    /// Spawns the UI at the position it was when it was created as a prefab
    /// 
    /// The parameter is not the Enum itself because Unity does not recognize
    /// it as a visual parameter for the invoke function inside the editor, Thanks Unity!
    /// </summary>
    /// <param name="_type"></param>
    public void SpawnUserUIAsStatic(string _type)
    {
        UserUITypes typeValue = (UserUITypes)Enum.Parse(typeof(UserUITypes), _type);

        if (Enum.IsDefined(typeof(UserUITypes), typeValue))
            UIManagement.SpawnUserUI(typeValue, false);
        else
            Debug.LogError("UI name does not appear in UserUITypes enum");
    }

    public void SpawnApplicationSpecificUI(string _uiName)
    {
        UIManagement.SpawnApplicationSpecificUI(_uiName, true);
    }

    public void SpawnApplicationSpecificUIAsStatic(string _uiName)
    {
        UIManagement.SpawnApplicationSpecificUI(_uiName);
    }

    public void SpawnGameObject(GameObject _obj)
    {
        if(_obj != null)
            Instantiate(_obj);
    }

    /// <summary>
    /// UI plays destruction animation if it has one, then it gets destroyed
    /// </summary>
    public void DestroyCurrentUserUI()
    {
        UIManagement.DestroyCurrentUI();
    }

    /// <summary>
    /// UI is destroyed immediately no matter if it has a destruction animation or not
    /// </summary>
    public void DestroyImmediateCurrentUserUI()
    {
        UIManagement.DestroyCurrentUI(true);
    }

    public void DestroyCurrentSpecialCaseUI()
    {
        UIManagement.DestroyAllCurrentSpecialCaseUI();
    }

    public void DestroyCurrentApplicationSpecificUI()
    {
        UIManagement.DestroyCurrentApplicationSpecificUI();
    }

    public void DestroyGameObject(GameObject _obj)
    {
        Destroy(_obj);
    }

    // Extra Public Functions ---------------------------

    public void SetToolAutoAttach(UIButton _button)
    {
        if (_button == null)
            return;

        if (OvidVRPhysX.OvidVRInteractableItem.GetInteractableItemGrabMode() == OvidVRPhysX.OvidVRInteractableItem.GrabMode.snap)
        {
            OvidVRPhysX.OvidVRInteractableItem.SetInteractableItemGrabMode(OvidVRPhysX.OvidVRInteractableItem.GrabMode.free);
            _button.SetButtonPrimarySecondaryState(false);
        }
        else
        {
            OvidVRPhysX.OvidVRInteractableItem.SetInteractableItemGrabMode(OvidVRPhysX.OvidVRInteractableItem.GrabMode.snap);
            _button.SetButtonPrimarySecondaryState(true);
        }
    }

    public void SetUIProgressNetworkDependantPosition(bool _isSinglePlayer)
    {
        Transform progressUI = UIManagement.Get.transform.Find("CurrentActiveProgressUI/ProgressUIDynamicReposition");

        if (progressUI == null || progressUI.GetComponent<ProgressUIDynamicReposition>() == null)
            return;

        progressUI.GetComponent<ProgressUIDynamicReposition>().UpdateGameStatus(_isSinglePlayer);
    }

    public void EnableAreYouSureButton(GameObject _areYouSureButton)
    {
        // If button does not exist or is already activated, return
        if (!_areYouSureButton || _areYouSureButton.activeSelf)
            return;

        _areYouSureButton.SetActive(true);
        // Enable Animator - It is important to be disabled otherwise it will break the animator with button hover and press behavior
        StartCoroutine(DisableAreYouSureButtonWithDelay(_areYouSureButton));
    }

    // Private Functions -----------------------

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator DisableAreYouSureButtonWithDelay(GameObject _areYouSureButton)
    {
        yield return new WaitForSeconds(8f);

        if (_areYouSureButton)
            _areYouSureButton.SetActive(false);
    }
}
