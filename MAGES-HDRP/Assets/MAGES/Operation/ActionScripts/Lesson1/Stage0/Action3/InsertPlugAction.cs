using ovidVR.ActionPrototypes;
using ovidVR.Utilities;
using UnityEngine;

/// <summary>
/// This is an example of an Animation Action.
/// We use this type of Actions in cases we want to insert an object but the object needs to be inserted with an animated movement
/// An example may be the insertion of a wire into a tube. To implement this Action we would need to record the insertion of the wire
/// and then we will push it with our hands to the final position. The movement from the controller is translated into the normalized
/// value of the animation [0-1] 
/// </summary>
public class InsertPlugAction : AnimationAction
{
    GameObject plug;

    /// <summary>
    /// Initialize() method overrides base.Initialize and sets the Animation Action 
    /// </summary>
    public override void Initialize()
    {
        //Helper variable to destroy the animated plug when coming back from a Perform (so not to have two of them)
        plug = GameObject.Find("PlugAnimated(Clone)");
        if(plug)
            DestroyUtilities.RemoteDestroy(plug);

        //The animated insertion of the Plug
        //In this example the user needs to insert the cable to the plug. The movement is recorded into an animation and played along with the movement
        //of the controllers
        SetAnimationPrefab("Lesson1/Stage0/Action3/PlugAnimated");

        //Hologram to pinpoint the correct insertion
        SetHoloObject("Lesson1/Stage0/Action3/PlugHolo");

        //We make sure the KnossosLight is turned off
        GameObject.Find("KnossosLight").GetComponent<Light>().enabled = false;
        
        base.Initialize();
    }

    public override void Perform()
    {
        //After inserting the cable we light up the knossos with
        GameObject.Find("KnossosLight").GetComponent<Light>().enabled = true;

        base.Perform();
    }
}
