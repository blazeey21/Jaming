using UnityEngine;

public class TriggerPsico : MonoBehaviour
{
    public Renderer[] objetosACambiar;
    public Material nuevoMaterial;
    public string tagJugador = "Player";

    private Material[] materialesOriginales;
    private bool materialesGuardados = false;

    void Start()
    {
        if (objetosACambiar != null && objetosACambiar.Length > 0)
        {
            materialesOriginales = new Material[objetosACambiar.Length];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagJugador))
        {
            CambiarMateriales();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagJugador))
        {
        }
    }

    private void CambiarMateriales()
    {
        if (objetosACambiar == null || objetosACambiar.Length == 0)
        {
            return;
        }

        if (nuevoMaterial == null)
        {
            return;
        }

        if (!materialesGuardados)
        {
            GuardarMaterialesOriginales();
        }

        foreach (Renderer renderer in objetosACambiar)
        {
            if (renderer != null)
            {
                renderer.material = nuevoMaterial;
            }
        }
    }

    private void GuardarMaterialesOriginales()
    {
        for (int i = 0; i < objetosACambiar.Length; i++)
        {
            if (objetosACambiar[i] != null)
            {
                materialesOriginales[i] = objetosACambiar[i].material;
            }
        }
        materialesGuardados = true;
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(tagJugador))
        {
            tagJugador = "Player";
        }
    }
}