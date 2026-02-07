using UnityEngine;

public class SingleTrigger : MonoBehaviour
{
    public string requiredTag;
    public string targetLayer = "Grabbable";

    [SerializeField] private bool isActive = false;
    private int layerValue;
    private Collider myCollider;

    void Start()
    {
        layerValue = LayerMask.NameToLayer(targetLayer);
        myCollider = GetComponent<Collider>();

        if (myCollider == null)
        {
            Debug.LogError($"❌ {gameObject.name} no tiene Collider");
            return;
        }

        myCollider.isTrigger = true;
        isActive = false; // Asegurar que empiece en false

        Debug.Log($"✅ Trigger '{gameObject.name}' listo (Tag requerido: '{requiredTag}', Layer: {targetLayer})");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != layerValue)
        {
            Debug.Log($"{gameObject.name}: Ignorado {other.name} - Layer incorrecta");
            return;
        }

        Debug.Log($"🎯 {gameObject.name} activado por: {other.name} (Tag: {other.tag})");

        if (other.CompareTag(requiredTag))
        {
            isActive = true;
            Debug.Log($"✅ {gameObject.name}: TAG CORRECTO '{requiredTag}'!");
            Destroy(other.gameObject);
        }
        else
        {
            Debug.Log($"❌ {gameObject.name}: Tag incorrecto. Esperaba '{requiredTag}', tiene '{other.tag}'");
            Destroy(other.gameObject);
        }
    }

    public bool IsActive()
    {
        return isActive;
    }

    public void ResetTrigger()
    {
        isActive = false;
        Debug.Log($"🔄 {gameObject.name} resetead");
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