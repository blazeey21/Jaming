using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class Hallmessage : MonoBehaviour
{
    [Header("Player")]
    public GameObject player; // mejor usar GameObject para poder activar/desactivar

    [Header("Dialogue Sequence IDs")]
    public string[] hallDialogues;

    [Header("Cinematica")]
    public CinemachineCamera vCamCinematica;
    public PlayableDirector director;
    public CinemachineCamera vCamJugador;

    [Header("Opciones")]
    public float priorityCinematica = 20f;
    public float priorityJugador = 10f;

    private bool activado = false;

    void OnTriggerEnter(Collider other)
    {
        if (activado) return;

        if (player != null && other.transform == player.transform)
        {
            activado = true;
            StartCoroutine(SecuenciaHallSimultanea());
        }
    }

    IEnumerator SecuenciaHallSimultanea()
    {
        // Desactivamos al player mientras dura la cinemática y los diálogos
        player.SetActive(false);

        // Guardamos prioridades originales
        int oldPriorityCine = vCamCinematica.Priority;
        int oldPriorityJugador = vCamJugador.Priority;

        // Activamos la cinemática
        if (vCamCinematica != null && vCamJugador != null)
        {
            vCamCinematica.Priority = (int)priorityCinematica;
        }

        // Lanzamos la cinemática y los diálogos en paralelo
        Coroutine cinem = null;
        if (director != null)
        {
            cinem = StartCoroutine(PlayCinematica());
        }

        Coroutine dialog = StartCoroutine(PlayDialogos());

        // Esperamos a que ambos terminen
        if (cinem != null) yield return cinem;
        yield return dialog;

        // Restauramos prioridades originales
        if (vCamCinematica != null && vCamJugador != null)
        {
            vCamCinematica.Priority = oldPriorityCine;
            vCamJugador.Priority = oldPriorityJugador;
        }

        // Reactivamos al player
        player.SetActive(true);
    }

    IEnumerator PlayCinematica()
    {
        director.Play();
        yield return new WaitForSeconds((float)director.duration);
    }

    IEnumerator PlayDialogos()
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