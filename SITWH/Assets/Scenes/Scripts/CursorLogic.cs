
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CenterScreenGrab : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private LayerMask grabbableLayer;
    [SerializeField] private float grabDistance = 5f;
    [SerializeField] private float holdDistance = 1.2f;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color grabbedColor = Color.green;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private InputActionProperty grabAction;
    [SerializeField] private Transform centerSprite;
    [SerializeField] private float throwForce = 12f;

    [Header("Zoom")]
    [SerializeField] private InputActionProperty zoomAction;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float zoomFOV = 30f;
    [SerializeField] private float zoomSpeed = 10f;

    [Header("Referencias")]
    [SerializeField] private GameObject crosshairUI;

    private GameObject currentGrabbable;
    private GameObject grabbedObject;
    private Renderer currentRenderer;
    private Material originalMaterial;
    private Color originalColor;

    private bool isZooming;

    private void Start()
    {
        SetFOV(normalFOV);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (grabAction.action != null)
        {
            grabAction.action.started += OnGrabStarted;
            grabAction.action.canceled += OnGrabCanceled;
        }

        if (zoomAction.action != null)
        {
            zoomAction.action.started += OnZoomStarted;
            zoomAction.action.canceled += OnZoomCanceled;
        }

        if (centerSprite != null) centerSprite.gameObject.SetActive(true);
        if (crosshairUI != null) crosshairUI.SetActive(true);
    }

    private void OnEnable()
    {
        if (grabAction.action != null) grabAction.action.Enable();
        if (zoomAction.action != null) zoomAction.action.Enable();
    }

    private void OnDisable()
    {
        if (grabAction.action != null) grabAction.action.Disable();
        if (zoomAction.action != null) zoomAction.action.Disable();

        ResetCurrentGrabbable();
        ReleaseObject();
    }

    private void Update()
    {
        UpdateZoom();

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

    private void UpdateZoom()
    {
        float currentFOV = GetFOV();
        float targetFOV = isZooming ? zoomFOV : normalFOV;
        float newFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * zoomSpeed);
        SetFOV(newFOV);
    }

    private float GetFOV()
    {
        if (virtualCamera != null) return virtualCamera.Lens.FieldOfView;
        if (playerCamera != null) return playerCamera.fieldOfView;
        return normalFOV;
    }

    private void SetFOV(float value)
    {
        if (virtualCamera != null)
        {
            var lens = virtualCamera.Lens;
            lens.FieldOfView = value;
            virtualCamera.Lens = lens;
        }
        else if (playerCamera != null)
        {
            playerCamera.fieldOfView = value;
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
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.detectCollisions = false;
            }

            Collider[] cols = grabbedObject.GetComponentsInChildren<Collider>();
            foreach (Collider col in cols) col.enabled = false;

            Destruible destruible = grabbedObject.GetComponent<Destruible>();
            if (destruible != null) destruible.OnGrabbed();

            if (currentRenderer != null) currentRenderer.material.color = grabbedColor;
        }
        else if (grabbedObject != null)
        {
            ReleaseObject();
        }
    }

    private void OnGrabCanceled(InputAction.CallbackContext context) { }

    private void OnZoomStarted(InputAction.CallbackContext context)
    {
        isZooming = true;
    }

    private void OnZoomCanceled(InputAction.CallbackContext context)
    {
        isZooming = false;
    }

    private void ReleaseObject()
    {
        if (grabbedObject == null) return;
        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.detectCollisions = true;
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Collider[] cols = grabbedObject.GetComponentsInChildren<Collider>();
        foreach (Collider col in cols) col.enabled = true;

        if (rb != null)
        {
            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
        }

        Destruible destruible = grabbedObject.GetComponent<Destruible>();
        if (destruible != null) destruible.OnReleased();

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

        Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;

        grabbedObject.transform.position = Vector3.Lerp(
            grabbedObject.transform.position,
            targetPosition,
            Time.deltaTime * 15f
        );
    }

    private void OnDestroy()
    {
        if (grabAction.action != null)
        {
            grabAction.action.started -= OnGrabStarted;
            grabAction.action.canceled -= OnGrabCanceled;
        }

        if (zoomAction.action != null)
        {
            zoomAction.action.started -= OnZoomStarted;
            zoomAction.action.canceled -= OnZoomCanceled;
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
            if (spriteRenderer != null) spriteRenderer.color = color;
        }
    }
}