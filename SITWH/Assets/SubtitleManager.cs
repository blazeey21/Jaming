using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance;
    public GameObject panel;
    public TextMeshProUGUI subtitleText;
    Coroutine routine;

    void Awake()
    {
        Instance = this;
    }

    public void ShowWhileAudio(string text, AudioSource source)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(text, source));
    }

    IEnumerator ShowRoutine(string text, AudioSource source)
    {
        panel.SetActive(true);
        subtitleText.text = text;
        subtitleText.gameObject.SetActive(true);

        yield return new WaitWhile(() => source != null && source.isPlaying);

        subtitleText.gameObject.SetActive(false);
        panel.SetActive(false);
    }
}