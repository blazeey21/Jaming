using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioPrinc : MonoBehaviour
{
    public EventReference evento;
    public float minVolume = 0.2f;
    public float maxVolume = 1f;
    public float speed = 1f;

    EventInstance instance;
    float t;

    void Start()
    {
        instance = RuntimeManager.CreateInstance(evento);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        instance.start();
    }

    void Update()
    {
        t += Time.deltaTime * speed;
        float v = Mathf.Lerp(minVolume, maxVolume, (Mathf.Sin(t) + 1f) * 0.5f);
        instance.setVolume(v);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
    }

    void OnDestroy()
    {
        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();
    }
}