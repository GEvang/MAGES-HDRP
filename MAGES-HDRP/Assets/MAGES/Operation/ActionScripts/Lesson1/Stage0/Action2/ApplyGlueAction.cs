using ovidVR.ActionPrototypes;

/// <summary>
/// This is an example of a Combined Action. We use the combine Action in cases we want multiple short Actions to be in the same Action script
/// In this Combined Action we set three sub-Actions : a) Insert b) Pump c) Remove
/// The Combined Action performs when all sub-Actions are performed
/// </summary>
public class ApplyGlueAction : CombinedAction
{
    /// <summary>
    /// Initialize() method overrides base.Initialize and sets the sub-Actions 
    /// </summary>
    public override void Initialize()
    {
        //You will see below the Actions have the same functionality as if they were written in a unique file each one
        //The only difference is the syntax of the Actions, functionality remains the same


        // (Insert Action) Attach syringe to position
        InsertAction attachSyringe = gameObject.AddComponent<InsertAction>();
        {
            attachSyringe.SetInsertPrefab("Lesson1/Stage0/Action2/SyringeInteractable", "Lesson1/Stage0/Action2/SyringeFinal");
            attachSyringe.SetHoloObject("Lesson1/Stage0/Action2/Hologram/SyringeHolo");
        }
        //--------------------------------------------------------------------------------------------
        // (Pump Action) Use the syringe to apply glue on the area
        //The Pump Action is used in situation we want to press/squeeze a 3D model to perform the Action
        //In this case the user needs to press the syringe to apply glue to the selected spot
        //To perform the Action hover the syringe with your controller and press the trigger button
        PumpAction fillSheath = gameObject.AddComponent<PumpAction>();
        {
            //Sets the pump prefabs
            //To set your own Pump prefab you can take this one as a template
            fillSheath.SetPumpPrefab("Lesson1/Stage0/Action2/SyringePump");

            //Pump Actions support multiple pumps within the same Action
            //fillSheath.SetPumpPrefab("Lesson1/Stage0/Action2/SyringePump2");
            //The developer can set multiple pumps to be completed
            //In case that multiple pump prefabs are set the Action will perform when all of the pump prefabs are pressed (sequence does not matter)
        }
        //--------------------------------------------------------------------------------------------
        // (Remove Action) Remove syringe
        RemoveAction removeSyringe = gameObject.AddComponent<RemoveAction>();
        {
            removeSyringe.SetRemovePrefab("Lesson1/Stage0/Action2/SyringeRemove");
        }


        //This function is mandatory for the Combined Actions to set the sequence of the sub-Action
        //In this way se set the order of the sub-Actions
        InsertIActions(attachSyringe, fillSheath, removeSyringe);

        base.Initialize();
    }

}
