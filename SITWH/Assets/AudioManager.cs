using UnityEngine;
using FMODUnity;
using FMOD.Studio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public EventInstance PlayEvent(EventReference eventRef)
    {
        EventInstance instance = RuntimeManager.CreateInstance(eventRef);
        instance.start();
        instance.release();
        return instance;
    }

    public void PlayDialogue(EventReference dialogueEvent, string subtitle, float duration)
    {
        PlayEvent(dialogueEvent);
        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.Show(subtitle, duration);
    }

    public void PlayDialogueAutoDuration(EventReference dialogueEvent, string subtitle)
    {
        EventInstance instance = RuntimeManager.CreateInstance(dialogueEvent);

        EventDescription desc;
        instance.getDescription(out desc);

        int lengthMs = 0;
        desc.getLength(out lengthMs);

        instance.start();
        instance.release();

        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.Show(subtitle, lengthMs / 1000f);
    }

    public void StopEvent(EventInstance instance, FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        instance.stop(mode);
        instance.release();
    }
}