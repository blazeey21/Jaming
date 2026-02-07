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

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask = -1;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private Transform groundCheckPoint;

    [Header("Rotation")]
    [SerializeField] private float rotationSmoothTime = 0.1f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;
    private float verticalVelocity;
    private float rotationVelocity;
    private bool isSprinting;
    private bool isGrounded;
    private bool isJumping;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Si no se asigna groundCheckPoint, usar el transform del personaje
        if (groundCheckPoint == null)
            groundCheckPoint = transform;
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
    public CursorDragDrop cursorDragDrop;
    public void Grab(InputAction.CallbackContext context)
    {
        cursorDragDrop.OnGrab(context);
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
        // Usar esfera para mejor detección de suelo
        isGrounded = Physics.CheckSphere(
            groundCheckPoint.position,
            groundCheckDistance,
            groundMask
        );

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
            isJumping = false;
        }
    }

    void HandleMovement()
    {
        float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Normalizar input diagonal para mantener velocidad constante
        Vector2 normalizedInput = moveInput.normalized;
        Vector3 moveDirection = new Vector3(normalizedInput.x, 0f, normalizedInput.y);

        if (moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            Vector3 worldDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            targetVelocity = worldDirection * targetSpeed;
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

        // Combinar movimientos
        Vector3 finalMovement = currentVelocity + Vector3.up * verticalVelocity;

        // Aplicar movimiento
        controller.Move(finalMovement * Time.deltaTime);
    }

    void HandleRotation()
    {
        if (currentVelocity.magnitude > 0.1f)
        {
            float targetRotation = Mathf.Atan2(currentVelocity.x, currentVelocity.z) * Mathf.Rad2Deg;
            float smoothedRotation = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetRotation,
                ref rotationVelocity,
                rotationSmoothTime
            );
            transform.rotation = Quaternion.Euler(0f, smoothedRotation, 0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckDistance);
        }
    }
}