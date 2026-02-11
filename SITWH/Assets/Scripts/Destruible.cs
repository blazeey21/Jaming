using UnityEngine;
using System.Collections;

public class Destruible : MonoBehaviour
{
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private bool destroyOnFloorTouch = true;
    [SerializeField] private float timeToDestroy = 0.5f;
    [SerializeField] private float floorCheckDelay = 3f;

    private bool hasBeenGrabbed = false;
    private bool isCheckingFloor = false;
    private float floorContactTime = 0f;
    private Rigidbody rb;
    private Collider objectCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();

        if (floorLayer.value == 0)
        {
            Debug.LogWarning($"No se ha asignado una capa para el suelo en {gameObject.name}. Se usará 'Floor' por defecto.");
            floorLayer = LayerMask.GetMask("Floor");
        }
    }

    void Update()
    {
        if (hasBeenGrabbed && isCheckingFloor)
        {
            if (IsOnFloor())
            {
                floorContactTime += Time.deltaTime;

                if (floorContactTime >= floorCheckDelay)
                {
                    HandleDestruction();
                    isCheckingFloor = false;
                }
            }
            else
            {
                floorContactTime = 0f;
            }
        }
    }

    public void OnGrabbed()
    {
        hasBeenGrabbed = true;
        isCheckingFloor = false;
        floorContactTime = 0f;
    }

    public void OnReleased()
    {
        if (hasBeenGrabbed)
        {
            StartCoroutine(StartFloorCheckAfterDelay(0.5f));
        }
    }

    private IEnumerator StartFloorCheckAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isCheckingFloor = true;
        floorContactTime = 0f;
    }

    private bool IsOnFloor()
    {
        if (objectCollider == null) return false;

        float raycastDistance = objectCollider.bounds.extents.y + 0.2f;
        Vector3[] rayOrigins = new Vector3[5];

        rayOrigins[0] = transform.position;

        Vector3 extents = objectCollider.bounds.extents * 0.8f;
        rayOrigins[1] = transform.position + new Vector3(extents.x, 0, extents.z);
        rayOrigins[2] = transform.position + new Vector3(-extents.x, 0, extents.z);
        rayOrigins[3] = transform.position + new Vector3(extents.x, 0, -extents.z);
        rayOrigins[4] = transform.position + new Vector3(-extents.x, 0, -extents.z);

        int floorHits = 0;

        for (int i = 0; i < rayOrigins.Length; i++)
        {
            Ray ray = new Ray(rayOrigins[i], Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastDistance, floorLayer))
            {
                floorHits++;
            }
        }

        return floorHits >= 3;
    }

    private void HandleDestruction()
    {
        if (destroyOnFloorTouch)
        {
            Destroy(gameObject, timeToDestroy);
        }
        else
        {
            gameObject.SetActive(false);
        }

        enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasBeenGrabbed && !isCheckingFloor && IsLayerInMask(collision.gameObject.layer, floorLayer))
        {
            StartCoroutine(StartFloorCheckAfterDelay(0.5f));
        }
    }

    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        return mask == (mask | (1 << layer));
    }

    private void OnDrawGizmosSelected()
    {
        if (objectCollider != null && isCheckingFloor)
        {
            Gizmos.color = Color.cyan;
            float raycastDistance = objectCollider.bounds.extents.y + 0.2f;

            Vector3[] rayOrigins = new Vector3[5];
            Vector3 extents = objectCollider.bounds.extents * 0.8f;

            rayOrigins[0] = transform.position;
            rayOrigins[1] = transform.position + new Vector3(extents.x, 0, extents.z);
            rayOrigins[2] = transform.position + new Vector3(-extents.x, 0, extents.z);
            rayOrigins[3] = transform.position + new Vector3(extents.x, 0, -extents.z);
            rayOrigins[4] = transform.position + new Vector3(-extents.x, 0, -extents.z);

            for (int i = 0; i < rayOrigins.Length; i++)
            {
                Gizmos.DrawRay(rayOrigins[i], Vector3.down * raycastDistance);
            }
        }
    }
}