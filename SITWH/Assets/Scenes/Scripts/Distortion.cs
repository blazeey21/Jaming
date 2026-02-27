using UnityEngine;

public class Distortion : MonoBehaviour
{
    public Transform player;
    public Material mat;
    public float maxDistance = 10f;

    [Header("Living Effect Settings")]
    public float speed = 1f;
    public float intensity = 0.1f;
    public float noiseScale = 2f;

    private Vector3 originalScale;
    private float offsetX;
    private float offsetZ;

    void Start()
    {
        originalScale = transform.localScale;
        offsetX = Random.Range(0f, 100f);
        offsetZ = Random.Range(0f, 100f);
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        float t = Mathf.InverseLerp(maxDistance, 0, dist);
        mat.SetFloat("_DistortionStrength", t * 0.05f);

        // Usar Perlin Noise para un movimiento más natural
        float breathX = Mathf.PerlinNoise(Time.time * speed + offsetX, 0) * 2 - 1;
        float breathZ = Mathf.PerlinNoise(Time.time * speed + offsetZ, 1) * 2 - 1;

        Vector3 newScale = originalScale;
        newScale.x = originalScale.x * (1f + breathX * intensity);
        newScale.z = originalScale.z * (1f + breathZ * intensity);

        transform.localScale = newScale;
    }
}