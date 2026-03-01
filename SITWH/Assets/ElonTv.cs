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
    public bool showSubtitle;
}

public class ElonTv : MonoBehaviour
{
    public static ElonTv Instance;

    public Transform audioFollowTarget;
    public float volume = 1.5f;

    [Header("Behaviour")]
    public bool playStartSequence = true;
    public bool subtitlesEnabled = true;
    public float globalCooldown = 0.1f;

    public ElonLine[] lines;

    [Header("Start sequence")]
    public string startA;
    public string startB;
    public string startC;
    public string clue1;
    public string clue2;

    Dictionary<string, ElonLine> lookup;
    float lastPlayTime = -999f;
    string lastPlayedId = "";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        lookup = new Dictionary<string, ElonLine>();
        foreach (var l in lines)
            if (!lookup.ContainsKey(l.id))
                lookup.Add(l.id, l);

        if (audioFollowTarget == null && Camera.main != null)
            audioFollowTarget = Camera.main.transform;
    }

    void Start()
    {
        if (playStartSequence)
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

        if (Time.time - lastPlayTime < globalCooldown) yield break;
        if (id == lastPlayedId) yield break;

        lastPlayTime = Time.time;
        lastPlayedId = id;

        var line = lookup[id];

        var instance = RuntimeManager.CreateInstance(line.dialogueEvent);

        if (audioFollowTarget != null)
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(audioFollowTarget));

        instance.setVolume(volume);

        instance.getDescription(out EventDescription desc);
        desc.getLength(out int lengthMs);

        instance.start();
        instance.release();

        if (subtitlesEnabled && line.showSubtitle && SubtitleManager.Instance != null && !string.IsNullOrEmpty(line.subtitle))
            SubtitleManager.Instance.Show(line.subtitle, lengthMs / 1000f);

        yield return new WaitForSecondsRealtime(lengthMs / 1000f);
    }

    public IEnumerator PlayAndReturnRoutine(string id)
    {
        if (!lookup.ContainsKey(id)) yield break;

        if (Time.time - lastPlayTime < globalCooldown) yield break;
        if (id == lastPlayedId) yield break;

        lastPlayTime = Time.time;
        lastPlayedId = id;

        var line = lookup[id];

        var instance = RuntimeManager.CreateInstance(line.dialogueEvent);

        if (audioFollowTarget != null)
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(audioFollowTarget));

        instance.setVolume(volume);

        instance.getDescription(out EventDescription desc);
        desc.getLength(out int lengthMs);

        instance.start();
        instance.release();

        if (subtitlesEnabled && line.showSubtitle && SubtitleManager.Instance != null && !string.IsNullOrEmpty(line.subtitle))
            SubtitleManager.Instance.Show(line.subtitle, lengthMs / 1000f);

        yield return new WaitForSecondsRealtime(lengthMs / 1000f);
    }
}