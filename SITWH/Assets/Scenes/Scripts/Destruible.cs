using UnityEngine;
using System.Collections;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class Destruible : MonoBehaviour
{
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private bool destroyOnFloorTouch = true;
    [SerializeField] private float timeToDestroy = 0.5f;
    [SerializeField] private float floorCheckDelay = 3f;
    [SerializeField] private GameObject prefabDeParticulas;

    [Header("FMOD audio")]
    public EventReference fmodEvent;
    public EventReference audioDespuesDeRomper;

    private bool hasBeenGrabbed = false;
    private bool isCheckingFloor = false;
    private float floorContactTime = 0f;
    private Rigidbody rb;
    private Collider objectCollider;

    CrumpsLogic crumps;

    // Gestión de cola de audio
    private static bool isAudioPlaying = false;
    private static Queue<System.Func<IEnumerator>> audioQueue = new Queue<System.Func<IEnumerator>>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();

        GameObject c = GameObject.FindGameObjectWithTag("Crumps");
        if (c != null) crumps = c.GetComponent<CrumpsLogic>();

        if (floorLayer.value == 0)
            floorLayer = LayerMask.GetMask("Floor");

        floorContactTime = 0.11f;
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
                    EnqueueAudioDestruction();
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

    private void EnqueueAudioDestruction()
    {
        audioQueue.Enqueue(() => HandleDestructionWithFMOD());
        if (!isAudioPlaying)
            StartCoroutine(ProcessAudioQueue());
    }

    private IEnumerator ProcessAudioQueue()
    {
        isAudioPlaying = true;
        while (audioQueue.Count > 0)
        {
            var audioCoroutine = audioQueue.Dequeue();
            yield return StartCoroutine(audioCoroutine());
        }
        isAudioPlaying = false;
    }

    private IEnumerator HandleDestructionWithFMOD()
    {
        // SONIDO DE ROMPERSE
        if (!fmodEvent.IsNull)
        {
            var instance = RuntimeManager.CreateInstance(fmodEvent);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            instance.start();

            instance.getDescription(out EventDescription desc);
            desc.getLength(out int lengthMs);

            instance.release();

            yield return new WaitForSeconds(lengthMs / 1000f);
        }

        // ESPERAR 3 SEGUNDOS
        yield return new WaitForSeconds(3f);

        // AUDIO EXTRA
        if (!audioDespuesDeRomper.IsNull)
        {
            var instance = RuntimeManager.CreateInstance(audioDespuesDeRomper);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            instance.start();
            instance.release();
        }

        // DESTRUIR O DESACTIVAR
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