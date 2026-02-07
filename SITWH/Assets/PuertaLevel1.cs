using UnityEngine;

public class PuertaLevel1 : MonoBehaviour
{
    [Header("Triggers específicos")]
    public SingleTrigger triggerColor;
    public SingleTrigger triggerSitio;

    [Header("Configuración")]
    public bool destruirPuerta = true;
    public float delayDestruccion = 0.5f;

    private bool puertaAbierta = false;

    void Start()
    {
        // Buscar automáticamente si no están asignados
        if (triggerColor == null)
        {
            triggerColor = GameObject.Find("TriggerColor")?.GetComponent<SingleTrigger>();
        }

        if (triggerSitio == null)
        {
            triggerSitio = GameObject.Find("TriggerSitio")?.GetComponent<SingleTrigger>();
        }

        if (triggerColor == null)
            Debug.LogError("❌ No se encontró TriggerColor");
        if (triggerSitio == null)
            Debug.LogError("❌ No se encontró TriggerSitio");

        if (triggerColor != null && triggerSitio != null)
            Debug.Log($"🚪 Puerta '{gameObject.name}' lista. Esperando ambos triggers...");
    }

    void Update()
    {
        if (!puertaAbierta && AmbosTriggersActivos())
        {
            AbrirPuerta();
        }
    }

    bool AmbosTriggersActivos()
    {
        bool colorActivo = triggerColor != null && triggerColor.IsActive();
        bool sitioActivo = triggerSitio != null && triggerSitio.IsActive();

        return colorActivo && sitioActivo;
    }

    void AbrirPuerta()
    {
        puertaAbierta = true;
        Debug.Log($"🎉 ¡PUERTA ABIERTA! Ambos triggers activos");

        Invoke(nameof(ProcesarPuerta), delayDestruccion);
    }

    void ProcesarPuerta()
    {
        if (destruirPuerta)
        {
            Debug.Log($"🗑️ Destruyendo puerta: {gameObject.name}");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"🔒 Desactivando puerta: {gameObject.name}");
            gameObject.SetActive(false);
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 30), $"TriggerColor activo: {(triggerColor != null ? triggerColor.IsActive().ToString() : "null")}");
        GUI.Label(new Rect(10, 40, 300, 30), $"TriggerSitio activo: {(triggerSitio != null ? triggerSitio.IsActive().ToString() : "null")}");
        GUI.Label(new Rect(10, 70, 300, 30), $"Puerta Abierta: {puertaAbierta}");

        if (AmbosTriggersActivos() && !puertaAbierta)
        {
            GUI.Label(new Rect(10, 100, 300, 30), "¡CONDICIÓN CUMPLIDA! Puerta se abrirá...");
        }
    }
}