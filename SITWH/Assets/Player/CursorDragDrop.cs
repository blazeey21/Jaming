using UnityEngine;
using UnityEngine.InputSystem;

public class CursorDragDrop : MonoBehaviour
{
    [SerializeField] private Sprite cursorSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color grabColor = Color.green;
    [SerializeField] private Vector2 cursorOffset = new Vector2(0f, -100f);

    [SerializeField] private float grabDistance = 5f;
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private LayerMask grabMask;

    [SerializeField] private float holdDistance = 2f;
    [SerializeField] private float smoothFollowSpeed = 15f;
    [SerializeField] private float verticalHoldOffset = -0.4f;

    private Camera mainCamera;
    private SpriteRenderer cursorRenderer;
    private Rigidbody grabbedRigidbody;
    private Vector2 screenCenter;
    private Vector3 currentHoldPosition;
    private bool isGrabbing;
    private bool canGrab;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Camera[] cams = FindObjectsOfType<Camera>();
            if (cams.Length == 0)
            {
                enabled = false;
                return;
            }
            mainCamera = cams[0];
        }

        GameObject cursorObject = new GameObject("CursorSprite");
        cursorRenderer = cursorObject.AddComponent<SpriteRenderer>();
        cursorRenderer.sprite = cursorSprite;
        cursorRenderer.sortingOrder = 999;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
        screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    void Update()
    {
        UpdateCursorPosition();
        CheckForGrabbableObjects();
        HandleGrabbedObject();
        UpdateCursorAppearance();
    }

    void UpdateCursorPosition()
    {
        Vector3 screenPos = new Vector3(
            screenCenter.x + cursorOffset.x,
            screenCenter.y + cursorOffset.y,
            mainCamera.nearClipPlane + 0.1f
        );

        cursorRenderer.transform.position = mainCamera.ScreenToWorldPoint(screenPos);
        cursorRenderer.transform.LookAt(mainCamera.transform);
        cursorRenderer.transform.Rotate(0, 180, 0);
    }

    void CheckForGrabbableObjects()
    {
        if (isGrabbing)
        {
            canGrab = false;
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabMask))
        {
            Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();
            canGrab = rb != null;
        }
        else
        {
            canGrab = false;
        }
    }

    void HandleGrabbedObject()
    {
        if (!isGrabbing || grabbedRigidbody == null) return;

        Vector3 basePos = mainCamera.transform.position + mainCamera.transform.forward * holdDistance;
        Vector3 offset = mainCamera.transform.up * verticalHoldOffset;
        Vector3 target = basePos + offset;

        currentHoldPosition = Vector3.Lerp(
            currentHoldPosition,
            target,
            smoothFollowSpeed * Time.deltaTime
        );

        grabbedRigidbody.transform.position = currentHoldPosition;
    }

    void UpdateCursorAppearance()
    {
        if (isGrabbing)
            cursorRenderer.color = grabColor;
        else if (canGrab)
            cursorRenderer.color = hoverColor;
        else
            cursorRenderer.color = normalColor;
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (!isGrabbing)
            TryGrabObject();
        else
            ReleaseObject();
    }


    void TryGrabObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        if (!Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabMask)) return;

        grabbedRigidbody = hit.collider.GetComponentInParent<Rigidbody>();
        if (grabbedRigidbody == null) return;

        grabbedRigidbody.isKinematic = true;
        currentHoldPosition = grabbedRigidbody.transform.position;
        isGrabbing = true;
    }

    void ReleaseObject()
    {
        if (grabbedRigidbody == null) return;

        grabbedRigidbody.isKinematic = false;
        grabbedRigidbody.AddForce(mainCamera.transform.forward * throwForce, ForceMode.Impulse);

        grabbedRigidbody = null;
        isGrabbing = false;
    }

    void OnDestroy()
    {
        if (cursorRenderer != null)
            Destroy(cursorRenderer.gameObject);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
