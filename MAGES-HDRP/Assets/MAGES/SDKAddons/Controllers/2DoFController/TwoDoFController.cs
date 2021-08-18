using ovidVR.GameController;
using OvidVRPhysX;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR;

/// <summary>
/// 2-DoF Controller
/// 
/// SETUP
/// Currently, enabling the 2dof controller through MAGES creates a camera rig
/// with the SmoothMouseLook.cs script attached to it, so we have to:
///     1. Remove that script for the gameobject (Camera - Eye)
///     2. Attach this script to it, and finally
///     3. Set the VisGrid and VisSphere fields to the corresponding
///     prefabs in the 2DoFController folder.
/// 
/// CONTROLS
/// There are 3 distinguished control modes: Body, RightHand and LeftHand.
/// You can switch between them by using F1, F2 and F3 accordingly.
/// 
/// For RightHand and LeftHand: There are 2 manipulation modes, Translation and
/// Rotation. You can toggle the current one using TAB.
/// 
/// LeftHand / RightHand mode:
/// Mouse L - Pinch current hand
/// Mouse R - Grip  current hand
///     For the Hand Translation mode:
///     Mouse       - Translate hand along X-Z plane
///     Ctrl+Mouse  - Translate hand along X-Y plane
/// 
///     For the Hand Rotation mode:
///     Mouse       - Rotate Yaw and Pitch
///     Ctrl+Mouse  - Rotate Roll
/// 
/// For the body mode:
/// Mouse              - Look around
/// 
/// ACCELERATORS
/// For ease of use, there are ways to peform common actions quickly:
/// WASD               - Move
/// Ctrl + Mouse Left  - Quick Grab (Right Hand)
/// Ctrl + Mouse Right - Quick Grab (Left Hand)
/// Space              - Reset position and orientation of hands
/// </summary>
public class TwoDoFController : MonoBehaviour
{
    public static TwoDoFController Instance { get; internal set; } = null;
    internal ControlMode currentControlMode = ControlMode.Body;
    internal Camera camera2dof;

    #region Mouse Look
    public Vector2 mouseSensitivity = new Vector2(1,1);
    public float mouseSmoothness = 3.0f;
    internal bool mouseIsLocked = false;
    internal MouseOrientator mouseLookOrientator;
    internal CharacterController controller;
    #endregion
    #region Body
    public float bodySpeed = 3.0f;
    public Vector3 defaultHandOffset = new Vector3(0.3f, -0.1f, 0.4f);
    internal Quaternion defaultLeftHandOrientation = Quaternion.AngleAxis(70, Vector3.up) * Quaternion.AngleAxis(0, Vector3.forward);
    internal Quaternion defaultLeftHandRollOrientation = Quaternion.AngleAxis(0, -Vector3.right);
    internal Quaternion defaultRightHandOrientation = Quaternion.AngleAxis(280, Vector3.up) * Quaternion.AngleAxis(175, -Vector3.forward);
    internal Quaternion defaultRightHandRollOrientation = Quaternion.AngleAxis(0, Vector3.right);
    #endregion
    #region Hands
    public Hand leftHand = new Hand();
    public Hand rightHand = new Hand();
    public Vector3 visGridOffset = new Vector3(-0.13f, -0.13f, 0);
    public GameObject VisGrid = null;
    public GameObject VisSphere = null;
    public float grabRadius = 3.5f;
    public float buttonAnimationSpeed = 17.2f;
    internal HandControlMode currentHandControlMode = HandControlMode.Translation;
    internal HandTranslationMode currentHandTranslationMode = HandTranslationMode.XZPlane;
    internal HandRotationMode currentHandRotationMode = HandRotationMode.YawPitch;
    internal VisualizationGrid activeGrid = null;
    internal List<VisualizationGrid> grids = new List<VisualizationGrid>();

    internal List<VisualizationSphere.Ring> activeRings = new List<VisualizationSphere.Ring>();
    internal List<VisualizationSphere.Ring> allRings = new List<VisualizationSphere.Ring>();

    public Color ringPitchColor = new Color(0.7f, 0,0);
    public Color ringYawColor = new Color(0, 0.7f, 0);
    public Color ringRollColor = new Color(0,0,0.7f);

    public Vector3 visSphereOffset = Vector3.zero;
    #endregion

    #region Control Modes
    internal enum ControlMode
    {
        Body,
        LeftHand,
        RightHand
    }

    internal enum HandControlMode
    {
        Translation,
        Rotation
    }

    internal enum HandTranslationMode
    {
        XZPlane,
        XYPlane
    }

    internal enum HandRotationMode
    {
        YawPitch,
        Roll
    }
    #endregion
    public class VisualizationGrid
    {
        public Vector3 offset;
        public Quaternion InternalOrientation { get; set; }
        public Quaternion Rotation
        {
            get
            {
                return grid.transform.rotation;
            }
            set
            {
                grid.transform.rotation = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                return grid.transform.position;
            }

            set
            {
                grid.transform.position = value;
            }
        }

        internal GameObject grid;
        internal Material mat;
        internal Renderer renderer;
        internal int propX, propY, propFade;
        public VisualizationGrid(GameObject _grid)
        {
            grid = _grid;
            renderer = grid.GetComponent<Renderer>();
            mat = renderer.material;
            propY = Shader.PropertyToID("_OffsetY");
            propX = Shader.PropertyToID("_OffsetX");
            propFade = Shader.PropertyToID("_TotalFade");

            mat.SetFloat(propFade, 0);
        }

        public void SetOffset(Vector2 offset)
        {
            mat.SetFloat(propX, offset.x);
            mat.SetFloat(propY, offset.y);
        }

        public void SetFade(float v)
        {
            mat.SetFloat(propFade, v);

            if (v == 0.0f)
                renderer.enabled = false;
            else
                renderer.enabled = true;
        }

        public void Update(Vector3 by, Quaternion orientation, bool isXZ)
        {
            if (isXZ)
            {
                Position = by + offset;
                Vector3 euler = Vector3.Scale(orientation.eulerAngles, new Vector3(0, 1, 0));
                Quaternion yRot = Quaternion.Euler(euler);

                Vector3 myEuler = Vector3.Scale(InternalOrientation.eulerAngles, new Vector3(1, 1, 0));
                Rotation = yRot * Quaternion.Euler(myEuler);
            }
        }
    }
    public class VisualizationSphere
    {
        public class Ring
        {
            private LineRenderer line;
            public Material Material { get; private set; }

            internal bool Enabled
            {
                get { return line.enabled; }
                set { line.enabled = value; }
            }

            internal Ring(GameObject go)
            {
                line = go.GetComponent<LineRenderer>();
                Material = line.material;
            }
        }

        internal Ring yawRing, pitchRing, rollRing;
        internal GameObject sphere;
        internal Vector3 offset;

        internal int offsetProperty, radiusProperty, colorProperty;
        public VisualizationSphere(GameObject sphere)
        {
            this.sphere = sphere;
            offsetProperty = Shader.PropertyToID("_HighlightSectionOffset");
            radiusProperty = Shader.PropertyToID("_HighlightSectionRadius");
            colorProperty = Shader.PropertyToID("_Color");

            GameObject yaw = sphere.transform.GetChild(0).gameObject;
            GameObject pitch = sphere.transform.GetChild(1).gameObject;
            GameObject roll = sphere.transform.GetChild(2).gameObject;

            yawRing = new Ring(yaw);
            pitchRing = new Ring(pitch);
            rollRing = new Ring(roll);

            offset = Vector3.zero;
        }

        public void SetYawPitch(float yaw, float pitch, float radius)
        {
            yawRing.Material.SetFloat(offsetProperty, yaw);
            yawRing.Material.SetFloat(radiusProperty, radius);
            pitchRing.Material.SetFloat(offsetProperty, pitch);
            pitchRing.Material.SetFloat(radiusProperty, radius);
        }

        public void SetRoll(float roll, float radius)
        {
            rollRing.Material.SetFloat(offsetProperty, roll);
            rollRing.Material.SetFloat(radiusProperty, radius);
        }

        public void SetColor(Color pitch, Color yaw, Color roll)
        {
            pitchRing.Material.SetColor("_Color", pitch);
            yawRing.Material.SetColor("_Color", yaw);
            rollRing.Material.SetColor("_Color", roll);
        }

        public void Update(Vector3 position, Quaternion orientation, Vector3 offset)
        {
            this.offset = offset;
            sphere.transform.position = (position) + orientation * offset;
            sphere.transform.rotation = orientation;
        }
    }
    public class Hand
    {
        public bool Trigger { get; set; }
        public bool Grip { get; set; }

        public float TriggerStrength { get; internal set; } = 0.0f;
        public float GripStrength { get; internal set; } = 0.0f;

        public HandLatch latch = null;
        public MouseTranslator xzTranslator;
        public VisualizationGrid xzGrid;
        public MouseTranslator xyTranslator;
        public VisualizationGrid xyGrid;

        public MouseOrientator orientator;
        public MouseOrientator rollOrientator;
        public VisualizationSphere visSphere;

        internal GameObject physicalHand = null;
        internal SphereCollider sphereCollider = null;
        internal float defaultSphereColliderRadius = 0.0f;
        internal Coroutine grabRoutine = null;

        public Quaternion Rotation
        {
            get
            {
                return latch.GlobalRotation;
            }
            set
            {
                
                latch.GlobalRotation = value;

            }
        }

        public Vector3 Position
        {
            get
            {
                return latch.GlobalPosition;

            }
            set
            {
                latch.GlobalPosition = value;
            }
        }
    }
    #region Quick Grab
    internal void QuickGrab(Hand hand)
    {
        if (hand.grabRoutine != null)
            StopCoroutine(hand.grabRoutine);
        hand.sphereCollider.radius = hand.defaultSphereColliderRadius;

        RaycastHit hitInfo;
        if (Physics.Raycast(camera2dof.transform.position, camera2dof.transform.forward, out hitInfo, 10.0f))
        {
            var interactable = hitInfo.transform.gameObject.GetComponentInParent<OvidVRInteractableItem>();
            if (true)
            {
                var toLocal = transform.worldToLocalMatrix;
                var newOffset = toLocal.MultiplyPoint(hitInfo.point - camera2dof.transform.forward * 0.18f);
                hand.latch.Offset = newOffset;

                hand.grabRoutine = StartCoroutine(WaitAndGrab(hand));
            }
        }
    }

    IEnumerator WaitAndGrab(Hand h)
    {
        h.Trigger = false;
        h.Grip = false;
        yield return new WaitForSeconds(0.1f); // wait for the item to drop away from us so we don't grab it again

        var requiredPosition = transform.localToWorldMatrix.MultiplyPoint(h.latch.Offset);
        h.Position = requiredPosition;
        h.sphereCollider.radius = grabRadius;
        // wait until physical hand is close enough to the hit position
        // exit if it takes too long.
        float timer = 0.0f;
        yield return new WaitWhile(() =>
        {
            timer += Time.deltaTime;
            var distance = Vector3.Distance(h.physicalHand.transform.position, requiredPosition);
            return (distance > 0.1f) && (timer < 5.0f);
        });

        h.Grip = true;
        yield return new WaitForSeconds(0.1f);

        h.latch.Offset = h.latch.DefaultOffset;
        h.grabRoutine = null;
        h.sphereCollider.radius = h.defaultSphereColliderRadius;
    }
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        Instance = this;
        XRSettings.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        camera2dof = GetComponent<Camera>();
        mouseLookOrientator = new MouseOrientator();
        mouseLookOrientator.SetClamp(null, new Vector2(-89, 89));
        mouseLookOrientator.ExtractOrientation(transform.rotation);

        var rb = GetComponent<Rigidbody>();
        if (rb) rb.freezeRotation = true;

        controller = GetComponent<CharacterController>();
        Assert.IsNotNull(controller);

        // Hands
        leftHand.latch = HandLatch.Install(OvidVRControllerClass.Get.leftController,   OvidVRControllerClass.OvidVRHand.left,  VisGrid, VisSphere);
        rightHand.latch = HandLatch.Install(OvidVRControllerClass.Get.rightController, OvidVRControllerClass.OvidVRHand.right, VisGrid, VisSphere);
        SetupHand(OvidVRControllerClass.OvidVRHand.left);
        SetupHand(OvidVRControllerClass.OvidVRHand.right);
    }

    void SetupHand(OvidVRControllerClass.OvidVRHand handType)
    {
        var localToWorld = transform.localToWorldMatrix;

        switch (handType)
        {
            case OvidVRControllerClass.OvidVRHand.left:
                leftHand.latch.DefaultOffset = Vector3.Scale(defaultHandOffset, new Vector3(-1, 1, 1));
                leftHand.latch.Offset = leftHand.latch.DefaultOffset;

                leftHand.Position = localToWorld.MultiplyPoint(leftHand.latch.Offset);

                // XZ
                leftHand.xzGrid = new VisualizationGrid(leftHand.latch.visGrids);
                leftHand.xzGrid.InternalOrientation = Quaternion.Euler(-90, 90, 0);
                leftHand.xzTranslator = new MouseTranslator();
                leftHand.xzTranslator.ExtractTranslation(leftHand.latch.Offset);
                grids.Add(leftHand.xzGrid);
                // XY
                leftHand.xyGrid = new VisualizationGrid(leftHand.latch.visGridXY);
                leftHand.xyTranslator = new MouseTranslator(Vector3.right, Vector3.up);
                leftHand.xyTranslator.ExtractTranslation(leftHand.latch.Offset);
                grids.Add(leftHand.xyGrid);

                leftHand.orientator = new MouseOrientator(Vector3.up, Vector3.forward);
                leftHand.orientator.ExtractOrientation(defaultLeftHandOrientation);

                leftHand.rollOrientator = new MouseOrientator(Vector3.right, Vector3.zero);
                leftHand.rollOrientator.ExtractOrientation(defaultLeftHandRollOrientation);

                leftHand.visSphere = new VisualizationSphere(leftHand.latch.visSphere);
                allRings.Add(leftHand.visSphere.yawRing);
                allRings.Add(leftHand.visSphere.pitchRing);
                allRings.Add(leftHand.visSphere.rollRing);

                leftHand.physicalHand = OvidVRControllerClass.Get.leftHand;
                leftHand.sphereCollider = leftHand.physicalHand.GetComponentInChildren<SphereCollider>();
                leftHand.defaultSphereColliderRadius = leftHand.sphereCollider.radius;
                break;
            case OvidVRControllerClass.OvidVRHand.right:
                rightHand.latch.DefaultOffset = defaultHandOffset;
                rightHand.latch.Offset = defaultHandOffset;
                rightHand.Position = localToWorld.MultiplyPoint(rightHand.latch.Offset);

                // XZ
                rightHand.xzGrid = new VisualizationGrid(rightHand.latch.visGrids);
                rightHand.xzGrid.InternalOrientation = Quaternion.Euler(-90, 90, 0);
                rightHand.xzTranslator = new MouseTranslator();
                rightHand.xzTranslator.ExtractTranslation(rightHand.latch.Offset);
                grids.Add(rightHand.xzGrid);

                // XY
                rightHand.xyGrid = new VisualizationGrid(rightHand.latch.visGridXY);
                rightHand.xyTranslator = new MouseTranslator(Vector3.right, Vector3.up);
                rightHand.xyTranslator.ExtractTranslation(rightHand.latch.Offset);
                grids.Add(rightHand.xyGrid);

                rightHand.orientator = new MouseOrientator(Vector3.up, -Vector3.forward);
                rightHand.orientator.ExtractOrientation(defaultRightHandOrientation);

                rightHand.rollOrientator = new MouseOrientator(Vector3.right, Vector3.zero);
                rightHand.rollOrientator.ExtractOrientation(defaultRightHandRollOrientation);

                rightHand.visSphere = new VisualizationSphere(rightHand.latch.visSphere);
                allRings.Add(rightHand.visSphere.yawRing);
                allRings.Add(rightHand.visSphere.pitchRing);
                allRings.Add(rightHand.visSphere.rollRing);

                rightHand.physicalHand = OvidVRControllerClass.Get.rightHand;
                rightHand.sphereCollider = rightHand.physicalHand.GetComponentInChildren<SphereCollider>();
                rightHand.defaultSphereColliderRadius = rightHand.sphereCollider.radius;
                break;
        }
    }

    internal void SwitchControlMode(ControlMode newMode)
    {
        if (newMode == currentControlMode) return;

        if (newMode == ControlMode.Body)
        {
            Hand h = GetControlledHand();
            h.Grip = h.Trigger || h.Grip;
            h.Trigger = false;

        }

        currentControlMode = newMode;
    }
    internal void ToggleHandControlMode()
    {
        if (currentControlMode == ControlMode.Body) return;
        
        HandControlMode newMode = currentHandControlMode == HandControlMode.Translation ? HandControlMode.Rotation : HandControlMode.Translation;
        currentHandControlMode = newMode;
    }

    // Update is called once per frame
    void Update()
    {

        // Visualization Grid offsets
        leftHand.xzGrid.offset = Vector3.Scale(visGridOffset, new Vector3(1, 1, -1));
        rightHand.xzGrid.offset = Vector3.Scale(visGridOffset, new Vector3(1, 1, +1));

        if (Input.GetKeyUp(KeyCode.Space))
        {
            leftHand.latch.DefaultOffset = Vector3.Scale(defaultHandOffset, new Vector3(-1, 1, 1));
            rightHand.latch.DefaultOffset = Vector3.Scale(defaultHandOffset, new Vector3(1, 1, 1));

            leftHand.orientator.ExtractOrientation(defaultLeftHandOrientation);
            leftHand.rollOrientator.ExtractOrientation(defaultLeftHandRollOrientation);

            rightHand.orientator.ExtractOrientation(defaultRightHandOrientation);
            rightHand.rollOrientator.ExtractOrientation(defaultRightHandRollOrientation);

            leftHand.latch.Offset = leftHand.latch.DefaultOffset;
            rightHand.latch.Offset = rightHand.latch.DefaultOffset;

        }

        // Mouse Lock
        if (Input.GetKeyUp(KeyCode.G)) ToggleMouseLock();

        // Control Mode
        if (Input.GetKeyUp(KeyCode.F1)) SwitchControlMode(ControlMode.Body);
        if (Input.GetKeyUp(KeyCode.F2)) SwitchControlMode(ControlMode.RightHand);
        if (Input.GetKeyUp(KeyCode.F3)) SwitchControlMode(ControlMode.LeftHand);

        if (Input.GetKeyUp(KeyCode.Tab)) ToggleHandControlMode();

        if (currentControlMode == ControlMode.Body || (currentHandControlMode == HandControlMode.Rotation)) activeGrid = null;
        if (currentControlMode == ControlMode.Body || (currentHandControlMode == HandControlMode.Translation)) activeRings.Clear();
        
        if (currentControlMode != ControlMode.Body)
        {
            if (currentHandControlMode == HandControlMode.Translation)
            {
                if (Input.GetKey(KeyCode.LeftControl)) currentHandTranslationMode = HandTranslationMode.XYPlane;
                else currentHandTranslationMode = HandTranslationMode.XZPlane;

                if (currentControlMode == ControlMode.LeftHand)
                {
                    activeGrid = currentHandTranslationMode == HandTranslationMode.XZPlane
                        ? leftHand.xzGrid
                        : leftHand.xyGrid;
                }
                else
                {
                    activeGrid = currentHandTranslationMode == HandTranslationMode.XZPlane
                        ? rightHand.xzGrid
                        : rightHand.xyGrid;
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftControl)) currentHandRotationMode = HandRotationMode.Roll;
                else currentHandRotationMode = HandRotationMode.YawPitch;

                activeRings.Clear();

                Hand h = GetControlledHand();

                if (currentHandRotationMode == HandRotationMode.YawPitch)
                {
                    activeRings.Add(h.visSphere.yawRing);
                    activeRings.Add(h.visSphere.pitchRing);
                }
                else
                {
                    activeRings.Add(h.visSphere.rollRing);
                }
            }
        }


        if (mouseIsLocked)
        {
            switch (currentControlMode)
            {
                case ControlMode.Body:
                    transform.rotation = mouseLookOrientator.Update();
                    activeGrid = null;

                    Hand hand = null;
                    if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
                        hand = rightHand;
                    else if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftControl))
                        hand = leftHand;

                    if (hand != null)
                    {
                        QuickGrab(hand);

                    }

                    break;
                case ControlMode.LeftHand:
                case ControlMode.RightHand:
                    ControlHand(currentControlMode);
                    break;
            }

        }
        else
        {
            activeGrid = null;
        }

        // Body Movement
        Vector3 moveDirection;
        Vector3 front = transform.rotation * Vector3.forward;
        Vector3 right = Vector3.Cross(front, Vector3.up);

        float inFront = Input.GetAxis("Vertical");
        float inRight = -Input.GetAxis("Horizontal");

        //Up-Down
        Vector3 upvector = new Vector3(); ;
        if (Input.GetKey(KeyCode.Q))
            upvector.y -= 1f * bodySpeed / 3 * Time.deltaTime;
        if (Input.GetKey(KeyCode.E))
            upvector.y += 1f * bodySpeed / 3 * Time.deltaTime;

        moveDirection = front * (bodySpeed * inFront * Time.deltaTime) + right * (bodySpeed * inRight * Time.deltaTime) + upvector;



        controller.Move(moveDirection);
        // Orient hands around and in front of the view frustum
        var localToWorld = transform.localToWorldMatrix;
        if (leftHand.grabRoutine == null)  leftHand.Position    = localToWorld.MultiplyPoint(leftHand.latch.Offset);
        if (rightHand.grabRoutine == null) rightHand.Position   = localToWorld.MultiplyPoint(rightHand.latch.Offset);

        leftHand.Rotation    = transform.rotation * leftHand.orientator.GetCurrentOrientation() * leftHand.rollOrientator.GetCurrentOrientation();
        rightHand.Rotation = transform.rotation * rightHand.orientator.GetCurrentOrientation() * rightHand.rollOrientator.GetCurrentOrientation();
        
        // Orient visualization grids
        {
            leftHand.xzGrid.Update(leftHand.Position, transform.rotation, true);
            leftHand.xyGrid.Update(Vector3.zero, transform.rotation, false);
            rightHand.xzGrid.Update(rightHand.Position, transform.rotation, true);
            rightHand.xyGrid.Update(Vector3.zero, transform.rotation, false);

            leftHand.xzGrid.SetOffset(new Vector2(-leftHand.latch.Offset.z,   -leftHand.latch.Offset.x ) * 5.25f);
            leftHand.xyGrid.SetOffset(new Vector2(-leftHand.latch.Offset.x,   +leftHand.latch.Offset.y ) * 5.25f);
            rightHand.xzGrid.SetOffset(new Vector2(-rightHand.latch.Offset.z,  -rightHand.latch.Offset.x) * 5.25f);
            rightHand.xyGrid.SetOffset(new Vector2(-rightHand.latch.Offset.x,  +rightHand.latch.Offset.y) * 5.25f);
        }

        // Fade grids
        foreach (var grid in grids)
        {
            if (grid == activeGrid)
                grid.SetFade(1.0f);
            else
                grid.SetFade(0);
        }

        foreach (var ring in allRings)
        {
            if (activeRings.Contains(ring))
                ring.Enabled = true;
            else
                ring.Enabled = false;
        }

        // Orientation Sphere
        {
            Hand h = GetControlledHand();
            if (h != null && currentHandControlMode == HandControlMode.Rotation)
            {
                if (currentHandRotationMode == HandRotationMode.YawPitch)
                {
                    var v = new Vector2(h.orientator.mx, h.orientator.my);
                    h.visSphere.SetYawPitch(v.x, v.y, 0.10f);
                }
                else
                    h.visSphere.SetRoll(h.rollOrientator.mx, 0.10f);
            }

            leftHand.visSphere.SetColor(ringPitchColor, ringYawColor, ringRollColor);
            rightHand.visSphere.SetColor(ringPitchColor, ringYawColor, ringRollColor);
        }
        {
            leftHand.visSphere.Update(leftHand.Position, transform.rotation, Vector3.Scale(visSphereOffset, new Vector3(-1, 1, 1)));
            rightHand.visSphere.Update(rightHand.Position, transform.rotation, visSphereOffset);
        }

        // Hand trigger and grip animation
        HandButtonAnimation(leftHand);
        HandButtonAnimation(rightHand);
    }
    
    #region Hands
    internal void HandButtonAnimation(Hand h)
    {
        float targetTrigger = h.Trigger ? 1.0f : 0.0f;
        float targetGrip = h.Grip ? 1.0f : 0.0f;

        Func<float, float, float> f = (target, current) =>
        {
            float result = current;
            if (current != target)
                result += (target - current) * Time.deltaTime * buttonAnimationSpeed;
            return Mathf.Clamp01(result);
        };

        h.TriggerStrength = f(targetTrigger, h.TriggerStrength);
        h.GripStrength = f(targetGrip, h.GripStrength);

    }

    internal void ControlHand(ControlMode which)
    {

        MouseTranslator translator = GetCurrentTranslator(which);
        Hand hand = which == ControlMode.LeftHand ? leftHand : rightHand;

        hand.Trigger = Input.GetMouseButton(0);
        hand.Grip = Input.GetMouseButton(1);

        if (translator != null)
        {
            var localToWorld = transform.localToWorldMatrix;
            translator.ExtractTranslation(hand.latch.Offset);

            // Project new controller position to viewport coordinates ([0,1],[0,1], [0,1])
            // if the transformed position is negative, or greater than 1 (component-wise compare)
            // then that point is outside of the view frustum.
            Vector3 newOffset = translator.Update();
            var viewport = camera2dof.WorldToViewportPoint(localToWorld.MultiplyPoint(newOffset));

            if ((viewport.x < 0 || viewport.y < 0 || viewport.z < 0) ||
                ((viewport.x > 1 || viewport.y > 1)))
            {
                translator.ExtractTranslation(hand.latch.Offset);
            }
            else
            {
                hand.latch.Offset = newOffset;
            }
            return;
        }

        MouseOrientator orientator = GetCurrentOrientator(which);
        orientator.Update();

    }

    internal Hand GetControlledHand()
    {
        if (currentControlMode == ControlMode.Body) return null;

        return currentControlMode == ControlMode.LeftHand
            ? leftHand
            : rightHand;
    }

    internal MouseOrientator GetCurrentOrientator(ControlMode which)
    {
        if (currentHandControlMode != HandControlMode.Rotation) return null;

        Hand hand = GetControlledHand();
        if (hand == null) return null;

        return currentHandRotationMode == HandRotationMode.YawPitch
            ? hand.orientator
            : hand.rollOrientator;
    }

    internal MouseTranslator GetCurrentTranslator(ControlMode which)
    {
        if (currentHandControlMode != HandControlMode.Translation) return null;

        Hand hand = GetControlledHand();
        if (hand == null) return null;

        return currentHandTranslationMode == HandTranslationMode.XZPlane
            ? hand.xzTranslator
            : hand.xyTranslator;
    }
    #endregion Hands
    #region Mouse Lock
    /// <summary>
    /// Unity has some weird behavior with Cursor.visible. Whenever it is set or unset, the cursor will not remain
    /// while in the play mode window.
    /// </summary>
    internal void ToggleMouseLock()
    {
        mouseIsLocked = !mouseIsLocked;
        Cursor.lockState = mouseIsLocked
            ? CursorLockMode.Locked
            : CursorLockMode.None;
        Cursor.visible = mouseIsLocked;
    }
    #endregion
    #region DebugVis
    internal static Texture2D lineTex = null;

    public Vector2 cursorSize = new Vector2(4, 4);
    public float cursorThickness = 1.0f;
    public Color cursorColor = Color.green;
    internal Rect labelRect = new Rect(0, 0, 256.0f, 100.0f);

    internal void OnGUI()
    {
        DrawLine(new Vector2(Screen.width / 2.0f - cursorSize.x - 1.0f, Screen.height / 2.0f), new Vector2(Screen.width / 2.0f + cursorSize.x, Screen.height / 2.0f), cursorColor, cursorThickness);
        DrawLine(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f - cursorSize.y), new Vector2(Screen.width / 2.0f, Screen.height / 2.0f + cursorSize.y + 1.0f), cursorColor, cursorThickness);



        if (currentControlMode != ControlMode.Body)
        {
            if (currentHandControlMode == HandControlMode.Translation)
                GUI.Label(labelRect, "Translation - " + currentControlMode.ToString() + " - Mode: " + currentHandTranslationMode.ToString());
            else
                GUI.Label(labelRect, "Rotation - " + currentControlMode.ToString() + " - Mode: " + currentHandRotationMode.ToString());
        }
        else
            GUI.Label(labelRect, "Body");

    }

    static internal void DrawLine(Vector2 A, Vector2 B, Color? color = null, float width = 1.0f)
    {
        if (color == null) color = Color.white;

        var matrix = GUI.matrix;

        if (!lineTex)
        {
            lineTex = new Texture2D(1, 1);
            lineTex.SetPixel(1, 1, Color.white);
            lineTex.Apply();
        }

        var savedColor = GUI.color;
        GUI.color = color.Value;

        var angle = Vector2.Angle(B - A, Vector2.right);

        if (A.y > B.y) angle = -angle;

        // adjust for the size of the line
        GUIUtility.ScaleAroundPivot(new Vector2((B - A).magnitude, width), new Vector2(A.x, A.y + 0.5f));
        // adjust for the rotation of the line
        GUIUtility.RotateAroundPivot(angle, A);

        GUI.DrawTexture(new Rect(A.x, A.y, 1, 1), lineTex);

        GUI.matrix = matrix;
        GUI.color = savedColor;
    }
    #endregion

}
