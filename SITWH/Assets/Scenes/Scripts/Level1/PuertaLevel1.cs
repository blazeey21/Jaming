using UnityEngine;
using System.Collections;

public class PuertaLevel1 : MonoBehaviour
{
    public SingleTrigger triggerColor;
    public SingleTrigger triggerSitio;

    public float delayDestruccion = 0.5f;
    public GameObject efectoMagiaPrefab;

    [Header("Start Sec (IDs de TVelon)")]
    public string A;
    public string B;
    public string C;
    public string D;
    public string E;
    public string F;
    public string Pista1;
    public string pista2;

    [Header("Dead sequence")]
    public string dead1;
    public string dead2;
    public string dead3;

    [Header("Radio sequence after death")]
    public string[] radioSequence;

    [Header("Light control")]
    public Light targetLight;
    public float lightFinalIntensity = 0.2f;

    [Header("Disable object")]
    public GameObject objectToDisable;

    [Header("Disable object")]
 
    private bool puertaAbierta = false;
    private Animator animator;
    public FMODUnity.EventReference ambienceEvent;
   


    void Start()
    {
        animator = GetComponent<Animator>();

        if (triggerColor == null)
            triggerColor = GameObject.Find("TriggerColor")?.GetComponent<SingleTrigger>();

        if (triggerSitio == null)
            triggerSitio = GameObject.Find("TriggerSitio")?.GetComponent<SingleTrigger>();

        StartCoroutine(SecuenciaInicio());
    }

    IEnumerator PlayAndWait(string id)
    {
        if (string.IsNullOrEmpty(id)) yield break;
        if (ElonTv.Instance == null) yield break;

        yield return ElonTv.Instance.StartCoroutine(
            ElonTv.Instance.PlayAndReturnRoutine(id)
        );
    }

    IEnumerator SecuenciaInicio()
    {
        yield return PlayAndWait(A);
        yield return PlayAndWait(B);
        yield return PlayAndWait(C);
        yield return PlayAndWait(D);
        yield return PlayAndWait(E);
        yield return PlayAndWait(F);
        yield return new WaitForSeconds(2f);
        yield return PlayAndWait(Pista1);
        yield return new WaitForSeconds(1f);
        yield return PlayAndWait(pista2);
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

        yield return PlayAndWait(dead1);

        if (objectToDisable != null)
            objectToDisable.SetActive(false);

        yield return PlayAndWait(dead2);
        yield return PlayAndWait(dead3);

        if (targetLight != null)
            StartCoroutine(FadeLight(targetLight, targetLight.intensity, lightFinalIntensity, 3f));
        FMODUnity.RuntimeManager.PlayOneShot(ambienceEvent);
        GetComponent<Collider>().enabled = false;
        ActivarPuerta();
        StartCoroutine(SecuenciaRadio());
    }

    IEnumerator SecuenciaRadio()
    {
        yield return new WaitForSeconds(5f);
        
        for (int i = 0; i < radioSequence.Length; i++)
        {
            yield return PlayAndWait(radioSequence[i]);
            yield return new WaitForSeconds(0.4f);
        }
        
    }

    void ActivarPuerta()
    {
        if (efectoMagiaPrefab != null)
            Instantiate(efectoMagiaPrefab, transform.position, Quaternion.identity);

        if (animator != null)
            animator.SetBool("Open", true);
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
        l.enabled = false;
    }
}