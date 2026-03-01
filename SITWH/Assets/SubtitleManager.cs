using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance;

    public GameObject panel;
    public TextMeshProUGUI subtitleText;

    Queue<SubtitleData> queue = new Queue<SubtitleData>();
    Coroutine playRoutine;
    bool isPlaying = false;

    struct SubtitleData
    {
        public string text;
        public float duration;

        public SubtitleData(string t, float d)
        {
            text = t;
            duration = d;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Show(string text, float duration)
    {
        if (panel == null || subtitleText == null) return;

        queue.Enqueue(new SubtitleData(text, duration));

        if (!isPlaying)
            playRoutine = StartCoroutine(ProcessQueue());
    }

    IEnumerator ProcessQueue()
    {
        isPlaying = true;

        panel.SetActive(true);

        while (queue.Count > 0)
        {
            var sub = queue.Dequeue();

            subtitleText.text = sub.text;
            subtitleText.gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(sub.duration);
        }

        subtitleText.gameObject.SetActive(false);
        panel.SetActive(false);

        isPlaying = false;
    }

    public void Clear()
    {
        queue.Clear();

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        if (subtitleText != null)
            subtitleText.gameObject.SetActive(false);

        if (panel != null)
            panel.SetActive(false);

        isPlaying = false;
    }
}