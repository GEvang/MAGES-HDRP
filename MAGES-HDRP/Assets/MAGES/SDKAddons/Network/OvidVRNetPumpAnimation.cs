using UnityEngine;
using ovidVR.GameController;
using ovidVR.Utilities.prefabSpawnManager.prefabSpawnConstructor;
using ovidVR.Networking;
using Photon.Pun;

public class OvidVRNetPumpAnimation : MonoBehaviour
{
    private Animation pumpAnimation;
    private float lastAnimationValue;
    private string currentHandInteracting = "RightHandAnimation";
    private PumpPrefabConstrutor pumpPrefabConstructor;
    private OvidVrNetworkingId netID;

    private void Start()
    {
        netID = this.GetComponent<OvidVrNetworkingId>();
        try
        {
            lastAnimationValue = 0.0f;
            pumpPrefabConstructor = GetComponent<PumpPrefabConstrutor>();
            pumpAnimation = GetComponent<Animation>();

        }
        catch (System.Exception e)
        {
            Debug.LogError("" + e.StackTrace);
        }
    }

    void Update()
    {
        if (!OvidVRControllerClass.NetworkManager.GetIsInNetwork())
        {
            return;
        }

        if (netID.HasAuthority)
        {
            float currentAnimValue = GetCurrentAnimationValue();

            if (currentAnimValue < 0.01 || currentAnimValue > 0.98)
            {
                return;
            }

            if (currentAnimValue != lastAnimationValue)
            {
                netID.ThisPV.RPC(nameof(RpcServerSendAnimationValue), RpcTarget.Others, currentAnimValue);
                RpcServerSendAnimationValue(currentAnimValue);
                lastAnimationValue = currentAnimValue;
            }            
        }
    }

    #region network commands

    [PunRPC] //Server -> Client
    public void RpcServerSendAnimationValue(float _animationValue)
    {
        lastAnimationValue = _animationValue;
        SetAnimationValue(_animationValue);
    }

    #endregion


    private float GetCurrentAnimationValue()
    {
        float returnValue = 0;

        if (pumpAnimation)
        {
            currentHandInteracting = pumpPrefabConstructor.GetAttachedHand();

            ; if (pumpAnimation[currentHandInteracting])
            {
                returnValue = pumpAnimation[currentHandInteracting].normalizedTime;
            }
        }
        else
        {
            if (gameObject.GetComponent<Animation>())
            {
                pumpAnimation = GetComponent<Animation>();
            }
        }
        return returnValue;
    }

    private void SetAnimationValue(float _animValue)
    {
        if (pumpAnimation)
        {
            currentHandInteracting = pumpPrefabConstructor.GetAttachedHand();
            if (pumpAnimation[currentHandInteracting])
            {
                pumpAnimation.Play(currentHandInteracting);
                pumpAnimation[currentHandInteracting].normalizedTime = _animValue;
            }

            pumpPrefabConstructor.PlayPumpSound();
        }
    }

}
