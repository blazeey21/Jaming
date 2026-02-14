using UnityEngine;

public class PathControler : MonoBehaviour
{
    [Header("Área de movimiento")]
    public Collider movementArea;

    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float smoothTime = 0.3f;
    public float rotateSpeed = 180f;
    public float arrivalTolerance = 0.5f;
    public float waitTimeAtDestination = 1f;

    [Header("Límite de inclinación (eje X)")]
    [Tooltip("Ángulo máximo de desviación en el eje X respecto a la rotación inicial (en grados)")]
    public float maxPitchDeviation = 30f;

    [Header("Opciones físicas")]
    public bool useRigidbody = true;

    private Rigidbody rb;
    private Vector3 targetPosition;
    private Vector3 smoothVelocity;
    private Quaternion initialRotation;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    // Almacenamos los ángulos Euler iniciales para limitar solo el pitch
    private float initialPitch;
    private float initialYaw;
    private float initialRoll;

    void Start()
    {
        if (useRigidbody)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
                useRigidbody = false;
        }

        if (movementArea == null)
        {
            Debug.LogError("Asigna un Collider como área de movimiento.");
            enabled = false;
            return;
        }

        // Guardamos la rotación inicial completa y sus ángulos Euler
        initialRotation = transform.rotation;
        Vector3 euler = initialRotation.eulerAngles;
        initialPitch = euler.x;
        initialYaw = euler.y;
        initialRoll = euler.z;

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
                // Mientras espera, vuelve suavemente a la rotación inicial (completa)
                RotateTowards(initialRotation);
            }
            return;
        }

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > arrivalTolerance)
        {
            // Movimiento suave hacia el destino
            Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref smoothVelocity, smoothTime, moveSpeed, Time.fixedDeltaTime);

            if (useRigidbody && rb != null)
                rb.MovePosition(newPosition);
            else
                transform.position = newPosition;

            // Calcular dirección del movimiento
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                // Rotación que mira hacia la dirección (solo modifica yaw y pitch)
                Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);

                // Aplicar límite solo al eje X (pitch)
                Quaternion limitedRotation = LimitPitch(desiredRotation);

                // Rotar suavemente hacia la rotación limitada
                RotateTowards(limitedRotation);
            }
        }
        else
        {
            isWaiting = true;
            waitTimer = waitTimeAtDestination;
            smoothVelocity = Vector3.zero;
        }
    }

    private Quaternion LimitPitch(Quaternion desiredRotation)
    {
        // Convertir la rotación deseada a ángulos Euler
        Vector3 desiredEuler = desiredRotation.eulerAngles;

        // Limitar el ángulo X (pitch) para que no se desvíe más de maxPitchDeviation respecto al inicial
        float deltaPitch = Mathf.DeltaAngle(desiredEuler.x, initialPitch);
        float clampedPitch = initialPitch + Mathf.Clamp(deltaPitch, -maxPitchDeviation, maxPitchDeviation);

        // Mantener el yaw (Y) deseado (para mirar hacia el destino) y el roll (Z) inicial (sin cambios)
        // Si quieres que el roll también pueda variar, puedes usar desiredEuler.z o limitarlo aparte.
        float newYaw = desiredEuler.y;      // Libertad total para mirar
        float newRoll = initialRoll;         // Mantenemos el roll original (puedes cambiarlo si quieres)

        // Construir la nueva rotación
        Quaternion limitedRotation = Quaternion.Euler(clampedPitch, newYaw, newRoll);
        return limitedRotation;
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
        smoothVelocity = Vector3.zero;
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