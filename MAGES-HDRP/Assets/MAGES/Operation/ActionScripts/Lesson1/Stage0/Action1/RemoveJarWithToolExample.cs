using ovidVR.ActionPrototypes;
using ovidVR.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Remove using Tool example
/// The Selected Tool to remove the prefabs is set from the prefab constructor (Unity editor) NOT from the script
/// </summary>
public class RemoveJarWithToolExample : RemoveAction {

    /// <summary>
    /// Initialize() method overrides base.Initialize and sets the removable prefab 
    /// </summary>
    public override void Initialize()
    {    
        //Sets removable prefabs
        //Each method call adds a new removable prefab to remove prefab list
        //Ehen user removes all of them then the Action performs
        SetRemovePrefab("Lesson1/Stage0/Action1/MinoanJar1RemovePivot");
        SetRemovePrefab("Lesson1/Stage0/Action1/MinoanJar2RemovePivot");
        SetRemovePrefab("Lesson1/Stage0/Action1/MinoanJar3RemovePivot");
        SetRemovePrefab("Lesson1/Stage0/Action1/MinoanJar4RemovePivot");

        SetHoloObject("Lesson1/Stage0/Action1/Hologram/HologramL1S0A1");

        base.Initialize();
    }

}
