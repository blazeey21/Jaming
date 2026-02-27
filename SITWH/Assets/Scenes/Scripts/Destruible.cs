using UnityEngine;
using System.Collections;

public class Destruible : MonoBehaviour
{
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private bool destroyOnFloorTouch = true;
    [SerializeField] private float timeToDestroy = 0.5f;
    [SerializeField] private float floorCheckDelay = 3f;
    [SerializeField] private GameObject prefabDeParticulas;

    private bool hasBeenGrabbed = false;
    private bool isCheckingFloor = false;
    private float floorContactTime = 0f;
    private Rigidbody rb;
    private Collider objectCollider;

    CrumpsLogic crumps;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();

        GameObject c = GameObject.FindGameObjectWithTag("Crumps");
        if (c != null) crumps = c.GetComponent<CrumpsLogic>();

        if (floorLayer.value == 0)
            floorLayer = LayerMask.GetMask("Floor");
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
                    NotifyCrumps();
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

    void NotifyCrumps()
    {
        if (crumps == null) return;

        if (CompareTag("ObjectosCrumpsGood"))
            crumps.OnGoodObjectDestroyed(transform.position);

        else if (CompareTag("ObjectosCrumpsBad"))
            crumps.OnBadObjectDestroyed(transform.position);
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
            StartCoroutine(StartFloorCheckAfterDelay(0.5f));
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

        Vector3[] rayOrigins =
        {
            transform.position,
            transform.position + new Vector3(objectCollider.bounds.extents.x * 0.8f,0, objectCollider.bounds.extents.z * 0.8f),
            transform.position + new Vector3(-objectCollider.bounds.extents.x * 0.8f,0, objectCollider.bounds.extents.z * 0.8f),
            transform.position + new Vector3(objectCollider.bounds.extents.x * 0.8f,0,-objectCollider.bounds.extents.z * 0.8f),
            transform.position + new Vector3(-objectCollider.bounds.extents.x * 0.8f,0,-objectCollider.bounds.extents.z * 0.8f)
        };

        int hits = 0;

        foreach (var origin in rayOrigins)
        {
            if (Physics.Raycast(origin, Vector3.down, raycastDistance, floorLayer))
                hits++;
        }

        return hits >= 3;
    }

    private void HandleDestruction()
    {
        if (destroyOnFloorTouch)
            Destroy(gameObject, timeToDestroy);
        else
            gameObject.SetActive(false);

        enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasBeenGrabbed &&
            IsLayerInMask(collision.gameObject.layer, floorLayer) &&
            IsOnFloor() &&
            prefabDeParticulas != null)
        {
            int count = GetParticleCountBySize();

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = collision.contacts.Length > 0
                    ? collision.contacts[0].point + Random.insideUnitSphere * 0.2f
                    : transform.position;

                Instantiate(prefabDeParticulas, spawnPos, Quaternion.identity);
            }
        }

        if (hasBeenGrabbed && !isCheckingFloor && IsLayerInMask(collision.gameObject.layer, floorLayer))
            StartCoroutine(StartFloorCheckAfterDelay(0.5f));
    }
    int GetParticleCountBySize()
    {
        if (objectCollider == null) return 1;

        Vector3 size = objectCollider.bounds.size;
        float volume = size.x * size.y * size.z;

        float normalized = Mathf.Clamp01(volume / 3f);
        return Mathf.Clamp(Mathf.CeilToInt(normalized * 6f), 1, 6);
    }

    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        return mask == (mask | (1 << layer));
    }
}