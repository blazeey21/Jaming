using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioQueueManager : MonoBehaviour
{
    public static AudioQueueManager Instance;

    private bool isVoicePlaying = false;
    private Queue<System.Func<IEnumerator>> voiceQueue = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EnqueueVoice(System.Func<IEnumerator> voice)
    {
        voiceQueue.Enqueue(voice);
        if (!isVoicePlaying)
            StartCoroutine(ProcessVoiceQueue());
    }

    public void PlaySFX(System.Func<IEnumerator> sfx)
    {
        StartCoroutine(sfx());
    }

    private IEnumerator ProcessVoiceQueue()
    {
        isVoicePlaying = true;

        while (voiceQueue.Count > 0)
            yield return StartCoroutine(voiceQueue.Dequeue()());

        isVoicePlaying = false;
    }
}