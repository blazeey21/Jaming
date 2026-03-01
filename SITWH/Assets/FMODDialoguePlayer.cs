using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODDialoguePlayer : MonoBehaviour
{
    public void PlayDialogue(EventReference dialogueEvent, string subtitle, float duration)
    {
        EventInstance instance = RuntimeManager.CreateInstance(dialogueEvent);
        instance.start();
        instance.release();

        if (SubtitleManager.Instance != null && !string.IsNullOrEmpty(subtitle))
            SubtitleManager.Instance.Show(subtitle, duration);
    }
}