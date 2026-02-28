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

    public Transform audioFollowTarget;
    public float volume = 1.5f;

    public ElonLine[] lines;

    [Header("Start sequence")]
    public string startA;
    public string startB;
    public string startC;
    public string clue1;
    public string clue2;

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

    void Start()
    {
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        yield return PlayAndWait(startA);
        yield return PlayAndWait(startB);
        yield return PlayAndWait(startC);
        yield return PlayAndWait(clue1);
        yield return PlayAndWait(clue2);
    }

    public void Play(string id)
    {
        StartCoroutine(PlayAndWait(id));
    }

    IEnumerator PlayAndWait(string id)
    {
        if (!lookup.ContainsKey(id)) yield break;

        var line = lookup[id];

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