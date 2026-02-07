using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLogic : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private LayerMask grabbableLayer;
    [SerializeField] private float grabDistance = 5f;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color grabbedColor = Color.green;

    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InputActionProperty grabAction;

    [Header("Cursor Settings")]
    [SerializeField] private bool useMouseCursor = true; // Si es false, usa centro de pantalla
    [SerializeField] private Texture2D grabCursorTexture; // Textura personalizada para el cursor
    [SerializeField] private Texture2D normalCursorTexture; // Textura normal del cursor

    private GameObject currentGrabbable;
    private GameObject grabbedObject;
    private Renderer currentRenderer;
    private Material originalMaterial;
    private Color originalColor;
    private Vector2 lastMousePosition;

    private void Start()
    {
        // Inicializar la acción de agarre si no está asignada
        if (grabAction.action == null)
        {
            Debug.LogWarning("No se ha asignado una acción de agarre. Se usará la tecla E por defecto.");
        }

        // Configurar cursor inicial
        if (useMouseCursor && normalCursorTexture != null)
        {
            Cursor.SetCursor(normalCursorTexture, Vector2.zero, CursorMode.Auto);
        }

        // Suscribir eventos de la acción de entrada
        if (grabAction.action != null)
        {
            grabAction.action.started += OnGrabStarted;
            grabAction.action.canceled += OnGrabCanceled;
        }
    }

    private void OnEnable()
    {
        if (grabAction.action != null)
        {
            grabAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (grabAction.action != null)
        {
            grabAction.action.Disable();
        }

        ResetCurrentGrabbable();
        ReleaseObject();

        // Restaurar cursor normal
        if (useMouseCursor)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    private void Update()
    {
        Ray ray;

        if (useMouseCursor)
        {
            // Raycast desde la posición actual del cursor del ratón
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            // Solo actualizar si el cursor se ha movido (para optimización)
            if (mousePosition == lastMousePosition && currentGrabbable != null && grabbedObject == null)
            {
                return;
            }

            lastMousePosition = mousePosition;
            ray = playerCamera.ScreenPointToRay(mousePosition);
        }
        else
        {
            // Raycast desde el centro de la pantalla (comportamiento original)
            ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, grabDistance, grabbableLayer))
        {
            // Si encontramos un nuevo objeto agarrable
            if (hit.collider.gameObject != currentGrabbable)
            {
                ResetCurrentGrabbable();

                currentGrabbable = hit.collider.gameObject;
                currentRenderer = currentGrabbable.GetComponent<Renderer>();

                if (currentRenderer != null)
                {
                    originalMaterial = currentRenderer.material;
                    originalColor = currentRenderer.material.color;
                    currentRenderer.material.color = hoverColor;

                    // Cambiar cursor si está configurado
                    if (useMouseCursor && grabCursorTexture != null)
                    {
                        Cursor.SetCursor(grabCursorTexture, Vector2.zero, CursorMode.Auto);
                    }
                }
            }
        }
        else
        {
            // Si no estamos apuntando a ningún objeto agarrable
            ResetCurrentGrabbable();

            // Restaurar cursor normal si no hay objeto agarrable
            if (useMouseCursor && normalCursorTexture != null && grabbedObject == null)
            {
                Cursor.SetCursor(normalCursorTexture, Vector2.zero, CursorMode.Auto);
            }
        }

        // Actualizar posición del objeto agarrado
        if (grabbedObject != null)
        {
            UpdateGrabbedObjectPosition();
        }
    }

    private void ResetCurrentGrabbable()
    {
        if (currentGrabbable != null && currentRenderer != null && currentGrabbable != grabbedObject)
        {
            currentRenderer.material.color = originalColor;
        }

        currentGrabbable = null;
        currentRenderer = null;
    }

    private void OnGrabStarted(InputAction.CallbackContext context)
    {
        if (currentGrabbable != null && grabbedObject == null)
        {
            // Agarrar el objeto
            grabbedObject = currentGrabbable;

            // Desactivar física temporalmente
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Cambiar color a verde
            if (currentRenderer != null)
            {
                currentRenderer.material.color = grabbedColor;
            }

            // Cambiar cursor a agarrado si está configurado
            if (useMouseCursor && grabCursorTexture != null)
            {
                Cursor.SetCursor(grabCursorTexture, Vector2.zero, CursorMode.Auto);
            }
        }
        else if (grabbedObject != null)
        {
            // Soltar el objeto
            ReleaseObject();
        }
    }

    private void OnGrabCanceled(InputAction.CallbackContext context)
    {
        // Opcional: soltar al cancelar la acción
        // ReleaseObject();
    }

    private void ReleaseObject()
    {
        if (grabbedObject == null) return;

        // Restaurar física
        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            // Aplicar velocidad para simular lanzamiento
            // rb.velocity = playerCamera.transform.forward * 5f;
        }

        // Restaurar color original si no está siendo apuntado
        if (currentRenderer != null && grabbedObject == currentGrabbable)
        {
            currentRenderer.material.color = hoverColor;
        }
        else if (currentRenderer != null)
        {
            currentRenderer.material.color = originalColor;
        }

        // Restaurar cursor normal si está configurado
        if (useMouseCursor && normalCursorTexture != null)
        {
            Cursor.SetCursor(normalCursorTexture, Vector2.zero, CursorMode.Auto);
        }

        grabbedObject = null;
    }

    private void UpdateGrabbedObjectPosition()
    {
        if (grabbedObject == null || playerCamera == null) return;

        // Calcular posición frente a la cámara basada en la posición del cursor
        Vector3 targetPosition;

        if (useMouseCursor)
        {
            // Si usamos cursor, mantener el objeto en el punto de mira del cursor
            Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            targetPosition = ray.origin + ray.direction * (grabDistance * 0.7f);
        }
        else
        {
            // Comportamiento original: frente a la cámara
            targetPosition = playerCamera.transform.position +
                            playerCamera.transform.forward * (grabDistance * 0.7f);
        }

        // Suavizar el movimiento
        grabbedObject.transform.position = Vector3.Lerp(
            grabbedObject.transform.position,
            targetPosition,
            Time.deltaTime * 10f
        );

        // Mantener rotación original o añadir rotación suave
        // grabbedObject.transform.rotation = Quaternion.Lerp(
        //     grabbedObject.transform.rotation,
        //     playerCamera.transform.rotation,
        //     Time.deltaTime * 5f
        // );
    }

    private void OnDestroy()
    {
        // Limpiar eventos
        if (grabAction.action != null)
        {
            grabAction.action.started -= OnGrabStarted;
            grabAction.action.canceled -= OnGrabCanceled;
        }
    }

    // Método para debug
    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;

            if (Application.isPlaying && useMouseCursor)
            {
                // Dibujar rayo desde la posición del cursor en tiempo de ejecución
                Vector2 mousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() :
                    new Vector2(Screen.width / 2, Screen.height / 2);
                Ray ray = playerCamera.ScreenPointToRay(mousePosition);
                Gizmos.DrawRay(ray.origin, ray.direction * grabDistance);
            }
            else
            {
                // Dibujar rayo desde el centro de la pantalla
                Vector3 rayStart = playerCamera.transform.position;
                Vector3 rayDirection = playerCamera.transform.forward * grabDistance;
                Gizmos.DrawRay(rayStart, rayDirection);
            }
        }
    }

    // Método público para cambiar entre modos de cursor
    public void SetUseMouseCursor(bool useCursor)
    {
        useMouseCursor = useCursor;

        if (useMouseCursor && normalCursorTexture != null)
        {
            Cursor.SetCursor(normalCursorTexture, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}