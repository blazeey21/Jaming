using UnityEngine;
using System.Collections;

public class CalavereFlotando : MonoBehaviour
{
    [Header("Mov")]
    public float velocidad = 2f;

    [Header("Flot")]
    public float amplitudFlotacion = 0.5f;
    public float frecuenciaFlotacion = 2f;

    [Header("Rot")]
    public Vector3 velocidadRotacion = new Vector3(180f, 180f, 180f);

    [Header("Par")]
    public GameObject particulasPrefab;
    public float offsetSalida = 0.02f;
    public string capaDestructor = "Destructor";

    [Header("Frases al inicio")]
    public string frase1;
    public string frase2;
    public string frase3;

    private Transform jugador;
    private float yInicial;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) jugador = player.transform;
        yInicial = transform.position.y;

        StartCoroutine(SecuenciaFrases());
    }

    void Update()
    {
        if (jugador == null) return;

        Vector3 posicionActual = transform.position;
        Vector3 direccionHorizontal = jugador.position - posicionActual;
        direccionHorizontal.y = 0;

        if (direccionHorizontal.magnitude > 0.01f)
        {
            direccionHorizontal.Normalize();
            transform.Translate(direccionHorizontal * velocidad * Time.deltaTime, Space.World);
        }

        Vector3 nuevaPosicion = transform.position;
        nuevaPosicion.y = yInicial + Mathf.Sin(Time.time * frecuenciaFlotacion) * amplitudFlotacion;
        transform.position = nuevaPosicion;

        transform.Rotate(velocidadRotacion * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(capaDestructor))
        {
            Vector3 spawnPoint = GetSurfacePoint(transform.position, other);
            Instantiate(particulasPrefab, spawnPoint, Quaternion.identity);

            Destroy(other.gameObject);
        }
    }

    Vector3 GetSurfacePoint(Vector3 referencePoint, Collider targetCollider)
    {
        Vector3 closest = targetCollider.ClosestPoint(referencePoint);
        Vector3 direction = (closest - targetCollider.bounds.center).normalized;
        if (direction == Vector3.zero) direction = Vector3.up;
        return closest + direction * offsetSalida;
    }

    // ------------------- SECUENCIA DE FRASES -------------------

    IEnumerator SecuenciaFrases()
    {
        if (!string.IsNullOrEmpty(frase1))
            yield return StartCoroutine(PlayAndWait(frase1));

        if (!string.IsNullOrEmpty(frase2))
            yield return StartCoroutine(PlayAndWait(frase2));

        if (!string.IsNullOrEmpty(frase3))
            yield return StartCoroutine(PlayAndWait(frase3));
    }

    IEnumerator PlayAndWait(string id)
    {
        if (ElonTv.Instance == null) yield break;

        yield return ElonTv.Instance.StartCoroutine(
            ElonTv.Instance.PlayAndReturnRoutine(id)
        );
    }
}