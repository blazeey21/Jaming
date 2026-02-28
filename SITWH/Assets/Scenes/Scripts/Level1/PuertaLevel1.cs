using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

public class PuertaLevel1 : MonoBehaviour
{
    public SingleTrigger triggerColor;
    public SingleTrigger triggerSitio;

    public float delayDestruccion = 0.5f;
    public GameObject efectoMagiaPrefab;
    public EventReference fmodEvent;

    [Header("Dead sequence")]
    public EventReference dead1;
    public EventReference dead2;
    public EventReference dead3;

    [Header("Light control")]
    public Light targetLight;
    public float lightFinalIntensity = 0.2f;

    [Header("Disable object")]
    public GameObject objectToDisable;

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
        StartCoroutine(SecuenciaApertura());
    }

    IEnumerator SecuenciaApertura()
    {
        yield return new WaitForSeconds(delayDestruccion);

        float d1 = PlayAndGetDuration(dead1);
        yield return new WaitForSeconds(d1);

        float d2 = GetDuration(dead2);
        float d3 = GetDuration(dead3);

        if (objectToDisable != null)
            objectToDisable.SetActive(false);

        if (targetLight != null)
            StartCoroutine(FadeLight(targetLight, targetLight.intensity, lightFinalIntensity, d2 + d3));

        Play(dead2);
        yield return new WaitForSeconds(d2);

        Play(dead3);
        yield return new WaitForSeconds(d3);

        ActivarPuerta();
    }

    void ActivarPuerta()
    {
        if (efectoMagiaPrefab != null)
            Instantiate(efectoMagiaPrefab, transform.position, Quaternion.identity);

        if (animator != null)
            animator.SetBool("Open", true);

        if (!fmodEvent.IsNull)
        {
            var instancia = RuntimeManager.CreateInstance(fmodEvent);
            instancia.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            instancia.start();
            instancia.release();
        }
    }

    float PlayAndGetDuration(EventReference ev)
    {
        float duration = GetDuration(ev);
        Play(ev);
        return duration;
    }

    void Play(EventReference ev)
    {
        if (ev.IsNull) return;
        var instance = RuntimeManager.CreateInstance(ev);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        instance.start();
        instance.release();
    }

    float GetDuration(EventReference ev)
    {
        if (ev.IsNull) return 0f;
        var instance = RuntimeManager.CreateInstance(ev);
        instance.getDescription(out EventDescription desc);
        desc.getLength(out int lengthMs);
        instance.release();
        return lengthMs / 1000f;
    }

    IEnumerator FadeLight(Light l, float from, float to, float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            l.intensity = Mathf.Lerp(from, to, t / time);
            yield return null;
        }
        l.intensity = to;
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