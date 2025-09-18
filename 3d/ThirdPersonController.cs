using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class ThirdPersonControllerRB : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundMask;

    [Header("Jump")]
    public float jumpForce = 5f;
    public Transform groundCheck;
    public float gravity = -20f;

    [Header("Camera")]
    public Transform cameraTransform;
    public Vector3 cameraOffset = new Vector3(0, 3, -6);
    public float mouseSensitivity = 3f;
    public float minPitch = -35f;
    public float maxPitch = 60f;
    public float cameraSmoothTime = 0.05f;
    public LayerMask cameraCollisionMask;
    public float cameraCollisionRadius = 0.3f;

    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded;
    private float yaw;
    private float pitch;
    private float turnSmoothVelocity;
    private float cameraDistance;
    private Vector3 desiredMove;
    private Vector3 camVel = Vector3.zero;
    private Vector3 velocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
        cameraDistance = cameraOffset.magnitude;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ReadInput();
        HandleCameraInput();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        CheckGround();
        ApplyGravity();
        MovePlayer();
        HandleJump();
        rb.linearVelocity = velocity;
    }

    void LateUpdate()
    {
        UpdateCameraPosition();
    }

    void ReadInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        if (inputDir.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + yaw;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.1f);
            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
            desiredMove = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                desiredMove *= sprintSpeed;
            else
                desiredMove *= walkSpeed;
        }
        else
        {
            desiredMove = Vector3.zero;
        }
    }

    void MovePlayer()
    {
        velocity.x = desiredMove.x;
        velocity.z = desiredMove.z;
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = jumpForce;
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
        else
            velocity.y += gravity * Time.fixedDeltaTime;
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
    }

    void HandleCameraInput()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void UpdateCameraPosition()
    {
        Vector3 targetLookPoint = transform.position + Vector3.up * 1.5f;
        Quaternion cameraRot = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredCameraPos = targetLookPoint + cameraRot * cameraOffset;

        Vector3 direction = (desiredCameraPos - targetLookPoint).normalized;
        float distance = cameraOffset.magnitude;

        if (Physics.SphereCast(targetLookPoint, cameraCollisionRadius, direction, out RaycastHit hit, distance, cameraCollisionMask, QueryTriggerInteraction.Ignore))
        {
            desiredCameraPos = targetLookPoint + direction * (hit.distance - 0.1f);
        }

        cameraTransform.position = desiredCameraPos;
        cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, cameraRot, Time.deltaTime * 12f);
    }

    void UpdateAnimator()
    {
        float moveSpeed = desiredMove.magnitude;
        animator.SetFloat("Walk", moveSpeed > 0.01f ? 1f : 0f);
    }

    //  Called by DollController
    public bool IsMoving()
    {
        return desiredMove.magnitude > 0.1f;
    }

    public void Die()
    {
        Debug.Log("Player died.");
        enabled = false;
        animator.SetFloat("Walk", 0f);
        // Add ragdoll, death anim, or restart here if you want
    }
}
