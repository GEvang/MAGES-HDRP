using ovidVR.Networking;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class NetworkPrefabConfigurationEx : EditorWindow
{
    #region Variables

    [SerializeField] static string path = @"LessonPrefabs\";
    /// <summary>
    /// path (starting from Assets/ ) to the prefab
    /// </summary>
    [SerializeField] List<string> prefabPaths;
    /// <summary>
    /// cached prefab strings that have the common base path removed
    /// </summary>
    List<string> prefabLabels;
    /// <summary>
    /// Actual prefab object
    /// </summary>
    List<Object> prefabObjects;
    /// <summary>
    /// Current config properties for prefab
    /// </summary>
    List<PrefabConfiguration> prefabConfigs;

    // Filters
    /// <summary>
    /// Filters by synchronization method
    /// </summary>
    ViewOptions viewOptions = ViewOptions.Everything;
    /// <summary>
    /// Filters by prefab path substring
    /// </summary>
    string currentSearchText = "";

    // Bulk Operations
    /// <summary>
    /// Target script for bulk operations, this combined with @bulkTargetTransformOptions and @bulkTargetMultiTransformOptions
    /// make up the customization part of bulk operations
    /// </summary>
    PrefabConfiguration.NetworkSynchronizationScript bulkTargetScript = PrefabConfiguration.NetworkSynchronizationScript.None;
    /// <summary>
    /// Target script parameters, if the script is of the type NetworkSynchronizationScript.Transform
    /// </summary>
    PrefabConfiguration.TransformOptions bulkTargetTransformOptions;
    /// <summary>
    /// Target script parameters, if the script is of the type NetworkSynchronizationScript.MultiTransform
    /// </summary>
    PrefabConfiguration.MultiTransformOptions bulkTargetMultiTransformOptions;

    /// <summary>
    /// Store previously known horizontal rectangles, associated by a unique label, to increase performance in the scroll view
    /// </summary>
    private Dictionary<string, Rect> entryRectCache = new Dictionary<string, Rect>();
    private Vector2 currentScroll = Vector2.zero;

    #region Styling
    private GUIStyle selectedHorizontalStyle = null;
    private GUIStyle selectedLabelStyle = null;
    private GUIStyle selectedEnumPopupStyle = null;
    private Texture2D selectedBackgroundTexture = null;
    #endregion

    #region Prefab Configuration Properties
    /// <summary>
    /// Config properties to be applied for a prefab
    /// </summary>
    class PrefabConfiguration
    {

        public bool selected;
        public NetworkSynchronizationScript networkScript;

        public TransformOptions transformOptions;
        public MultiTransformOptions multiTransformOptions;

        public string lesson;
        public string stage;
        public string rest;

        public class TransformOptions
        {
            public int sendTimesPerSecond;
            public float movementThreshold;
            public float rotationThreshold;
            public OvidVRSyncTransformMode transformMode;
            public TransformOptions(int sendTimesPerSecond, float movementThreshold, float rotationThreshold, OvidVRSyncTransformMode transformMode)
            {
                this.sendTimesPerSecond = sendTimesPerSecond;
                this.movementThreshold = movementThreshold;
                this.rotationThreshold = rotationThreshold;
                this.transformMode = transformMode;
            }

            public TransformOptions()
            {

            }
        }

        public class MultiTransformOptions
        {
            public int transformSyncCount;
            public int sendTimesPerSecond;
            public float rotationThreshold;
            public float movementThreshold;

            public MultiTransformOptions(int transformSyncCount, int sendTimesPerSecond, float rotationThreshold, float movementThreshold)
            {
                this.transformSyncCount = transformSyncCount;
                this.sendTimesPerSecond = sendTimesPerSecond;
                this.rotationThreshold = rotationThreshold;
                this.movementThreshold = movementThreshold;
            }

            public MultiTransformOptions()
            {
            }
        }

        public enum NetworkSynchronizationScript {
            None = 0,
            Transform = 1,
            MultiTransform = 2
        }


        public PrefabConfiguration()
        {
            selected = false;
            networkScript = NetworkSynchronizationScript.Transform;
        }

        public PrefabConfiguration(NetworkSynchronizationScript syncScript)
        {
            selected = false;
            networkScript = syncScript;
        }
    }

    private PrefabConfiguration GetPrefabConfigurationProperties(GameObject gameObject)
    {
        PrefabConfiguration result = new PrefabConfiguration();

        var photonView = gameObject.GetComponent<PhotonView>();
        if (photonView == null)
        {
            result.networkScript = PrefabConfiguration.NetworkSynchronizationScript.None;
            return result;
        }

        if (gameObject.GetComponent<INetworkID>() == null)
        {
            // if the object had no network sync at all, we would've never reached this point
            gameObject.AddComponent<OvidVrNetworkingId>();
        }

        // Up to this point we know that the gameobject _has_ to have either the Transform network scripts
        // or the MultiTransformNetworkScripts, so check that they exist, and default to Transform if they don't

        var singleTransformScript = gameObject.GetComponent<OvidVrSyncTransformPhoton>();
        var multiTransformScript = gameObject.GetComponent<NetMultiTransformPhoton>();

        if (singleTransformScript == null && multiTransformScript == null)
        {
            // default to single transform
            singleTransformScript = gameObject.AddComponent<OvidVrSyncTransformPhoton>();
            result.networkScript = PrefabConfiguration.NetworkSynchronizationScript.Transform;
        }
        else if (singleTransformScript != null && multiTransformScript == null) // having a multi transform script overrides type to MultiTransform
            result.networkScript = PrefabConfiguration.NetworkSynchronizationScript.Transform;
        else if (multiTransformScript != null)
            result.networkScript = PrefabConfiguration.NetworkSynchronizationScript.MultiTransform;

        result.transformOptions = new PrefabConfiguration.TransformOptions();
        result.multiTransformOptions = new PrefabConfiguration.MultiTransformOptions();

        switch (result.networkScript)
        {
            case PrefabConfiguration.NetworkSynchronizationScript.Transform:
                {
                    result.transformOptions.movementThreshold = singleTransformScript.MovementThreshold;
                    result.transformOptions.rotationThreshold = singleTransformScript.RotationThreshold;
                    result.transformOptions.sendTimesPerSecond = singleTransformScript.SendTimesPerSecond;
                    result.transformOptions.transformMode = singleTransformScript.SyncTransformMode;

                }
                break;

            case PrefabConfiguration.NetworkSynchronizationScript.MultiTransform:
                {
                    result.multiTransformOptions.movementThreshold = multiTransformScript.MovementThreshold;
                    result.multiTransformOptions.rotationThreshold = multiTransformScript.RotationThreshold;
                    result.multiTransformOptions.sendTimesPerSecond = multiTransformScript.SendTimesPerSecond;
                    result.multiTransformOptions.transformSyncCount = multiTransformScript.transformSyncCount;
                }
                break;

            default: break;
        }

        return result;
    }


    private void ApplyTransformConfiguration(GameObject gameObject, PrefabConfiguration.TransformOptions options)
    {
        var photonView = gameObject.GetComponent<PhotonView>();
        if (photonView == null)
            photonView = AddDefaultPhotonView(gameObject);

        if (gameObject.GetComponent<INetworkID>() == null)
            gameObject.AddComponent<OvidVrNetworkingId>();

        var singleTransformScript = gameObject.GetComponent<OvidVrSyncTransformPhoton>();
        DestroyImmediate(gameObject.GetComponent<NetMultiTransformPhoton>(), true);

        // unity really complains about this
        if (singleTransformScript != null) DestroyImmediate(singleTransformScript, true);

        singleTransformScript = gameObject.AddComponent<OvidVrSyncTransformPhoton>();
        singleTransformScript.SendTimesPerSecond = options.sendTimesPerSecond;
        singleTransformScript.RotationThreshold = options.rotationThreshold;
        singleTransformScript.MovementThreshold = options.movementThreshold;
        singleTransformScript.SyncTransformMode = options.transformMode;
    }

    private void ApplyMultiTransformConfiguration(GameObject gameObject, PrefabConfiguration.MultiTransformOptions options)
    {
        var photonView = gameObject.GetComponent<PhotonView>();
        if (photonView == null)
            photonView = AddDefaultPhotonView(gameObject);

        if (gameObject.GetComponent<INetworkID>() == null)
            gameObject.AddComponent<OvidVrNetworkingId>();

        // Destroy(gameObject.GetComponent<OvidVrSyncTransformPhoton>());
        var multiTransformScript = gameObject.GetComponent<NetMultiTransformPhoton>();
        if (!multiTransformScript)
            multiTransformScript = gameObject.AddComponent<NetMultiTransformPhoton>();

        multiTransformScript.SendTimesPerSecond = options.sendTimesPerSecond;
        multiTransformScript.RotationThreshold = options.rotationThreshold;
        multiTransformScript.MovementThreshold = options.movementThreshold;
        multiTransformScript.transformSyncCount = options.transformSyncCount;

    }


    private void ApplyNoneConfiguration(GameObject gameObject)
    {
        DestroyImmediate(gameObject.GetComponent<OvidVrSyncTransformPhoton>(), true);
        DestroyImmediate(gameObject.GetComponent<NetMultiTransformPhoton>(), true);
        DestroyImmediate(gameObject.GetComponent<OvidVrNetworkingId>(), true); // maybe not _just_ this type of script?
        DestroyImmediate(gameObject.GetComponent<PhotonView>(), true);
    }
    #endregion

    #region Filtering by sync type
    enum ViewOptions
    {
        None,
        Transform,
        MultiTransform,
        Everything
    }

    ViewOptions GetScriptTargetView(PrefabConfiguration.NetworkSynchronizationScript option)
    {
        switch (option)
        {
            case PrefabConfiguration.NetworkSynchronizationScript.None: return ViewOptions.None;
            case PrefabConfiguration.NetworkSynchronizationScript.Transform: return ViewOptions.Transform;
            case PrefabConfiguration.NetworkSynchronizationScript.MultiTransform: return ViewOptions.MultiTransform;

            default: throw new System.Exception();
        }
    }

    bool IsEntryViewable(ViewOptions options, ViewOptions target)
    {
        return (options == ViewOptions.Everything || (options == target));
    }

    #endregion

    #region Bulk Operations
    private void DoBulkOperationMenuBar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Bulk Op", GUILayout.ExpandWidth(false));

        bulkTargetScript = (PrefabConfiguration.NetworkSynchronizationScript)EditorGUILayout.EnumPopup(bulkTargetScript, EditorStyles.toolbarPopup);

        switch (bulkTargetScript)
        {
            case PrefabConfiguration.NetworkSynchronizationScript.Transform:
                {
                    bulkTargetTransformOptions.sendTimesPerSecond = EditorGUILayout.IntField("Send times / sec", bulkTargetTransformOptions.sendTimesPerSecond, EditorStyles.toolbarTextField);
                    bulkTargetTransformOptions.movementThreshold = EditorGUILayout.FloatField("Movement Threshold", bulkTargetTransformOptions.movementThreshold, EditorStyles.toolbarTextField);
                    bulkTargetTransformOptions.rotationThreshold = EditorGUILayout.FloatField("Rotation Threshold", bulkTargetTransformOptions.rotationThreshold, EditorStyles.toolbarTextField);
                    bulkTargetTransformOptions.transformMode = (OvidVRSyncTransformMode)EditorGUILayout.EnumPopup("Mode", bulkTargetTransformOptions.transformMode, EditorStyles.toolbarPopup);
                }
                break;

            case PrefabConfiguration.NetworkSynchronizationScript.MultiTransform:
                {
                    bulkTargetMultiTransformOptions.sendTimesPerSecond = EditorGUILayout.IntField("Send times / sec", bulkTargetMultiTransformOptions.sendTimesPerSecond, EditorStyles.toolbarTextField);
                    bulkTargetMultiTransformOptions.movementThreshold = EditorGUILayout.FloatField("Movement Threshold", bulkTargetMultiTransformOptions.movementThreshold, EditorStyles.toolbarTextField);
                    bulkTargetMultiTransformOptions.rotationThreshold = EditorGUILayout.FloatField("Rotation Threshold", bulkTargetMultiTransformOptions.rotationThreshold, EditorStyles.toolbarTextField);
                    bulkTargetMultiTransformOptions.transformSyncCount = EditorGUILayout.IntField("Sync count", bulkTargetMultiTransformOptions.transformSyncCount, EditorStyles.toolbarTextField);
                }
                break;

            default: break;
        }

        if (GUILayout.Button("Apply", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
        {
            for (int objectIndex = 0; objectIndex < prefabObjects.Count; ++objectIndex)
            {
                PrefabConfiguration config = prefabConfigs[objectIndex];

                if (!config.selected) continue;

                config.networkScript = bulkTargetScript;


                switch (bulkTargetScript)
                {
                    case PrefabConfiguration.NetworkSynchronizationScript.Transform:
                        {
                            config.transformOptions = bulkTargetTransformOptions;
                        }
                        break;

                    case PrefabConfiguration.NetworkSynchronizationScript.MultiTransform:
                        {
                            config.multiTransformOptions = bulkTargetMultiTransformOptions;
                        }
                        break;
                    default: break;
                }

            }
        }

        GUILayout.EndHorizontal();
    }
    #endregion

    #region Entry List Cache
    private void ClearEntryCache()
    {
        entryRectCache.Clear();
    }

    private bool IsEntryInCache(string entryLabel, out Rect entryRect)
    {

        // If the cache contains our Rect and the rect is outside of the boundaries of the current view, 
        // then we don't need to draw it. But we still need to space it in order for our "cache" to work.
        // Cache being very liberaly used as a term for HashMap that we clean up once in a while 
        if (!entryRectCache.TryGetValue(entryLabel, out entryRect)) return false;

        if (entryRect.yMax < currentScroll.y || currentScroll.y < ((entryRect.yMin - position.height)))
            return true;
        else
            return false;
    }

    private void UpdateEntryInCache(string label)
    {
        if (Event.current.type == EventType.Repaint)
        {
            entryRectCache[label] = GUILayoutUtility.GetLastRect();

        }
    }
    #endregion

    #region Helper Functions
    Color RGBToColor(int r, int g, int b)
    {
        return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f);
    }

    internal PhotonView AddDefaultPhotonView(GameObject gameObject)
    {
        var pv = gameObject.AddComponent<PhotonView>();
        pv.OwnershipTransfer = OwnershipOption.Takeover;
        return pv;
    }

    /// <summary>
    /// The cache that accelerates the scroll view is invalidated when the scroll 
    /// view width is too small to fit every element properly; So determine the
    /// minimum width by doing:
    /// <code>
    /// width += GUILayoutUtility.GetLastRect().width;
    /// </code>
    /// This is just a shorthand for writing that out
    /// </summary>
    internal void PushWidth(ref float current)
    {
        current += GUILayoutUtility.GetLastRect().width;
    }
    #endregion
    #endregion

    [MenuItem("MAGES/Configure Prefabs for Network")]
    static void InitializeEditorWindow()
    {
        NetworkPrefabConfigurationEx t = GetWindow<NetworkPrefabConfigurationEx>();
        t.Initialize();

        t.Show();
    }

    public void Initialize()
    {

        Object[] loadedObjects = Resources.LoadAll(@path);

        prefabPaths = new List<string>();
        prefabObjects = new List<Object>();
        prefabConfigs = new List<PrefabConfiguration>();
        prefabLabels = new List<string>();

        for (long loadedObjectIndex = 0; loadedObjectIndex < loadedObjects.Length; ++loadedObjectIndex)
        {
            Object o = loadedObjects[loadedObjectIndex];

            string assetPath = AssetDatabase.GetAssetPath(o);
            if (!assetPath.EndsWith(".prefab")) continue;

            const string assetPathParent = "Assets/Resources/LessonPrefabs/";
            string uniquePath = assetPath.Remove(0, assetPathParent.Length);
            prefabLabels.Add(uniquePath);
            prefabPaths.Add(assetPath);
            prefabObjects.Add(o);

            var config = GetPrefabConfigurationProperties(o as GameObject);

            // uniquePath has a format of: LessonX/StageX/ActionX/...
            string[] uniquePathSeparated = uniquePath.Split('/');
            config.lesson = uniquePathSeparated[0];
            config.stage = uniquePathSeparated[1];
            config.rest = string.Join("/", uniquePathSeparated.Skip(2));

            prefabConfigs.Add(config);

        }

        bulkTargetTransformOptions = new PrefabConfiguration.TransformOptions(15, 0.005f, 1, OvidVRSyncTransformMode.all);
        bulkTargetMultiTransformOptions = new PrefabConfiguration.MultiTransformOptions(0, 20, 0.005f, 1);

        selectedBackgroundTexture = new Texture2D(1, 1);
        selectedBackgroundTexture.SetPixel(1, 1, RGBToColor(58, 114, 176));
        selectedBackgroundTexture.Apply();

        selectedHorizontalStyle = new GUIStyle {
            margin = { left = -4, right = -4, top = -2, bottom = -2 },
            normal = { textColor = Color.white, background = selectedBackgroundTexture }
        };

        selectedLabelStyle = new GUIStyle(EditorStyles.label);
        selectedLabelStyle.normal.textColor = Color.white;

        selectedEnumPopupStyle = new GUIStyle(EditorStyles.popup);
    }

    private void OnApplyConfigurationButton(bool onlySelected)
    {
        for (int prefabIndex = 0; prefabIndex < prefabObjects.Count; ++prefabIndex)
        {
            GameObject prefab = prefabObjects[prefabIndex] as GameObject;
            string prefabPath = prefabPaths[prefabIndex];
            PrefabConfiguration config = prefabConfigs[prefabIndex];

            if (!config.selected && onlySelected) continue;

            if (config.networkScript == PrefabConfiguration.NetworkSynchronizationScript.Transform)
                ApplyTransformConfiguration(prefab, config.transformOptions);
            else if (config.networkScript == PrefabConfiguration.NetworkSynchronizationScript.MultiTransform)
                ApplyMultiTransformConfiguration(prefab, config.multiTransformOptions);
            else
                ApplyNoneConfiguration(prefab);
        }
    }

    private void OnMenuSelectItems(bool shouldSelect)
    {
        for (int i = 0; i < prefabConfigs.Count; ++i)
        {
            PrefabConfiguration config = prefabConfigs[i];
            var prefabLabel = prefabLabels[i];

            ViewOptions targetView = GetScriptTargetView(config.networkScript);

            if (!IsEntryViewable(viewOptions, targetView) || !prefabLabel.Contains(currentSearchText))
                continue;
            config.selected = shouldSelect;

        }
    }

    private void DoGuiMenubar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Configure for network", GUILayout.ExpandWidth(false));

        if (GUILayout.Button("Select All", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            OnMenuSelectItems(true);
        if (GUILayout.Button("Deselect All", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            OnMenuSelectItems(false);

        ViewOptions prevViewOptions = viewOptions;
        viewOptions = (ViewOptions)EditorGUILayout.EnumPopup(viewOptions, EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));

        if (prevViewOptions != viewOptions)
        {
            ClearEntryCache();
        }

        if (viewOptions.HasFlag(ViewOptions.Everything)) viewOptions = ViewOptions.Everything;

        string prevSearchText = currentSearchText;
        currentSearchText = GUILayout.TextField(currentSearchText, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));

        if (prevSearchText != currentSearchText)
            ClearEntryCache();

        GUILayout.EndHorizontal();

        DoBulkOperationMenuBar();
    }

    
    private void DoGUIListEntry(int objectIndex)
    {
        float width = 0.0f;
        

        string prefabString = prefabPaths[objectIndex];
        string prefabLabel = prefabLabels[objectIndex];
        Object prefab = prefabObjects[objectIndex];
        PrefabConfiguration config = prefabConfigs[objectIndex];

        ViewOptions targetView = GetScriptTargetView(config.networkScript);

        if (!IsEntryViewable(viewOptions, targetView) || !prefabLabel.Contains(currentSearchText))
            return;

        // If the cache contains our Rect and the rect is outside of the boundaries of the current view, 
        // then we don't need to draw it. But we still need to space it in order for our "cache" to work.
        // Cache being very liberaly used as a term for HashMap that we clean up once in a while 
        Rect entryRect;
        if (IsEntryInCache(prefabLabel, out entryRect))
        {
            GUILayout.Space(entryRect.height);
            return;
        }

        GUIStyle style = new GUIStyle();
        style.margin.top = style.margin.bottom = 0; // GUILayout.GetLastRect() doesn't account for style, so wrap it all in a vertical block without margins
        GUILayout.BeginVertical(style);

        GUIStyle labelStyle, enumStyle;
        if (config.selected)
        {
            GUILayout.BeginHorizontal(selectedHorizontalStyle);
            labelStyle = selectedLabelStyle;
            enumStyle = selectedEnumPopupStyle;
        }
        else
        {
            GUILayout.BeginHorizontal();
            labelStyle = EditorStyles.label;
            enumStyle = EditorStyles.popup;

        }

        config.selected = GUILayout.Toggle(config.selected, "");                        PushWidth(ref width);
        GUILayout.Label(config.lesson + "/", labelStyle,GUILayout.ExpandWidth(false));  PushWidth(ref width);
        GUILayout.Label(config.stage + "/", labelStyle, GUILayout.ExpandWidth(false));  PushWidth(ref width);
        GUILayout.Label(config.rest, labelStyle, GUILayout.ExpandWidth(false));         PushWidth(ref width);
        if (GUILayout.Button("Locate"))
        {
            Selection.activeObject = prefab;
            Selection.activeGameObject = prefab as GameObject;
            EditorGUIUtility.PingObject(prefab);
        }
        PushWidth(ref width);

        GUILayout.FlexibleSpace();
        
        var prevConfigNetworkScript = config.networkScript;
        //GUILayout.Label("Synchronization", labelStyle, GUILayout.ExpandWidth(false));
        config.networkScript = (PrefabConfiguration.NetworkSynchronizationScript)EditorGUILayout.EnumPopup(config.networkScript, enumStyle, GUILayout.ExpandWidth(true));
        PushWidth(ref width);

        if (config.networkScript != prevConfigNetworkScript) ClearEntryCache(); // clear cache since our view will not contain the same number of items

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        UpdateEntryInCache(prefabLabel);

        /// If the new width that was determined is larger, then the minimum size
        /// of the editor window should be exactly that. See <c>PushWidth</c>
        Vector2 minSizeNow = minSize;
        if (minSizeNow.x < width)
        {
            minSizeNow.x = width;
            minSize = minSizeNow;
        }
    }

    private void OnGUI()
    {
        DoGuiMenubar();

        GUILayout.BeginVertical();

        currentScroll = GUILayout.BeginScrollView(currentScroll);

        for (int currentObjectIndex = 0; currentObjectIndex < prefabObjects.Count; ++currentObjectIndex)
            DoGUIListEntry(currentObjectIndex);

        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Configure All", GUILayout.ExpandWidth(false)))
            OnApplyConfigurationButton(false);
        if (GUILayout.Button("Configure Selected", GUILayout.ExpandWidth(false)))
            OnApplyConfigurationButton(true);
        GUILayout.EndHorizontal();
    }


}
#endif