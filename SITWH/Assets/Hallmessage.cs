using UnityEngine;
using System.Collections;

public class Hallmessage : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Trigger Collider")]
    public Collider triggerCollider;

    [Header("Dialogue Sequence IDs")]
    public string[] hallDialogues; 
    private bool activado = false;

    void OnTriggerEnter(Collider other)
    {
        if (activado) return;

        if (player != null && triggerCollider != null && other == triggerCollider)
        {
            activado = true;
            StartCoroutine(SecuenciaHall());
        }
    }

    IEnumerator SecuenciaHall()
    {
        foreach (var id in hallDialogues)
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