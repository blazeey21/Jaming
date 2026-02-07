using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerLogic : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask = -1;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private Transform groundCheckPoint;

    [Header("Rotation")]
    [SerializeField] private float rotationSmoothTime = 0.1f;
    [SerializeField] private bool rotateToMoveDirection = true;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;
    private float verticalVelocity;
    private float rotationVelocity;
    private bool isSprinting;
    private bool isGrounded;
    private bool isJumping;
    public bool CanMove;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (groundCheckPoint == null)
            groundCheckPoint = transform;

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main?.transform;
        }
        CanMove = true;
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleRotation();
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            isJumping = true;
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(
            groundCheckPoint.position,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
            isJumping = false;
        }
    }

    void HandleMovement()
    {
        if (CanMove == true)
        {
            float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;

            Vector2 normalizedInput = moveInput.normalized;

            if (moveInput.magnitude >= 0.1f)
            {
                Vector3 moveDirection = GetCameraRelativeMovement(normalizedInput);

                if (moveDirection.magnitude >= 0.1f)
                {
                    targetVelocity = moveDirection * targetSpeed;
                }
            }
            else
            {
                targetVelocity = Vector3.zero;
            }

            float currentAcceleration = targetVelocity.magnitude > 0.01f ? acceleration : deceleration;
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                targetVelocity,
                currentAcceleration * Time.deltaTime
            );

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 finalMovement = currentVelocity + Vector3.up * verticalVelocity;

            controller.Move(finalMovement * Time.deltaTime);
        }
    }

    Vector3 GetCameraRelativeMovement(Vector2 input)
    {
        if (cameraTransform == null)
        {
            return new Vector3(input.x, 0f, input.y);
        }

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 relativeForward = cameraForward * input.y;
        Vector3 relativeRight = cameraRight * input.x;

        return (relativeForward + relativeRight).normalized;
    }

    void HandleRotation()
    {
        if (currentVelocity.magnitude <= 0.1f || !rotateToMoveDirection)
            return;

        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        if (horizontalVelocity.magnitude > 0.1f)
        {
            float targetRotation = Mathf.Atan2(horizontalVelocity.x, horizontalVelocity.z) * Mathf.Rad2Deg;

            float smoothedRotation = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetRotation,
                ref rotationVelocity,
                rotationSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, smoothedRotation, 0f);
        }
    }

    public void SetCameraReference(Transform newCameraTransform)
    {
        cameraTransform = newCameraTransform;
    }

    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsJumping()
    {
        return isJumping;
    }

    public float GetVerticalVelocity()
    {
        return verticalVelocity;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Application.isPlaying ? (isGrounded ? Color.green : Color.red) : Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckDistance);
        }
    }
}