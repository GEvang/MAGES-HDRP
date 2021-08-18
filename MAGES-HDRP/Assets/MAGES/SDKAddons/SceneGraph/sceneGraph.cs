/*
 * Scene graph is the main graph to controll LSA of current opperation
 * This graph is created on start and contains the information needed for LSA
 * The graph contains gameObjects of Lessons Stages and Actions to be used
 */
using UnityEngine;
using ovidVR.CustomStoryBoard;
using System;
using ovidVR.CustomStoryBoard.Importer;
using System.Text;
using ovidVR.OperationAnalytics;
using System.Collections;
using System.Collections.Generic;
using ovidVR.AnalyticsEngine;
using ovidVR.UIManagement;
using ovidVR.Utilities;

namespace ovidVR.sceneGraphSpace
{
    public class LoginLogs : MonoBehaviour
    {
        public bool prevLogin = false;
        public bool loginOnce = false;
        private static LoginLogs instance = null;

        public static LoginLogs Get
        {
            get
            {
                if (instance == null)
                {
                    GameObject g = new GameObject("LoginLogs");

                    instance = g.AddComponent<LoginLogs>();
                    DontDestroyOnLoad(instance);
                }
                return instance;
            }
        }
    }

    public class sceneGraph : MonoBehaviour
    {

        Func<bool> licenseFunc;

        GameObject licenceRequestGameobject;
        LicenseRequest licenceRequestScript;

        public static License.LicenseType GetLicenseType()
        {
            return StoryBoard.licenseType;
        }


        [SerializeField]
        private string 
            OperationXML = null,
            AlternativeLessonsXML = null, 
            AlternativeStagesXML = null,
            AlternativeActionsXML = null;

        private StringBuilder _version;

        /// <summary>
        /// Start() to initialize the graph when the operation starts
        /// </summary>
        private void Awake()
        {
            try
            {
#if !UNITY_EDITOR

                Configuration SceneManagement = GameObject.Find("SCENE_MANAGEMENT").GetComponent<Configuration>();

                if (SceneManagement.LoginUI == null)
                    licenceRequestGameobject = PrefabImporter.SpawnGenericPrefab("MAGESres/UI/License/UILicenseRequestSSO");
                else
                    licenceRequestGameobject = Instantiate(SceneManagement.LoginUI);

                licenceRequestScript = licenceRequestGameobject.GetComponent<LicenseRequest>();
                
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError("Something went wrong " + e.StackTrace);
            }
        }

        void Start()
        {
            Configuration SceneManagement = GameObject.Find("SCENE_MANAGEMENT").GetComponent<Configuration>();

            // Note: The order is important.
            // Here you can specify certain configuration properties that concern the following: Paths, ApplicationSettings.
            Configuration.ProductCode = SceneManagement.productCode; // This should only change on build! On Unity.Editor it must remain as it is
            Configuration.Quality = QualityConfig.High;
            Configuration.Region = Region.Auto;
            Configuration.Difficulty = UserAccountManager.Difficulty.Easy;
            Configuration.SetXmlNames(OperationXML,
                AlternativeLessonsXML,
                AlternativeStagesXML,
                AlternativeActionsXML);
            Configuration.ConfigurePreInitialization(); // Important

            initializeSceneGraph();
        }

        private void OnDisable()
        {
            licenseFunc = null;
            CancelInvoke();
            StopAllCoroutines();
        }

        /// <summary>
        /// A helper funtion to properly set the hierarchy of graph
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        public void setChildOfParent(GameObject child, GameObject parent)
        {
            child.transform.parent = parent.transform;
        }

        /// <summary>
        /// The initalization of sceneGraph
        /// To generate the graph we read from the XML file and traverse the LSA Lists to
        /// generate the graph
        /// </summary>
        public void initializeSceneGraph()
        {
            StartCoroutine(LoadLSA());
        }


        IEnumerator LoadLSA()
        {

#if UNITY_ANDROID
            _version = new StringBuilder("1.1.0");
            if (Configuration.Get.UserLogin){
                 if(licenceRequestScript!= null)
                        licenceRequestScript.CompareVersions(_version.ToString());
            }
#endif
              
            InterfaceManagement.Get.InterfaceRaycastActivation(true);
            licenseFunc = new Func<bool>(() => ((LoginLogs.Get.prevLogin == true) || LicenseRequest.HasLicense() == true));
            yield return new WaitUntil(licenseFunc);
            if(licenceRequestScript != null && licenceRequestScript.cameraRig != null)
            {
                licenceRequestScript.cameraRig.enabled = true;
            }
            Destroy(licenceRequestGameobject);


            StoryBoard.ImportStoryBoard(StoryBoardImporter.ImportMode.Operation);

            // We must pause execution until StoryBoard is loaded correctly!
            yield return new WaitUntil(() => StoryBoard.loading == false);

            print("License type: " + GetLicenseType());

            // Configuration properties that regard post Scenegraph initilization such as Analytics, Users go here.
            //Configuration.User = new ApplicationUser
            //{
            //    id = "app-user",
            //    firstName = "User",
            //    lastName = "User",
            //    country = "Devland",
            //    userName = "proplayer"
            //};
            //Configuration.UserPassword = ("default");

            // For uploading analytics specify your endpoint and add any extra form fields or headers required from your service.
            Configuration.OnlineURL = "";
            //Configuration.OnlineURL = "";
            Configuration.FormFields = new List<AnalyticsExporter.FormField>
            {
                new AnalyticsExporter.FormField { key = "Username", value = UserAccountManager.Get.GetUsername() },
                new AnalyticsExporter.FormField { key = "Operation", value = "SDK" },
            };
            Configuration.HeaderKeys = new List<AnalyticsExporter.HeaderKey>
            {
                new AnalyticsExporter.HeaderKey { key = "Authorization", value = "Bearer " }
            };
            Configuration.ConfigurePostInitialization(); // Important

            Operation.Get.SetOperationName(Configuration.ProductCode);

            if (Configuration.Get.UserLogin)
            {
                LoginLogs.Get.prevLogin = true;
                LoginLogs.Get.loginOnce = true;
            }

            bool isDemo = false, isDemoFinished = false;
            if (GetLicenseType() == License.LicenseType.Demo)
                isDemo = true;

            bool accessLesson = false;

            if (GameObject.Find("UILeaderboard"))
                GameObject.Find("UILeaderboard").GetComponent<Canvas>().sortingOrder = 0;


            Dictionary<int,List<ScoringFactor>> currentActionAnalytics;
            int multiplier = 1;
            string customScoringFactor="";
            foreach (Lessons lesson in StoryBoard.TKRstoryBoard)
            {
                GameObject lessonNode = new GameObject();
                lessonNode.AddComponent<Lesson>().SetLessonName(lesson.lessonName);
                lessonNode.name = lessonNode.GetComponent<Lesson>().GetLessonName();

                foreach (Stages stage in lesson.allStages)
                {
                    GameObject stageNode = new GameObject();
                    stageNode.AddComponent<Stage>().SetStageName(stage.stageName);
                    stageNode.name = stageNode.GetComponent<Stage>().GetStageName();

                    foreach (Actions action in stage.allActions)
                    {
                        if (isDemoFinished)
                            continue;

                        GameObject actionNode = new GameObject();
                        currentActionAnalytics = AnalyticsRuntimeImporter.ImportAnalyticsForAction(ref multiplier,ref customScoringFactor, action.className);
                        
                        // l = Last Demo Action (rest of strings, n: No, y: Yes) - force it to become OperationEnd
                        if (isDemo && action.IsDemoApplicable == "l")
                        {
                            actionNode.name = "Operation End";
                            IAction classInst = (IAction)actionNode.AddComponent(typeof(OperationEndAction));
                            classInst.ActionNode = actionNode;
                            classInst.ActionName = actionNode.name;

                            ActionProperties actionProperties = actionNode.AddComponent<ActionProperties>();
                            actionProperties.actionType = ActionType.Simple;
                            actionProperties.averageActionTime = 10f;
                            actionProperties.isDemoApplicable = "y";

                            setChildOfParent(actionNode, stageNode);

                            isDemoFinished = true;
                            continue;
                        }

                        // Check if action is applicable to make lesson accessable
                        if (isDemo && action.IsDemoApplicable == "y")
                        {
                            accessLesson = true;
                        }

                        actionNode.name = action.actionName;

                        var classType = Type.GetType(action.className);
                        
                        try
                        {
                            IAction classInst = (IAction)actionNode.AddComponent(classType);
                            if (classInst == null)
                            {
                                throw new Exception($"Action script {classInst.ActionName} not found!");
                            }
                            classInst.ActionNode = actionNode;
                            classInst.ActionName = actionNode.name;

                            // Add Action Properties to actionNode
                            ActionProperties actionProperties = actionNode.AddComponent<ActionProperties>();
                            actionProperties.actionType = action.actionType;
                            actionProperties.averageActionTime = action.averageActionTime;
                            actionProperties.isDemoApplicable = action.IsDemoApplicable;

                            //Action Analytics Specific

                            actionProperties.scoringFactors = currentActionAnalytics;
                            actionProperties.multiplier = multiplier;
                            if (customScoringFactor != "")
                            {
                                var scoreType = Type.GetType(customScoringFactor);
                                //actionProperties.scoringFactors.Add((ScoringFactor)scoreType.GetConstructor(Type.EmptyTypes).Invoke(null));
                            }

                            
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Exception while Importing Action in Scenegraph: " + ex.Message);
                        }

                        setChildOfParent(actionNode, stageNode);

                    } // foreach action
                    setChildOfParent(stageNode, lessonNode);
                } // foreach stage
                setChildOfParent(lessonNode, Operation.Get.GetOperationNode());

                // Set access to Lesson
                if (isDemo) lessonNode.GetComponent<Lesson>().accessLesson = accessLesson;
                accessLesson = false;
            } // foreach lesson


            //Load Alternative paths
            AlternativePathImporter.Get.InitializeAlternativePathBucket(AlternativeLessonsXML, AlternativeStagesXML, AlternativeActionsXML);

            switch (Configuration.Difficulty)
            {
                case UserAccountManager.Difficulty.Easy:
                    {
                        Operation.Get.SetHolograms(true);
                        Operation.Get.SetOperationDifficulty(Difficulty.Easy);
                        break;
                    }
                case UserAccountManager.Difficulty.Medium:
                    {
                        Operation.Get.SetHolograms(false);
                        Operation.Get.SetOperationDifficulty(Difficulty.Medium);
                        break;
                    }
                case UserAccountManager.Difficulty.Hard:
                    {
                        Operation.Get.SetHolograms(false);
                        Operation.Get.SetOperationDifficulty(Difficulty.Hard);
                        break;
                    }
                default:
                    {
                        Operation.Get.SetHolograms(true);
                        Operation.Get.SetOperationDifficulty(Difficulty.Easy);
                        break;
                    }
            }

            // Initialize UserPathTracer (each action the user performs is stored there)
            UserPathTracer.Get.StartUserPathTracer();

            Operation.Get.LoadLSA();


            // Initialize Monitor Displaying the Action Status. It uses info from UserPathTracer
            // So it must be initialized after that
            GameObject uiMonitor = GameObject.Find("UI_GamificationMonitor");
            if (uiMonitor && uiMonitor.GetComponent<PointSystemManager>())
                uiMonitor.GetComponent<PointSystemManager>().StartPointSystem();

        }
    }
}
