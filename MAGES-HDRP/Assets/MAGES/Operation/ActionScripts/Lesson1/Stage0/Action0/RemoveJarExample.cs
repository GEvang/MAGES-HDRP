using ovidVR.ActionPrototypes;
using ovidVR.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ovidVR.Utilities.VoiceActor;
/// <summary>
/// Example of Remove Action with hand
/// </summary>
public class RemoveJarExample : RemoveAction {

    /// <summary>
    /// Initialize() is the only method from basePrototye that we MUST orerride
    /// This method initializes the Action by setting the paths to the spawned prefabs
    /// </summary>
    public override void Initialize()
    {
        Destroy(GameObject.Find("MinoanJar"));
        //This method sets the prefab that will be removed
        SetRemovePrefab("Lesson1/Stage0/Action0/MinoanJarRemove");
        //Sets hologram 
        SetHoloObject("Lesson1/Stage0/Action0/Hologram/HologramL1S0A0Hand");

        //Set Voice Actor to play after performing the Action
        SetPerformAction(() => { VoiceActor.PlayVoiceActor("excellent"); });

        base.Initialize();
    }

    /// <summary>
    /// In this Action we need to override Undo to spawn dummy jars cause the have been removed by Initialize()
    /// Instead of overriding Undo we can use SetUndoAction() and give as argument a function that loads the jar
    /// This function will trigger every time we Undo this Action
    /// </summary>
    public override void Undo()
    {
        GameObject dummyJar = Spawn("Lesson1/Stage0/Action0/MinoanJar");
        dummyJar.name = "MinoanJar";

        base.Undo();
    }
}
