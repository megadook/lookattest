using UnityEngine;
using System.Collections;

/// <summary>
/// camera controls for a third person character
/// the camera rig should follow the hierarchy of:
///     --base object
///             --pivot point
///                 --effects object (screen shake etc)
///                     --camera
/// </summary>
public class ThirdPersonCam : MonoBehaviour 
{
    // is the player using a gamepad?
    public bool usingGamepad;

    // invert y input
    public bool invertPitch = false;

    [Header("Speeds")]
    // positional speed
    public float moveSpeed = 1f;   
    // rotational speed
    public float turnSpeed = 1.5f;
    // speed of the yaw centering from movement
    public float yawSnapSpeed = 10f;

    [Space(5)]
    // mouseturn input smoothing
    public float turnSmoothing = 0.1f;

    // local offsets
    [Header("Local Offsets")]
    public float localOffsetX;
    public float localOffsetY;
    public float localZoom;

    // pivot offsets
    [Header("Pivot Offsets")]
    public float pivotOffsetX;
    public float pivotOffsetY;
    public float pivotOffsetZ;

    // min and mix pitch rotation
    public float pitchMax = 75f;    
    public float pitchMin = 45f;

    // min and max zoom (must be inverted, local z is negative)
    public float maxZoom = 4f;
    public float minZoom = 0.5f;
    
    // should the camera snap back to place 
    public bool pitchAutoReturn = false;
    //public bool yawAutoReturn = false;

    // pivot rotation data
    private float pitchAngle = 0;
    private float yawTargetAngle = 180;
    private float yawPlayerInfluence = 0;

    [SerializeField] // follow target
    private Transform target;

    // where should the camera update?
    public enum UpdateType { Update, FixedUpdate, LateUpdate, ManualUpdate }
    public UpdateType updateType;

    // the transform of the camera
    private Transform cam;
    // the point at which the camera pivots around
    private Transform pivotTrans;
    // the transform used for various effects
    private Transform effectTrans;

    private Vector3 lastTargetPosition;

    #region player input 
    // player/camera movement
    float movementInputX = 0;
    float movementInputY = 0;
    float cameraInputX = 0;
    float cameraInputY = 0;

    // camera zoom
    float cameraZoomInput = 0;

    // button for camera re-centering
    bool cameraSnapInput = false;

    // is there currently input read for player/camera movement
    bool movementInput
    {
        get
        {
            if (movementInputX == 0 && movementInputY == 0)
            {
                return false;
            }

            return true;
        }
    }
    bool cameraInput
    {
        get
        {
            if (cameraInputX == 0 && cameraInputY == 0)
            {
                return false;
            }

            return true;
        }
    }
    #endregion


    void Awake()
    {
        // grab rig components
        cam = GetComponentInChildren<Camera>().transform;
        pivotTrans = cam.parent.parent;
        effectTrans = cam.parent;

        // hide/lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        // check for target
        if (target == null)
        {
            Debug.Log("no target has been assigned to " + this);
            SetTarget(transform);
        }
    }

    void Update()
    {
        // input
        ReadPlayerInput();

        if (updateType == UpdateType.Update)
        {
            UpdateRotation();
            FollowTarget();
            UpdateLocalOffsets();
        }

        // TODO cursor locking/visability should NOT be controlled in update
        ReadCursorInput();
    }

    /// <summary>
    /// TEMP reads to change the cursor visability/locking
    /// </summary>
    void ReadCursorInput()
    {
        if (Cursor.lockState == CursorLockMode.None && Input.GetMouseButtonUp(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// fills input data
    /// TODO read zoom and snapping input
    /// </summary>
    void ReadPlayerInput()
    {
        // movement
        movementInputX = Input.GetAxis("Horizontal");
        movementInputY = Input.GetAxis("Vertical");

        // camera
        if (usingGamepad)
        {
            cameraInputX = Input.GetAxis("RightStickX");
            cameraInputY = Input.GetAxis("RightStickY");
        }
        else
        {
            cameraInputX = Input.GetAxis("Mouse X");
            cameraInputY = -Input.GetAxis("Mouse Y");
        }

        // invert pitch
        if (!invertPitch)
        {
            cameraInputY = -cameraInputY;
        }
    }

    /// <summary>
    /// position tracking and movement
    /// </summary>
    void FollowTarget()
    {
        if (target == null) { return; }

        // grab target position and alter with offset
        Vector3 targetPosition = target.position;

        // move towards by moveSpeed
        targetPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        transform.position = targetPosition;
    }

    /// <summary>
    /// updates the rotation of the camera based on current input values
    /// </summary>
    void UpdateRotation()
    {
        #region calculate yaw rotation
        // player is moving
        if (movementInput)
        {
            // move towards the back of the target
            if (!cameraInput && usingGamepad)
            {
                yawTargetAngle += movementInputX * turnSpeed;
            }

            // player influence
            yawPlayerInfluence += cameraInputX * turnSpeed;
        }
        // not moving
        else
        {
            yawPlayerInfluence += cameraInputX * turnSpeed;
        }
        #endregion

        #region calculate pitch rotation
        if (pitchAutoReturn && cameraInputY == 0)
        {
            // move to min/max tilt
            pitchAngle = cameraInputY > 0 ? Mathf.Lerp(0, -pitchMin, cameraInputY) : Mathf.Lerp(0, pitchMax, -cameraInputY);
        }
        else
        {
            // on platforms with a mouse, we adjust the current angle based on Y mouse input and turn speed
            pitchAngle -= cameraInputY * turnSpeed;
            // and make sure the new value is within the tilt range
            pitchAngle = Mathf.Clamp(pitchAngle, -pitchMin, pitchMax);
        }
        #endregion
        
        // target rotation from pitch and yaw
        Quaternion pivotTargetRot = Quaternion.Euler(pitchAngle, yawTargetAngle + yawPlayerInfluence, 0f);

        // apply rotation
        if (turnSmoothing > 0)
        {
            pivotTrans.localRotation = Quaternion.Slerp(pivotTrans.localRotation, pivotTargetRot, turnSmoothing * Time.deltaTime);
        }
        else
        {
            pivotTrans.localRotation = pivotTargetRot;
        }
    }

    /// <summary>
    /// sets the local offsets of the camera to the current internal values
    /// </summary>
    void UpdateLocalOffsets()
    {
        Vector3 targetPivotOffset = new Vector3(pivotOffsetX, pivotOffsetY, pivotOffsetZ);
        pivotTrans.localPosition = targetPivotOffset;

        Vector3 targetLocalOffset;

        targetLocalOffset.x = localOffsetX;
        targetLocalOffset.y = localOffsetY;
        targetLocalOffset.z = localZoom;

        cam.transform.localPosition = targetLocalOffset;
    }

    void FixedUpdate()
    {
        if (updateType == UpdateType.FixedUpdate)
        {
            UpdateRotation();
            FollowTarget();
            UpdateLocalOffsets();
        }
    }

    void LateUpdate()
    {
        if (updateType == UpdateType.LateUpdate)
        {
            UpdateRotation();
            FollowTarget();
            UpdateLocalOffsets();
        }
    }

    /// <summary>
    /// updates the rotation and camera follow
    /// </summary>
    public void ManualUpdate()
    {
        UpdateRotation();
        FollowTarget();
        UpdateLocalOffsets();
    }

    /// <summary>
    /// sets a new Transform target to follow
    /// </summary>
    public void SetTarget(Transform newTransform)
    {
        target = newTransform;

        // tell the target that this will be its camera
        target.SendMessage("SetCamera", cam, SendMessageOptions.DontRequireReceiver);
    }
}
