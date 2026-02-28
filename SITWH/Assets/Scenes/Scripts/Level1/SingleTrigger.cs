using UnityEngine;
using FMODUnity;

public class SingleTrigger : MonoBehaviour
{
    public string requiredTag;
    public string targetLayer = "Grabbable";

    [Header("Sonido FMOD")]
    public EventReference sonidoCorrecto;

    [SerializeField] private bool isActive = false;
    private int layerValue;
    private Collider myCollider;
    [SerializeField] public GameObject myCollider1;
    [SerializeField] public GameObject myCollider2;

    void Start()
    {
        layerValue = LayerMask.NameToLayer(targetLayer);
        myCollider = GetComponent<Collider>();

        myCollider.isTrigger = true;
        isActive = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != layerValue)
        {
            Debug.Log($"{gameObject.name}: Ignorado {other.name} - Layer incorrecta");
            return;
        }

        if (other.CompareTag(requiredTag))
        {
            isActive = true;
            Destroy(other.gameObject);

            myCollider1.SetActive(false);
            myCollider2.SetActive(true);

            ReproducirSonidoCorrecto();
        }
        else
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                float multiplicador = 6f;
                rb.linearVelocity *= multiplicador;
            }
        }
    }

    private void ReproducirSonidoCorrecto()
    {
        if (!sonidoCorrecto.IsNull)
        {
            var instancia = RuntimeManager.CreateInstance(sonidoCorrecto);
            instancia.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            instancia.start();
            instancia.release();
        }
    }

    public bool IsActive()
    {
        return isActive;
    }

    public void ResetTrigger()
    {
        isActive = false;
    }

    void OnDrawGizmos()
    {
        if (myCollider != null && myCollider.enabled)
        {
            Gizmos.color = isActive ? Color.green : Color.red;
            Gizmos.DrawWireCube(myCollider.bounds.center, myCollider.bounds.size);
        }
    }
}