using UnityEngine;

public class PathController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidadMovimiento = 2f;   // Velocidad de desplazamiento de los hijos
    public bool moverHijos = true;           // Activar/desactivar el movimiento

    private Collider triggerCollider;         // Collider trigger del padre
    private Transform[] hijos;                // Referencias a los hijos
    private Vector3[] direcciones;            // Dirección actual de cada hijo

    void Start()
    {
        // Obtener el collider del padre y verificar que sea trigger
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null || !triggerCollider.isTrigger)
        {
            Debug.LogError("El objeto padre necesita un Collider configurado como Trigger.");
            enabled = false;  // Desactivar el script si no hay collider válido
            return;
        }

        int childCount = transform.childCount;
        hijos = new Transform[childCount];
        direcciones = new Vector3[childCount];

        for (int i = 0; i < childCount; i++)
        {
            hijos[i] = transform.GetChild(i);
            direcciones[i] = Random.insideUnitSphere.normalized;
        }
    }

    void Update()
    {
        if (!moverHijos) return;

        for (int i = 0; i < hijos.Length; i++)
        {
            if (hijos[i] == null) continue;

            // Calcular nueva posición
            Vector3 nuevaPos = hijos[i].position + direcciones[i] * velocidadMovimiento * Time.deltaTime;

            // Si la nueva posición está dentro del collider, mover
            if (EstaDentroDelCollider(nuevaPos))
            {
                hijos[i].position = nuevaPos;
            }
            else
            {
                // Si está fuera, cambiar dirección aleatoriamente (efecto "rebote suave")
                direcciones[i] = Random.insideUnitSphere.normalized;
            }
        }
    }

    bool EstaDentroDelCollider(Vector3 punto)
    {
        Vector3 puntoMasCercano = triggerCollider.ClosestPoint(punto);
        return puntoMasCercano == punto; // Si el punto más cercano es él mismo, está dentro
    }

    void OnDrawGizmosSelected()
    {
        if (triggerCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(triggerCollider.bounds.center, triggerCollider.bounds.size);
        }
    }
}