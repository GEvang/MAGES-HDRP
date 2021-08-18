#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using ovidVR.Networking;
using UnityEditor;
using UnityEngine;
using ovidVR.Utilities.prefabSpawnManager.prefabSpawnConstructor;
using ovidVR.toolManager.tool;
using OvidVRPhysX;
using ovidVR.Utilities;
using Photon.Pun;
using System;

public class ovidVRExtendEditor : MonoBehaviour {

    // Prefabs --------------------------------------------------

    [MenuItem("MAGES/Create Prefab/Generic")]
    public static void NewOvidVRPrefabGeneric()
    {
        GameObject newPrefab = new GameObject("New_Generic_Prefab");
        SetUpNetwork(newPrefab);
    }

    [MenuItem("MAGES/Create Prefab/Interactable")]
    public static GameObject NewOvidVRPrefabInteractable()
    {
        GameObject newPrefab = new GameObject("New_Interactable_Prefab");
        SetUpNetwork(newPrefab);

        newPrefab.AddComponent<InteractablePrefabConstructor>();
        newPrefab.AddComponent<Rigidbody>();
        newPrefab.AddComponent<AudioSource>();
        MoveGameobjectInFrontOfCamera(newPrefab);
        return newPrefab;
    }

    [MenuItem("MAGES/Create Prefab/Interactable With Parent")]
    public static void NewOvidVRPrefabInteractableWithParent()
    {
        GameObject newPrefab = new GameObject("New_Interactable_Child_Prefab");
        SetUpNetwork(newPrefab);

        newPrefab.AddComponent<InteractableWithParentPrefabConstructor>();
        newPrefab.AddComponent<Rigidbody>();
        newPrefab.AddComponent<AudioSource>();
    }

    [MenuItem("MAGES/Create Prefab/Final Placement of Interactable")]
    public static GameObject NewOvidVRPrefabInteractableFinalPlacement()
    {
        GameObject newPrefab = new GameObject("New_Final_Placement_Prefab");
        SetUpNetwork(newPrefab);
        newPrefab.AddComponent<InteractableFinalPlacementPrefabConstructor>();
        newPrefab.AddComponent<Rigidbody>();
        newPrefab.AddComponent<PrefabLerpPlacement>();
        MoveGameobjectInFrontOfCamera(newPrefab);
        return newPrefab;
    }

    [MenuItem("MAGES/Create Prefab/Pump Prefab")]
    public static void NewOvidVRPumpPrefab()
    {
        GameObject newPrefab = new GameObject("New_Pump_Prefab");
        newPrefab.AddComponent<Rigidbody>().isKinematic = true;

        SetUpNetwork(newPrefab);
        
        OvidVRInteractableItem ovrItem = newPrefab.AddComponent<OvidVRInteractableItem>();
        ovrItem.DisableKinematicOnAttach = false;
        ovrItem.EnableKinematicOnDetach = true;
        ovrItem.DropDistance = 0.08f;

        OvidVRAutoAttach autoAttach = newPrefab.AddComponent<OvidVRAutoAttach>();
        autoAttach.allowAutoAttachLeftHand = true;
        autoAttach.allowAutoAttachRightHand = true;
        autoAttach.EnableCanAttachOnCollision = true;

        PumpPrefabConstrutor pump = newPrefab.AddComponent<PumpPrefabConstrutor>();
        pump.pumpsToPerform = 1;

        newPrefab.AddComponent<OvidVREnableDisableOnAttach>();
        
        AudioSource audioSource = newPrefab.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f;

        newPrefab.AddComponent<OvidVRNetPumpAnimation>();
    }

    [MenuItem("MAGES/Create Prefab/Tool Collider")]
    public static void NewOvidVRPrefabToolCollider()
    {
        GameObject newPrefab = new GameObject("New_Tool_Collider_Prefab");
        GameObject newChild = new GameObject("Tool_Collider_Child-1 (Template Example)");
        newChild.transform.parent = newPrefab.transform;
        SetUpNetwork(newPrefab);

        newPrefab.AddComponent<ToolColliderPrefabConstructor>();
        newPrefab.AddComponent<Rigidbody>();
        newChild.AddComponent<ToolTriggerCollider>();
    }

    [MenuItem("MAGES/Create Prefab/Use Action Collider")]
    public static GameObject NewOvidVRUseActionCollider()
    {
        GameObject newPrefab = new GameObject("New_Use_Action_Collider_Prefab");
        SetUpNetwork(newPrefab);

        newPrefab.AddComponent<UseColliderPrefabConstructor>();
        newPrefab.AddComponent<Rigidbody>();
        MoveGameobjectInFrontOfCamera(newPrefab);

        return newPrefab;
    }

    [MenuItem("MAGES/Create Prefab/Collision Hit")]
    public static void NewOvidVRPrefabCollisionHit()
    {
        GameObject newPrefab = new GameObject("New_Collition_Hit_Prefab");   
        SetUpNetwork(newPrefab);

        newPrefab.AddComponent<CollisionHitPrefabConstructor>();
        newPrefab.AddComponent<Rigidbody>();
        BoxCollider bx = newPrefab.AddComponent<BoxCollider>();
        bx.isTrigger = true;
    }

    [MenuItem("MAGES/Create Prefab/Remove With Tools")]
    public static void NewOvidVRPrefabToolRemoval()
    {
        GameObject newPrefab = new GameObject("New_Tool_Removal_Prefab");
        SetUpNetwork(newPrefab);

        newPrefab.AddComponent<RemoveWithToolsCostructor>();
        newPrefab.AddComponent<Rigidbody>();
    }

    [MenuItem("MAGES/Create Prefab/Animation Prefab Constructor")]
    public static void NewOvidVRPrefabAnimationConstructor()
    {
        GameObject newPrefab = new GameObject("New_Animation_Prefab_Constructor");
        GameObject interactablePrefab = new GameObject("Interactable Part");
        GameObject animationPrefab = new GameObject("Animation Part");
        GameObject animationEndPrefab = new GameObject("End of Interactable");

        animationPrefab.transform.parent = newPrefab.transform;
        animationPrefab.AddComponent<Animator>();

        interactablePrefab.transform.parent = animationPrefab.transform;
        var interactableItem = interactablePrefab.AddComponent<OvidVRInteractableItem>();
        interactableItem.EnableKinematicOnDetach = true;
        interactablePrefab.AddComponent<ConfigureChildMovment>();


        animationEndPrefab.transform.parent = newPrefab.transform;

        SetUpNetworkChild(newPrefab,interactablePrefab);

        newPrefab.AddComponent<AnimationMovePrefabConstructor>();
    }

    public static void SetUpNetwork(GameObject _prefab)
    {      
        OvidVrNetworkingId ni = _prefab.AddComponent<OvidVrNetworkingId>();
        _prefab.AddComponent<PhotonView>().OwnershipTransfer = OwnershipOption.Request;

        OvidVrSyncTransformPhoton nt = _prefab.AddComponent<OvidVrSyncTransformPhoton>();
        nt.SendTimesPerSecond = 20;
        nt.ChangeSyncMode(OvidVRSyncTransformMode.all);
        nt.MovementThreshold = 0.005f;
        nt.RotationThreshold = 1f;
    }

    public static void SetUpNetworkChild(GameObject _prefab, GameObject _child)
    {
        OvidVrNetworkingId ni = _prefab.AddComponent<OvidVrNetworkingId>();
        _prefab.AddComponent<PhotonView>().OwnershipTransfer = OwnershipOption.Request;

        _prefab.AddComponent<RequestAuthorityForChildren>();
        var multiTransformSync = _prefab.AddComponent<NetworkMultiTransformSync>();
        multiTransformSync.T = new Transform[1];
        multiTransformSync.T[0] = _child.transform;
        multiTransformSync.SendTimesPerSecond = 20;
        multiTransformSync.MovementThreshold = 0.005f;
        multiTransformSync.RotationThreshold = 1f;
        
    }

    // Tools ----------------------------------------------------

    [MenuItem("MAGES/Tools/New Tool")]
    public static void NewOvidVRTool()
    {
        GameObject toolSet = GameObject.Find("Models/Dynamic/Tools/ToolSet");

        GameObject newTool = new GameObject("'NewToolName'Pivot");
        newTool.transform.parent = toolSet.transform;

        newTool.AddComponent<Rigidbody>();
        OvidVRInteractableItem oit = newTool.AddComponent<OvidVRInteractableItem>();
        oit.CanAttach = true;
        oit.DisableKinematicOnAttach = true;
        oit.EnableKinematicOnDetach = false;
        oit.EnableGravityOnDetach = true;
        oit.TwoHanded = false;
        oit.DisablePhysicalMaterialsOnAttach = false;

        ToolConstructor tc = newTool.AddComponent<ToolConstructor>();
        tc.resetToolTimer = 4f;
        tc.toolGrabType = ToolGrabbingType.Grab;

        newTool.layer = LayerMask.NameToLayer("Tools");
    }

    public static void MoveGameobjectInFrontOfCamera(GameObject _gameObject)
    {
        try
        {
            _gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.01f;
        }
        catch(Exception e)
        {
            Debug.LogError("Can not move GameObject in front of main camera: " + e);
        }
    }

    public static void RemoveAllComponenetsFromGameObject(GameObject _gameObject, bool recursively = false)
    {
        for (int i = 0; i < 6; i++)
        {
            foreach (var comp in _gameObject.GetComponents<Component>())
            {
                if (!(comp is Transform))
                {
                    try
                    {
                        DestroyImmediate(comp);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        if(recursively)
        {
            for(int i =0;i<_gameObject.transform.childCount;++i)
            {
                RemoveAllComponenetsFromGameObject(_gameObject.transform.GetChild(i).gameObject,recursively);
            }
        }
    }

    public static void AddDefaultBoxColliders(GameObject _gameObject,bool isTrigger, bool recursively = false)
    {
        if(_gameObject.GetComponent<Renderer>()!=null)
            _gameObject.AddComponent<BoxCollider>().isTrigger = isTrigger;

        if (recursively)
        {
            for (int i = 0; i < _gameObject.transform.childCount; ++i)
            {
                AddDefaultBoxColliders(_gameObject.transform.GetChild(i).gameObject,isTrigger, recursively);
            }
        }
    }

    public static void AddDefaultHologramMaterial(GameObject _gameObject, bool recursively = false)
    {

        Material holoMat = AssetDatabase.LoadAssetAtPath("Assets/Resources/MAGESres/HolographicPrefabs/Materials/HoloMaterial.mat", typeof(Material)) as Material;


        if (_gameObject.GetComponent<Renderer>() != null)
        {
            Renderer r = _gameObject.GetComponent<Renderer>();
            r.sharedMaterial = holoMat;
            r.sharedMaterials = new Material[] {holoMat };
        }

        if (recursively)
        {
            for (int i = 0; i < _gameObject.transform.childCount; ++i)
            {
                AddDefaultHologramMaterial(_gameObject.transform.GetChild(i).gameObject,recursively);
            }
        }
    }

}
#endif