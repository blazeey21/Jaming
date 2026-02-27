using UnityEngine;

public class SupportCrumps : MonoBehaviour
{
    public CrumpsLogic crumpsLogic;
    public LayerMask grabbableLayer;

    void Start()
    {
        if (crumpsLogic == null)
        {
            GameObject crumps = GameObject.FindGameObjectWithTag("Crumps");
            if (crumps != null)
                crumpsLogic = crumps.GetComponent<CrumpsLogic>();
        }

        if (grabbableLayer.value == 0 && crumpsLogic != null)
            grabbableLayer = crumpsLogic.grabbableLayer;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsInLayerMask(other.gameObject, grabbableLayer))
            return;

        if (other.CompareTag("ObjectosCrumpsGood"))
        {
            crumpsLogic?.OnGoodObjectDestroyed(other.transform.position);
        }
        else if (other.CompareTag("ObjectosCrumpsBad"))
        {
            crumpsLogic?.OnBadObjectDestroyed(other.transform.position);
        }
        else
        {
            return;
        }

        DisableAllColliders(other.gameObject);

        
        // Destruir el objeto
        Destroy(other.gameObject);
    }


    private void DisableAllColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }
}