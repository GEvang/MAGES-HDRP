using ovidVR.ActionPrototypes;
using ovidVR.Utilities;
using UnityEngine;

/// <summary>
/// Use Action Example
/// </summary>
public class CleanSponzaAction : UseAction {

    GameObject sponzaHandLock;

    /// <summary>
    /// Initialize method overrides base.Initialize() and sets the use Action
    /// </summary>
    public override void Initialize()
    {
        //Set the interactable prefab which user will take and use it (touch collider) to perform the Action (1st argument)
        //Sets the collider which triggers the use Prefab. For more customization (CollisionStay time toperform) see prefab constructor(Unity editor) (2nd argument)
        SetUsePrefab("AlternativeLessonPrefabs/SponzaRestoration/Stage0/Action0/cloth", "AlternativeLessonPrefabs/SponzaRestoration/Stage0/Action0/Dust");
        //Sets physical colliders that will spawn on initialize
        //For this Action we need extra non triggered colliders since the model of Sponza dont have by default
        //These collider will be destroyed after perform/ Undo
        SetPhysicalColliderPrefab("AlternativeLessonPrefabs/SponzaRestoration/Stage0/Action0/PhysicalCollider");
        //Sets the hologram for current Action
        SetHoloObject("AlternativeLessonPrefabs/SponzaRestoration/Stage0/Action0/Hologram/hologram_clotha");

        if (sponzaHandLock)
            DestroyUtilities.RemoteDestroy(sponzaHandLock);

        sponzaHandLock = Spawn("AlternativeLessonPrefabs/SponzaRestoration/Stage0/Action0/SponzaHandLock");

        base.Initialize();
    }

    public override void Perform()
    {
        DestroyUtilities.RemoteDestroy(sponzaHandLock);

        base.Perform();
    }

    public override void Undo()
    {
        DestroyUtilities.RemoteDestroy(sponzaHandLock);

        base.Undo();
    }
}
