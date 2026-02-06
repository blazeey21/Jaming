using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerLogic : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -25f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public float maxLookUp = 80f;

    [Header("References")]
    public Transform cameraPivot;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;

    private float xRotation;
    private float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    // ===== INPUT =====
    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    // ===== MOVEMENT =====
    void HandleMovement()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 horizontalMove = transform.right * moveInput.x * moveSpeed;
        Vector3 verticalMove = Vector3.up * verticalVelocity;

        controller.Move((horizontalMove + verticalMove) * Time.deltaTime);
    }

    // ===== LOOK =====
    void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * 0.01f;
        float mouseY = lookInput.y * mouseSensitivity * 0.01f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookUp, maxLookUp);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}
