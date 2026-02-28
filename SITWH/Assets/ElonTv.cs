using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ElonLine
{
    public string id;
    public EventReference dialogueEvent;
    public string subtitle;
}

public class ElonTv : MonoBehaviour
{
    public static ElonTv Instance;

    [Header("Camera audio follow")]
    public Transform audioFollowTarget;

    [Header("Audio")]
    public float volume = 1.5f;

    [Header("Dialogues")]
    public ElonLine[] lines;

    Dictionary<string, ElonLine> lookup;

    void Awake()
    {
        Instance = this;

        lookup = new Dictionary<string, ElonLine>();
        foreach (var l in lines)
            lookup[l.id] = l;

        if (audioFollowTarget == null && Camera.main != null)
            audioFollowTarget = Camera.main.transform;
    }

    public void Play(string id)
    {
        if (!lookup.ContainsKey(id)) return;
        StartCoroutine(PlayRoutine(lookup[id]));
    }

    IEnumerator PlayRoutine(ElonLine line)
    {
        var instance = RuntimeManager.CreateInstance(line.dialogueEvent);

        if (audioFollowTarget != null)
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(audioFollowTarget));

        instance.setVolume(volume);

        EventDescription desc;
        instance.getDescription(out desc);

        int lengthMs = 0;
        desc.getLength(out lengthMs);

        instance.start();
        instance.release();

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.Show(line.subtitle, lengthMs / 1000f);

        yield return new WaitForSeconds(lengthMs / 1000f);
    }
}