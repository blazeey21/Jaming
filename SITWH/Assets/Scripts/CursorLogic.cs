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

    private GameObject currentGrabbable;
    private GameObject grabbedObject;
    private Renderer currentRenderer;
    private Material originalMaterial;
    private Color originalColor;

    private void Start()
    {
        // Inicializar la acción de agarre si no está asignada
        if (grabAction.action == null)
        {
            Debug.LogWarning("No se ha asignado una acción de agarre. Se usará la tecla E por defecto.");
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
    }

    private void Update()
    {
        // Raycast desde la cámara hacia adelante
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
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
                }
            }
        }
        else
        {
            // Si no estamos apuntando a ningún objeto agarrable
            ResetCurrentGrabbable();
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

        grabbedObject = null;
    }

    private void UpdateGrabbedObjectPosition()
    {
        if (grabbedObject == null || playerCamera == null) return;

        // Calcular posición frente a la cámara
        Vector3 targetPosition = playerCamera.transform.position +
                                playerCamera.transform.forward * (grabDistance * 0.7f);

        // Suavizar el movimiento (opcional)
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

    // Método para debug (opcional)
    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Vector3 rayStart = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward * grabDistance;
            Gizmos.DrawRay(rayStart, rayDirection);
        }
    }
}