using ovidVR.ActionPrototypes;
using ovidVR.sceneGraphSpace;
using ovidVR.UIManagement;
using System;
using UnityEngine;

/// <summary>
/// This is an example of Parallel Action
/// Both Actions (AssembleKnossosPartOfAction, AssembleSponzaPartOfAction) will initialized automatically and run in parallel
/// </summary>
public class AssembleKnossosORSponzaAction : ParallelAction {

    /// <summary>
    /// Combined Action consists of several "sub-Actions" that run one after another
    /// In this Combined Action we have InsertAction->InsertAction->ToolAction
    /// </summary>
    public class AssembleKnossosPartOfAction : CombinedAction
    {
        public override void Initialize()
        {
            AnalyticsManager.AddScoringFactor<ForceScoringFactor>(2);
            
            //InsertAction sub-Action
            InsertAction insertFrontGateAction = gameObject.AddComponent<InsertAction>();
            insertFrontGateAction.SetInsertPrefab("Lesson0/Stage1/Action0/FrontPartInteractable",
                                                  "Lesson0/Stage1/Action0/FrontPartFinal");
            insertFrontGateAction.SetHoloObject("Lesson0/Stage1/Action0/Hologram/FrontPartHologram");

            //--------------------------------------------------------------------------------------------
            //InsertAction sub - Action
            InsertAction insertBackGateAction = gameObject.AddComponent<InsertAction>();
            insertBackGateAction.SetInsertPrefab("Lesson0/Stage1/Action0/BackPartInteractable",
                                                 "Lesson0/Stage1/Action0/BackPartFinal");
            insertBackGateAction.SetHoloObject("Lesson0/Stage1/Action0/Hologram/BackPartHologram");
            //--------------------------------------------------------------------------------------------
            //ToolAction sub - Action
            ToolAction hitWithMallet = gameObject.AddComponent<ToolAction>();
            hitWithMallet.SetToolActionPrefab("Lesson0/Stage1/Action0/BackPartHitMallet", ovidVR.toolManager.tool.ToolsEnum.Mallet);
            hitWithMallet.SetHoloObject("Lesson0/Stage1/Action0/Hologram/MalletHologramL0S1A0");

            InsertIActions(insertFrontGateAction, insertBackGateAction, hitWithMallet);


            base.Initialize();
        }
    }

    /// <summary>
    /// Example of Insert Action
    /// </summary>
    public class AssembleSponzaPartOfAction : InsertAction
    {
        /// <summary>
        /// Initialize method overrides base.Initialize() and sets the prefab user will insert
        /// </summary>
        public override void Initialize()
        {
            //Set Prefab to insert
            //First Argument: Interactable prefab
            //Second Argument: Final prefab
            //Third Argument: Hologram
            SetInsertPrefab("Lesson0/Stage1/Action0/SponzaInteractable", "Lesson0/Stage1/Action0/SponzaFinal");
            SetHoloObject("Lesson0/Stage1/Action0/Hologram/HologramSponzaFinal");
            base.Initialize();
        }
    }

    /// <summary>
    /// Initialize() function for Parallel Action
    /// Sets the alt path to triggen when performing each Action
    /// </summary>
    public override void Initialize()
    {
        AssembleKnossosPartOfAction assembleKnossosPartOfAction = this.gameObject.AddComponent<AssembleKnossosPartOfAction>();

        AssembleSponzaPartOfAction assembleSponzaPartOfAction = this.gameObject.AddComponent<AssembleSponzaPartOfAction>();
        //Set different event Manager for second Action
        assembleSponzaPartOfAction.SetEventListener("SponzaPart");

        InsertIActionToDictionary(-1, assembleKnossosPartOfAction);
        InsertIActionToDictionary(0, assembleSponzaPartOfAction);

        //UIManagement.PlaySpeech(LanguageSpeech.Decision);


        base.Initialize();
    }
}
