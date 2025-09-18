using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private Vector3 velocity;
    private float rotationX = 0;
    private float rotationY = 0;

    public Transform playerCamera;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate both X (up/down) and Y (left/right)
        rotationX -= mouseY;
        rotationY += mouseX;

        playerCamera.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }

    void HandleMovement()
    {
        float moveZ = Input.GetAxisRaw("Vertical");
        float moveX = Input.GetAxisRaw("Horizontal");

        // Movement relative to camera rotation
        Vector3 move = playerCamera.transform.TransformDirection(new Vector3(moveX, 0, moveZ));
        move.y = 0f; // keep movement flat

        controller.Move(move.normalized * moveSpeed * Time.deltaTime);

        // Jump + gravity
        if (controller.isGrounded)
        {
            velocity.y = -1f;
            if (Input.GetButtonDown("Jump"))
                velocity.y = jumpForce;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
