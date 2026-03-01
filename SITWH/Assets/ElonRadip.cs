using UnityEngine;
using System.Collections;

public class ElonRadip : MonoBehaviour
{
    [Header("Player Tag")]
    public string playerTag = "Player";

    [Header("Trigger Collider")]
    public Collider triggerCollider; // Collider que activa la secuencia

    [Header("Sequence IDs")]
    public string[] radios =
    {
        "Radio1","Radio2","Radio3","Radio4","Radio5","Radio6","Radio7","Radio8",
        "Radio9","Radio10","Radio11","Radio12","Radio13","Radio14","Radio15","Radio16"
    };

    private bool activado = false;

    void OnTriggerEnter(Collider other)
    {
        if (activado) return;

        if (other.CompareTag(playerTag) && triggerCollider != null && triggerCollider.bounds.Intersects(other.bounds))
        {
            activado = true;
            StartCoroutine(SecuenciaCerraduras());
        }
    }

    IEnumerator SecuenciaCerraduras()
    {
        foreach (var id in radios)
        {
            yield return StartCoroutine(PlayAndWait(id));
        }
    }

    IEnumerator PlayAndWait(string id)
    {
        if (ElonTv.Instance == null) yield break;

        yield return ElonTv.Instance.StartCoroutine(
            ElonTv.Instance.PlayAndReturnRoutine(id)
        );
    }
}