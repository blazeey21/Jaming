using UnityEngine;

public class PathControler : MonoBehaviour
{
    [Header("Área de movimiento")]
    [Tooltip("Collider que define la zona donde el objeto puede moverse (debe tener un volumen)")]
    public Collider movementArea;

    [Header("Movimiento")]
    public float moveSpeed = 2f;           
    public float rotateSpeed = 180f;       
    public float arrivalTolerance = 0.5f;  
    public float waitTimeAtDestination = 1f; 

    [Header("Límite de rotación (como muñeca)")]
    [Tooltip("Ángulo máximo de desviación desde la rotación inicial (en grados)")]
    public float maxWristAngle = 30f;

    [Header("Opciones físicas")]
    [Tooltip("Si el objeto tiene Rigidbody, se usará MovePosition/MoveRotation para respetar la física")]
    public bool useRigidbody = true;

    private Rigidbody rb;
    private Vector3 targetPosition;
    private Quaternion initialRotation;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    void Start()
    {
        if (useRigidbody)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                useRigidbody = false;
            }
        }

        if (movementArea == null)
        {
            enabled = false;
            return;
        }

        initialRotation = transform.rotation;

        PickNewDestination();
    }

    void FixedUpdate()
    {
        if (movementArea == null) return;

        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                PickNewDestination();
            }
            else
            {
                RotateTowards(initialRotation);
            }
            return;
        }

        Vector3 direction = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > arrivalTolerance)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);

            if (useRigidbody && rb != null)
            {
                rb.MovePosition(newPosition);
            }
            else
            {
                transform.position = newPosition;
            }

            if (direction != Vector3.zero)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
                Quaternion limitedRotation = LimitWristRotation(desiredRotation);
                RotateTowards(limitedRotation);
            }
        }
        else
        {
            isWaiting = true;
            waitTimer = waitTimeAtDestination;
        }
    }

    private void RotateTowards(Quaternion targetRot)
    {
        if (useRigidbody && rb != null)
        {
            Quaternion newRot = Quaternion.RotateTowards(rb.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRot);
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }

    private Quaternion LimitWristRotation(Quaternion desiredRotation)
    {
        Quaternion delta = desiredRotation * Quaternion.Inverse(initialRotation);
        float angle; Vector3 axis;
        delta.ToAngleAxis(out angle, out axis);

        // Normalizar ángulo
        if (angle > 180f) angle -= 360f;

        angle = Mathf.Clamp(angle, -maxWristAngle, maxWristAngle);
        Quaternion limitedDelta = Quaternion.AngleAxis(angle, axis);
        return limitedDelta * initialRotation;
    }
    private void PickNewDestination()
    {
        if (movementArea == null) return;

        Bounds bounds = movementArea.bounds;
        Vector3 randomPoint = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );


        targetPosition = randomPoint;
    }

    public void ResetMovement()
    {
        isWaiting = false;
        waitTimer = 0f;
        PickNewDestination();
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && movementArea != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPosition, 0.2f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}