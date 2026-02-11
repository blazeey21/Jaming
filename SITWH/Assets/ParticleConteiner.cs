using UnityEngine;

public class ParticleConteiner : MonoBehaviour
{
    [Header("Configuración del Campo")]
    [Tooltip("Collider que define el área donde las partículas deben permanecer.")]
    public Collider fieldBoundary;

    [Tooltip("(Opcional) Sistema de partículas que representa el campo visualmente. No se modificará.")]
    public ParticleSystem fieldParticleSystem;

    [Header("Opciones")]
    [Tooltip("Si está activo, configura automáticamente al inicio.")]
    public bool autoConfigureOnStart = true;

    void Start()
    {
        if (autoConfigureOnStart)
        {
            ConfigureContainment();
        }
    }

    /// <summary>
    /// Configura todos los ParticleSystem hijos (excepto fieldParticleSystem) para que usen fieldBoundary
    /// como volumen de contención. Las partículas que salgan del collider serán destruidas.
    /// </summary>
    public void ConfigureContainment()
    {
        if (fieldBoundary == null)
        {
            Debug.LogError("ParticleConteiner: No se ha asignado un collider de campo.", this);
            return;
        }

        // Buscar todos los ParticleSystem en los hijos (incluyendo el propio GameObject)
        ParticleSystem[] allParticleSystems = GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in allParticleSystems)
        {
            // Saltar el sistema de partículas que representa el campo, si se asignó
            if (fieldParticleSystem != null && ps == fieldParticleSystem)
                continue;

            // Activar el módulo Trigger
            ParticleSystem.TriggerModule trigger = ps.trigger;
            trigger.enabled = true;

            // Limpiar colisiones previas (opcional pero recomendado)
            trigger.SetCollider(0, null);

            // Asignar el collider del campo
            trigger.SetCollider(0, fieldBoundary);

            // Establecer la acción cuando la partícula está fuera del volumen
            trigger.inside = ParticleSystemOverlapAction.Ignore;
            trigger.outside = ParticleSystemOverlapAction.Kill;
            trigger.enter = ParticleSystemOverlapAction.Ignore;
            trigger.exit = ParticleSystemOverlapAction.Ignore;

       
        }

        Debug.Log($"ParticleConteiner: Configurados {allParticleSystems.Length - (fieldParticleSystem != null ? 1 : 0)} sistemas de partículas para contención.");
    }

    // Método útil para reconfigurar en caliente desde el Inspector o eventos
    public void UpdateBoundary(Collider newBoundary)
    {
        fieldBoundary = newBoundary;
        ConfigureContainment();
    }
}