using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class AudioObjects : MonoBehaviour
{
    [System.Serializable]
    public class AudioObject
    {
        public Transform objeto;
        public EventReference sonidoLoop;
        public float distanciaActivacion = 10f;
        public AnimationCurve volumenPorDistancia = AnimationCurve.Linear(0, 1, 1, 0);

        [HideInInspector] public EventInstance instancia;
        [HideInInspector] public bool sonando;
    }

    public Transform player;
    public List<AudioObject> audioObjects = new List<AudioObject>();

    void Update()
    {
        foreach (var a in audioObjects)
        {
            if (a.objeto == null) continue;

            float distancia = Vector3.Distance(player.position, a.objeto.position);
            float t = Mathf.Clamp01(distancia / a.distanciaActivacion);
            float volumen = a.volumenPorDistancia.Evaluate(t);

            if (distancia <= a.distanciaActivacion)
            {
                if (!a.sonando)
                {
                    a.instancia = RuntimeManager.CreateInstance(a.sonidoLoop);
                    a.instancia.set3DAttributes(RuntimeUtils.To3DAttributes(a.objeto));
                    a.instancia.start();
                    a.sonando = true;
                }

                a.instancia.set3DAttributes(RuntimeUtils.To3DAttributes(a.objeto));
                a.instancia.setVolume(volumen);
            }
            else
            {
                if (a.sonando)
                {
                    a.instancia.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    a.instancia.release();
                    a.sonando = false;
                }
            }
        }
    }

    void OnDestroy()
    {
        foreach (var a in audioObjects)
        {
            if (a.sonando)
            {
                a.instancia.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                a.instancia.release();
            }
        }
    }
}