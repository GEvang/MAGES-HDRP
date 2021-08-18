using ovidVR.GameController;
using UnityEngine;
using ovidVR.UIManagement;
using System.Collections;

public class UIButtonsInitState : MonoBehaviour {

    public void AdjustToolHandPosture(UIButton _button)
    {
        if (_button == null)
            return;
        
        if (OvidVRPhysX.OvidVRInteractableItem.GetInteractableItemGrabMode() == OvidVRPhysX.OvidVRInteractableItem.GrabMode.snap)
            _button.SetButtonPrimarySecondaryState(true);
        else
            _button.SetButtonPrimarySecondaryState(false);
    }

    public void SetCreateSessionButtonState(UIButton _button)
    {
        if (_button == null)
            return;
    }

    public void DisableButtonForClientUsers(UIButton _button)
    {
        if (_button == null)
            return;

        if (OvidVRControllerClass.NetworkManager.GetIsClient())
            _button.SetButtonActivity(false, LanguageTranslator.NAJoinedUsersPermBtnExp);
    }

    public void DisableButtonForSixDOF(UIButton _button)
    {
        if (_button == null)
            return;

        if (OvidVRControllerClass.Get.DOF == ControllerDOF.SixDOF)
            _button.SetButtonActivity(false, LanguageTranslator.NAModeBtnExp);
    }

    public void DisableButtonForThreeDOF(UIButton _button)
    {
        if (_button == null)
            return;

        if (OvidVRControllerClass.Get.DOF == ControllerDOF.ThreeDOF)
            _button.SetButtonActivity(false, LanguageTranslator.NAModeBtnExp);
    }

    public void DisableButtonForTwoDOF(UIButton _button)
    {
        if (_button == null)
            return;

        if (OvidVRControllerClass.Get.DOF == ControllerDOF.TwoDOF)
            _button.SetButtonActivity(false, LanguageTranslator.NAModeBtnExp);
    }

    public void DisableAreYouSureButton(GameObject _areYouSureButton)
    {
        if (_areYouSureButton == null || !_areYouSureButton.activeSelf)
            return;

        StartCoroutine(DisableAreYouSureButtonWithDelay(_areYouSureButton));
    }

    private IEnumerator DisableAreYouSureButtonWithDelay(GameObject _areYouSureButton)
    {
        yield return 0;

        if (_areYouSureButton)
            _areYouSureButton.SetActive(false);
    }
}
