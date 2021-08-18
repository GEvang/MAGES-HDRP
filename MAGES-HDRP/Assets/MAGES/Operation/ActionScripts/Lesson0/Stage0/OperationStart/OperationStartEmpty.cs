﻿using UnityEngine;
using ovidVR.CustomEventManager;
using ovidVR.sceneGraphSpace;
using UnityEngine.Events;
using System;
using ovidVR.UIManagement;
using ovidVR.Utilities;
using System.Collections;
using ovidVR.GameController;

public class OperationStartEmpty : MonoBehaviour, IAction
{

    #region Action Variables
    private UnityAction listener;
    private GameObject customizationCanvas;
    private GameObject startAnimation;
    private GameObject sanitizer;
    private bool initialized = false;
    #endregion

    #region IAction Variables
    private string AName;
    private GameObject acNode;
    public string ActionName
    {
        get { return this.AName; }
        set { this.AName = value; }
    }
    public GameObject ActionNode
    {
        get { return this.acNode; }
        set { this.acNode = value; }
    }
    public int AlternativePath
    {
        get { return altPath; }
        set { this.altPath = value; }
    }
    private int altPath = -1;
    #endregion

    #region IAction Functions
    public void Initialize()
    {
        InterfaceManagement.Get.SetUserSpawnedUIAllowance(false);

        listener = new UnityAction(EndAction);
        EventManager.StartListening(this.gameObject.name, listener);

        EnableButton();

        StartCoroutine("SpawnAfterWelcome");
    }

    IEnumerator SpawnAfterWelcome()
    {
        InterfaceManagement.Get.InterfaceRaycastActivation(false);

        yield return new WaitForSeconds(5f);

        ovidVR.AmbientSounds.ApplicationAmbientSounds.PlayAmbientMusic();

        InterfaceManagement.Get.InterfaceRaycastActivation(true);
        customizationCanvas = PrefabImporter.SpawnGenericPrefab("MAGESres/UI/OperationPrefabs/CharacterCustomizationCanvas");
    }

    public void Perform()
    {
        //Replace all the prefabs with new ones for the Coop
        //All prefabs in coop need to spawn from the server to sync their positions to all the players
        if (!initialized)
        {
            SpawnNetworkPrefabs();
            initialized = true;
        }

        EventManager.StopListeningAll(this.gameObject.name);

        if (GameObject.Find("RaycastRightHand/Sphere"))
            Destroy(GameObject.Find("RaycastRightHand/Sphere").GetComponent<BoxCollider>());

        if (GameObject.Find("RaycastLeftHand/Sphere"))
            Destroy(GameObject.Find("RaycastLeftHand/Sphere").GetComponent<BoxCollider>());

        if (GameObject.Find("CharacterCustomizationCanvas(Clone)"))
        {
            customizationCanvas.GetComponent<CustomizationManager>().SkipAllActions();
        }

        InterfaceManagement.Get.SetUserSpawnedUIAllowance(true);
        InterfaceManagement.Get.ResetInterfaceManagement();

        InterfaceManagement.Get.InterfaceRaycastActivation(false);

        if (startAnimation)
            Destroy(startAnimation);

        StopAllCoroutines();

        ovidVR.AmbientSounds.ApplicationAmbientSounds.PlayAmbientNoise();




        DestroyUtilities.RemoteDestroy(GameObject.Find("OperationStart(Clone)"));
    }


    private void SpawnNetworkPrefabs()
    {

    }
    void EndAction()
    {
        Operation.Get.Perform();
    }

    public void Undo()
    {
        EventManager.StopListeningAll(this.gameObject.name);

        InterfaceManagement.Get.ResetInterfaceManagement();
        InterfaceManagement.Get.InterfaceRaycastActivation(false);

        if (startAnimation)
            Destroy(startAnimation);

        StopAllCoroutines();

        Debug.LogWarning("Can't Undo first Action");
    }

    public void InitializeHolograms()
    { }

    public void DifficultyRestrictions()
    { }

    public void SetNextModule(Action action)
    {
    }

    public void EnableButton()
    {
        GameObject prev = GameObject.Find("PreviousActionTab");
        if (prev)
        {
            if (prev.transform.childCount == 3)
            {
                prev.transform.GetChild(0).gameObject.SetActive(true);
                prev.transform.GetChild(1).gameObject.SetActive(true);
                prev.transform.GetChild(2).gameObject.SetActive(true);
            }

        }
    }
    #endregion

}
