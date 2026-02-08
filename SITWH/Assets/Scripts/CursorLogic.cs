using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLogic : MonoBehaviour
{
    [SerializeField] private LayerMask grabbableLayer;
    [SerializeField] private float grabDistance = 5f;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color grabbedColor = Color.green;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InputActionProperty grabAction;
    [SerializeField] private bool useMouseCursor = true;
    [SerializeField] private Texture2D grabCursorTexture;
    [SerializeField] private Texture2D normalCursorTexture;

    private GameObject currentGrabbable;
    private GameObject grabbedObject;
    private Renderer currentRenderer;
    private Material originalMaterial;
    private Color originalColor;
    private Vector2 lastMousePosition;

    private void Start()
    {
        if (grabAction.action == null)
        {
            Debug.LogWarning("No se ha asignado una acción de agarre. Se usará la tecla E por defecto.");
        }

        if (useMouseCursor && normalCursorTexture != null)
        {
            Cursor.SetCursor(normalCursorTexture, Vector2.zero, CursorMode.Auto);
        }

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
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (mousePosition == lastMousePosition && currentGrabbable != null && grabbedObject == null)
            {
                return;
            }

            lastMousePosition = mousePosition;
            ray = playerCamera.ScreenPointToRay(mousePosition);
        }
        else
        {
            ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, grabDistance, grabbableLayer))
        {
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

                    if (useMouseCursor && grabCursorTexture != null)
                    {
                        Cursor.SetCursor(grabCursorTexture, Vector2.zero, CursorMode.Auto);
                    }
                }
            }
        }
        else
        {
            ResetCurrentGrabbable();

            if (useMouseCursor && normalCursorTexture != null && grabbedObject == null)
            {
                Cursor.SetCursor(normalCursorTexture, Vector2.zero, CursorMode.Auto);
            }
        }

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
            grabbedObject = currentGrabbable;

            Destruible destruible = grabbedObject.GetComponent<Destruible>();
            if (destruible != null)
            {
                destruible.OnGrabbed();
            }

            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            if (currentRenderer != null)
            {
                currentRenderer.material.color = grabbedColor;
            }

            if (useMouseCursor && grabCursorTexture != null)
            {
                Cursor.SetCursor(grabCursorTexture, Vector2.zero, CursorMode.Auto);
            }
        }
        else if (grabbedObject != null)
        {
            ReleaseObject();
        }
    }

    private void OnGrabCanceled(InputAction.CallbackContext context)
    {
    }

    private void ReleaseObject()
    {
        if (grabbedObject == null) return;

        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Destruible destruible = grabbedObject.GetComponent<Destruible>();
        if (destruible != null)
        {
            destruible.OnReleased();
        }

        if (currentRenderer != null && grabbedObject == currentGrabbable)
        {
            currentRenderer.material.color = hoverColor;
        }
        else if (currentRenderer != null)
        {
            currentRenderer.material.color = originalColor;
        }

        if (useMouseCursor && normalCursorTexture != null)
        {
            Cursor.SetCursor(normalCursorTexture, Vector2.zero, CursorMode.Auto);
        }

        grabbedObject = null;
    }

    private void UpdateGrabbedObjectPosition()
    {
        if (grabbedObject == null || playerCamera == null) return;

        Vector3 targetPosition;

        if (useMouseCursor)
        {
            Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            targetPosition = ray.origin + ray.direction * (grabDistance * 0.7f);
        }
        else
        {
            targetPosition = playerCamera.transform.position +
                            playerCamera.transform.forward * (grabDistance * 0.7f);
        }

        grabbedObject.transform.position = Vector3.Lerp(
            grabbedObject.transform.position,
            targetPosition,
            Time.deltaTime * 10f
        );
    }

    private void OnDestroy()
    {
        if (grabAction.action != null)
        {
            grabAction.action.started -= OnGrabStarted;
            grabAction.action.canceled -= OnGrabCanceled;
        }
    }

    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;

            if (Application.isPlaying && useMouseCursor)
            {
                Vector2 mousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() :
                    new Vector2(Screen.width / 2, Screen.height / 2);
                Ray ray = playerCamera.ScreenPointToRay(mousePosition);
                Gizmos.DrawRay(ray.origin, ray.direction * grabDistance);
            }
            else
            {
                Vector3 rayStart = playerCamera.transform.position;
                Vector3 rayDirection = playerCamera.transform.forward * grabDistance;
                Gizmos.DrawRay(rayStart, rayDirection);
            }
        }
    }

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