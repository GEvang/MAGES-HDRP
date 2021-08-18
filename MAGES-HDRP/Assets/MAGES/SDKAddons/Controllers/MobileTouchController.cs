/**
 * Implements critical methods for MobileDevice
 * */

using ovidVR.sceneGraphSpace;
using UnityEngine;
using ovidVR.Utilities.prefabSpawnManager.prefabSpawnConstructor;
using ovidVR.CustomEventManager;
using ovidVR.toolManager.tool;

namespace ovidVR.GameController
{
    public class MobileTouchController : OvidVRControllerClass
    {
        GUIStyle gUIStyle;
        Texture2D normal;
        Texture2D hover;
        GUIStyle style = new GUIStyle();

        private static readonly float PanSpeed = .5f;
        private static readonly float ZoomSpeedTouch = .1f;
        private static readonly float ZoomSpeedMouse = 15f;

        private static readonly float[] BoundsX = new float[] { -10f, 5f };
        private static readonly float[] BoundsZ = new float[] { -18f, -4f };
        private static readonly float[] ZoomBounds = new float[] { 10f, 85f };

        private Camera cam;

        private Vector3 lastPanPosition;
        private int panFingerId; // Touch mode only

        private bool wasZoomingLastFrame; // Touch mode only
        private Vector2[] lastZoomPositions; // Touch mode only

        ToolsEnum tools = (ToolsEnum)0x25876;
        string selectedJig = null;
        private Matrix4x4 matrix;

        void Awake()
        {
            cam = Camera.main;
            leftController = GetComponent<OvidVRControllerClass>().leftController;
            rightController = GetComponent<OvidVRControllerClass>().rightController;

            style.fontSize = 30;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.UpperCenter;
            style.normal.textColor = Color.black;
        }

        private void Start()
        {
            Operation.Get.Perform();
            gUIStyle = new GUIStyle();
            gUIStyle.fontSize = (Screen.width / (Screen.height + Screen.width)) * 18;
            gUIStyle.normal.textColor = Color.black;
            gUIStyle.hover.textColor = Color.blue;
            gUIStyle.fontStyle = FontStyle.Bold;
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.wrapMode = TextureWrapMode.Repeat;
            texture2D.SetPixel(0, 0, Color.green);
            texture2D.Apply();

            gUIStyle.alignment = TextAnchor.MiddleCenter;
            normal = Resources.Load(@"MAGESres\Operation\ImportExportData\Images\squareForLessons_BLACK") as Texture2D;
            hover = Resources.Load(@"MAGESres\Operation\ImportExportData\Images\squareForLessons_WHITE") as Texture2D;
            gUIStyle.normal.background = normal;
            gUIStyle.hover.background = hover;
            //gUIStyle.onHover.background = hover;

        }

        void Update()
        {
            gUIStyle.fontSize = ((Screen.width + Screen.height) / (800 + 600)) * 12;

            if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                HandleTouch();
            }
            else
            {
                HandleMouse();
            }

            if (Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    //Debug.Log("You selected the " + hit.transform.name); // ensure you picked right object

                    if (hit.transform.gameObject.GetComponent<UIButton>() != null)
                    {
                        hit.transform.gameObject.GetComponent<UIButton>().buttonFunction.Invoke();

                    }
                }
            }
            Camera.main.transform.LookAt(GameObject.Find("LookTarget").transform);

        }

        void HandleTouch()
        {
            switch (Input.touchCount)
            {
                case 1: // Panning
                    wasZoomingLastFrame = false;

                    // If the touch began, capture its position and its finger ID.
                    // Otherwise, if the finger ID of the touch doesn't match, skip it.
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        lastPanPosition = touch.position;
                        panFingerId = touch.fingerId;
                    }
                    else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                    {
                        PanCamera(touch.position);
                    }
                    break;

                case 2: // Zooming
                    Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                    if (!wasZoomingLastFrame)
                    {
                        lastZoomPositions = newPositions;
                        wasZoomingLastFrame = true;
                    }
                    else
                    {
                        // Zoom based on the distance between the new positions compared to the 
                        // distance between the previous positions.
                        float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                        float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                        float offset = newDistance - oldDistance;

                        ZoomCamera(offset, ZoomSpeedTouch);

                        lastZoomPositions = newPositions;
                    }
                    break;

                default:
                    wasZoomingLastFrame = false;
                    break;
            }
        }

        void HandleMouse()
        {
            // On mouse down, capture it's position.
            // Otherwise, if the mouse is still down, pan the camera.
            if (Input.GetMouseButtonDown(1))
            {
                lastPanPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(1))
            {
                PanCamera(Input.mousePosition);
            }

            // Check for scrolling to zoom the camera
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            HandleActions();

            ZoomCamera(scroll, ZoomSpeedMouse);
        }

        void PanCamera(Vector3 newPanPosition)
        {
            // Determine how much to move the camera
            Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
            Vector3 move = new Vector3(offset.x * PanSpeed, offset.y * PanSpeed, 0);

            // Perform the movement
            Camera.main.transform.Translate(move, Space.Self);

            // Ensure the camera remains within bounds.
            Vector3 pos = Camera.main.transform.position;
            pos.x = Mathf.Clamp(Camera.main.transform.position.x, BoundsX[0], BoundsX[1]);
            pos.z = Mathf.Clamp(Camera.main.transform.position.z, BoundsZ[0], BoundsZ[1]);
            Camera.main.transform.position = pos;
            // Cache the position
            lastPanPosition = newPanPosition;
        }

        void ZoomCamera(float offset, float speed)
        {
            if (offset == 0)
            {
                return;
            }

            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
        }

        void OnGUI()
        {


            if (GUI.Button((new Rect(Screen.width * 0.5f / 5f, Screen.height * 4f / 5f, Screen.width / 8f, Screen.height / 12f)), "Prev Action", gUIStyle))
                Operation.Get.Undo();
            if (GUI.Button((new Rect(Screen.width * 4f / 5f, Screen.height * 4f / 5f, Screen.width / 8f, Screen.height / 12f)), "Next Action", gUIStyle))
                Operation.Get.Perform();


            GUI.Label(ResizeGUI(new Rect(Screen.width * (0.6f / 5f), Screen.height * (0.1f / 5f), Screen.width / 5, Screen.height / 5)), ScenegraphTraverse.GetCurrentAction().name, style);
        }

        Rect ResizeGUI(Rect _rect)
        {
            float FilScreenWidth = _rect.width / 800;
            float rectWidth = FilScreenWidth * Screen.width;
            float FilScreenHeight = _rect.height / 600;
            float rectHeight = FilScreenHeight * Screen.height;
            float rectX = (_rect.x / 800) * Screen.width;
            float rectY = (_rect.y / 600) * Screen.height;

            style.fontSize = ((Screen.width + Screen.height) / (800 + 600)) * 12;

            return new Rect(rectX, rectY, rectWidth, rectHeight);
        }

        void HandleActions()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask("TriggerColliderLesson"), QueryTriggerInteraction.Collide))
                {
                    //Debug.Log("You selected the " + hit.transform.name); // ensure you picked right object

                    if (hit.transform.gameObject.GetComponent<CollisionHitPrefabConstructor>() != null && tools == ToolsEnum.Mallet)
                    {
                        CollisionHitPrefabConstructor c = hit.transform.gameObject.GetComponent<CollisionHitPrefabConstructor>();
                        c.FinalizeByNetwork();
                    }

                }
            }
            else if (Input.GetKey(KeyCode.Mouse0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask("TriggerColliderLesson"), QueryTriggerInteraction.Collide))
                {
                    //Debug.Log("You selected the " + hit.transform.name); // ensure you picked right object

                    if (hit.collider.gameObject.GetComponent<ToolTriggerCollider>() != null)
                    {
                        ToolColliderPrefabConstructor t = hit.transform.gameObject.GetComponent<ToolColliderPrefabConstructor>();
                        foreach (ToolAndTime tnt in t.ToolsList)
                        {
                            if (tnt.useTool == tools)
                            {
                                t.CollisionEvent(hit.collider.gameObject);
                            }
                        }
                    }
                    else if (hit.collider.gameObject.GetComponent<UseColliderPrefabConstructor>() != null)
                    {
                        UseColliderPrefabConstructor u = hit.collider.gameObject.GetComponent<UseColliderPrefabConstructor>();
                        u.FinalizeByNetwork();
                    }
                    else if (hit.transform.gameObject.GetComponent<RemoveWithToolsCostructor>() != null)
                    {
                        RemoveWithToolsCostructor i = hit.transform.gameObject.GetComponent<RemoveWithToolsCostructor>();
                        if (!i.GetComponent<MouseMovement>())
                            i.gameObject.AddComponent<MouseMovement>();

                    }

                }
                if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask("GrabbablePrefabs"), QueryTriggerInteraction.Collide))
                {
                    //Debug.Log("You selected the " + hit.transform.name); // ensure you picked right object

                    if (hit.transform.gameObject.GetComponent<PrefabLerpPlacement>() != null)
                    {
                        PrefabLerpPlacement p = hit.transform.gameObject.GetComponent<PrefabLerpPlacement>();
                        foreach (GameObject g in p.interactablePrefabs)
                        {
                            if (g.name + "(Clone)" == selectedJig)
                            {
                                p.StartCoroutine("PrefabLerpingTimer");
                                p.FinalizePrefabActionByNetWork();
                                EventManager.TriggerEvent(ScenegraphTraverse.GetCurrentAction().name);
                            }
                        }
                    }
                    else if (hit.transform.gameObject.GetComponent<InteractablePrefabConstructor>() != null)
                    {
                        InteractablePrefabConstructor i = hit.transform.gameObject.GetComponent<InteractablePrefabConstructor>();
                        if (i.prefabInteractableType == PrefabInteractableType.Remove)
                        {
                            if (!i.GetComponent<MouseMovement>())
                                i.gameObject.AddComponent<MouseMovement>();

                        }
                    }


                }
            }

        }
    }
}