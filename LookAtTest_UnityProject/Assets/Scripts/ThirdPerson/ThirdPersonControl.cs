using UnityEngine;
using System.Collections;

public class ThirdPersonControl : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody rb;
    public Animator anim;

    [Space(5)]
    public Camera playerCam;

    [Header("Control Parameters")]
    public bool active = true;

    [Space(5)]
    public float movingTurnSpeed = 360;
    public float stationaryTurnSpeed = 180;

    [Space(5)]
    public float maxMoveSpeed = 1f;
    public float moveAcceleration = 0.5f;
    private float targetMoveSpeed;
    private float currentMoveSpeed = 0f;

    public float animSpeed = 1f;

    [Space(5)]
    public LayerMask groundLayerMask;
    public float groundCheckDistance = 0.1f;
    public float gravityMultiplier = 2f;

    private bool grounded = false;
    private Vector3 groundNormal;


    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            Debug.LogWarning("No Rigidbody found on ThirdPerson character, cannot continue.");
            this.enabled = false;
            return;
        }

        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        if (anim != null)
        {
            anim.applyRootMotion = false;
        }
    }

    void FixedUpdate()
    {
        if (!active) { return; }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // calculate move direction to pass to character
        // either camera space or world space
        Vector3 moveDir;
        if (playerCam != null)
        {
            Vector3 camForward = Vector3.Scale(playerCam.transform.forward, new Vector3(1, 0, 1)).normalized;
            moveDir = v * camForward + h * playerCam.transform.right;
        }
        else
        {
            moveDir = v * Vector3.forward + h * Vector3.right;
        }

        targetMoveSpeed = moveDir.magnitude * maxMoveSpeed;

        // walk speed multiplier
        if (Input.GetKey(KeyCode.LeftShift)) { targetMoveSpeed *= 0.5f; }

        Move(moveDir);
    }

    void Move(Vector3 moveDir)
    {
        if (moveDir.magnitude > 1f) moveDir.Normalize();
        GroundCheck();

        // find turn and forward values
        moveDir = transform.InverseTransformDirection(moveDir);
        moveDir = Vector3.ProjectOnPlane(moveDir, groundNormal);

        float turnAmount = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
        float forwardAmount = moveDir.z;
        Debug.Log(forwardAmount);

        ApplyRotation(turnAmount, forwardAmount);

        ApplyMovement(forwardAmount, turnAmount);

        UpdateAnimator(forwardAmount, turnAmount);
    }

    void ApplyRotation(float turnAmount, float forwardAmount)
    {
        float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, MapUtility.Map(currentMoveSpeed, 0f, maxMoveSpeed, 0, 1));
        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
    }

    void ApplyMovement(float forwardAmount, float turnAmount)
    {
        currentMoveSpeed = Mathf.Clamp(0f, targetMoveSpeed, currentMoveSpeed += (moveAcceleration * Time.deltaTime));
        transform.position = transform.position + (transform.forward * forwardAmount * currentMoveSpeed * Time.deltaTime);
    }

    void UpdateAnimator(float forwardAmount, float turnAmount)
    {
        anim.SetFloat("Forward", MapUtility.Map(currentMoveSpeed, 0, maxMoveSpeed, 0, 1));
        anim.SetFloat("Turn", MapUtility.Map(turnAmount, -180, 180, -1, 1));
    }

    void GroundCheck()
    {
        RaycastHit hitInfo;

        if (Application.isEditor)
        {
            Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
        }

        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance, groundLayerMask))
        {
            grounded = true;
            groundNormal = hitInfo.normal;
        }
        else
        {
            grounded = false;
            groundNormal = Vector3.up;
        }
    }
}
