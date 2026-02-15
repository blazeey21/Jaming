using UnityEngine;
using UnityEngine.InputSystem;

public class CenterScreenGrab : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private LayerMask grabbableLayer;
    [SerializeField] private float grabDistance = 5f;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color grabbedColor = Color.green;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InputActionProperty grabAction;
    [SerializeField] private Transform centerSprite;
    [SerializeField] private float throwForce = 12f;

    [Header("Referencias")]
    [SerializeField] private GameObject crosshairUI;

    private GameObject currentGrabbable;
    private GameObject grabbedObject;
    private Renderer currentRenderer;
    private Material originalMaterial;
    private Color originalColor;

    private void Start()
    {
        if (grabAction.action == null)
        {
            Debug.LogWarning("No se ha asignado una acción de agarre. Se usará la tecla E por defecto.");
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (grabAction.action != null)
        {
            grabAction.action.started += OnGrabStarted;
            grabAction.action.canceled += OnGrabCanceled;
        }

        if (centerSprite != null) centerSprite.gameObject.SetActive(true);
        if (crosshairUI != null) crosshairUI.SetActive(true);
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
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
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
                }
            }
        }
        else
        {
            ResetCurrentGrabbable();
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
            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
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

        grabbedObject = null;
    }

    private void UpdateGrabbedObjectPosition()
    {
        if (grabbedObject == null || playerCamera == null) return;

        Vector3 targetPosition = playerCamera.transform.position +
                                playerCamera.transform.forward * (grabDistance * 0.7f);

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
            Vector3 rayStart = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward * grabDistance;
            Gizmos.DrawRay(rayStart, rayDirection);
        }
    }

    public void SetCenterSpriteColor(Color color)
    {
        if (centerSprite != null)
        {
            SpriteRenderer spriteRenderer = centerSprite.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }
    }
}
