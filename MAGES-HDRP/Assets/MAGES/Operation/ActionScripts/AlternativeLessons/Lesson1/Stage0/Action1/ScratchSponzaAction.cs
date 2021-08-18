using ovidVR.ActionPrototypes;
using ovidVR.toolManager.tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ToolAction Example
/// </summary>
public class ScratchSponzaAction : ToolAction
{
    /// <summary>
    /// Initialize() method overrides base.Initialize and sets the Action
    /// </summary>
    public override void Initialize()
    {
        //Set the colliders that will trigger the perform when all touched by the tool
        //The second argument is the tooll needed to complete the Action
        SetToolActionPrefab("AlternativeLessonPrefabs/SponzaRestoration/Stage0/Action1/RoofDirt_Tool_Collider_Prefab", ToolsEnum.Scalpel);
        //Error colliders are trigger colliders that will count an error when touched by a tool
        SetErrorColliders("AlternativeLessonPrefabs/SponzaRestoration/Stage0/Action1/Colliders/ErrorColliders");
        //Physical collider for the Action
        SetPhysicalColliderPrefab("AlternativeLessonPrefabs/SponzaRestoration/Stage0/Action1/Colliders/PhysicalCollider");
        //Action's hologram
        SetHoloObject("AlternativeLessonPrefabs/SponzaRestoration/Stage0/Action1/Hologram/hologram_scalpel");

        base.Initialize();
    }
}
