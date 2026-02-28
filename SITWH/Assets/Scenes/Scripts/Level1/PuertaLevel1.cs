using UnityEngine;
using FMODUnity;

public class PuertaLevel1 : MonoBehaviour
{
    public SingleTrigger triggerColor;
    public SingleTrigger triggerSitio;

    public float delayDestruccion = 0.5f;
    public GameObject efectoMagiaPrefab;
    public EventReference fmodEvent;

    private bool puertaAbierta = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (triggerColor == null)
            triggerColor = GameObject.Find("TriggerColor")?.GetComponent<SingleTrigger>();

        if (triggerSitio == null)
            triggerSitio = GameObject.Find("TriggerSitio")?.GetComponent<SingleTrigger>();
    }

    void Update()
    {
        if (!puertaAbierta && AmbosTriggersActivos())
            AbrirPuerta();
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
        Invoke(nameof(ActivarPuerta), delayDestruccion);
    }
    void ActivarPuerta()
    {
        if (efectoMagiaPrefab != null)
            Instantiate(efectoMagiaPrefab, transform.position, Quaternion.identity);

        if (animator != null)
            animator.SetBool("Open", true);

        if (!fmodEvent.IsNull)
        {
            var instancia = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);

            FMOD.ATTRIBUTES_3D attrs = FMODUnity.RuntimeUtils.To3DAttributes(transform);
            instancia.set3DAttributes(attrs);

            FMOD.RESULT resultado = instancia.start();
            if (resultado == FMOD.RESULT.OK)
                Debug.Log($"Evento FMOD '{fmodEvent.Path}' reproducido correctamente");
            else
                Debug.LogWarning($"Error al reproducir evento FMOD: {resultado}");

            instancia.release();
        }
        else
        {
            Debug.LogWarning("fmodEvent está vacío, no se puede reproducir sonido");
        }
    }
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 30), $"TriggerColor activo: {(triggerColor != null ? triggerColor.IsActive().ToString() : "null")}");
        GUI.Label(new Rect(10, 40, 300, 30), $"TriggerSitio activo: {(triggerSitio != null ? triggerSitio.IsActive().ToString() : "null")}");
        GUI.Label(new Rect(10, 70, 300, 30), $"Puerta Abierta: {puertaAbierta}");

        if (AmbosTriggersActivos() && !puertaAbierta)
            GUI.Label(new Rect(10, 100, 300, 30), "¡CONDICIÓN CUMPLIDA! Puerta se abrirá...");
    }
}