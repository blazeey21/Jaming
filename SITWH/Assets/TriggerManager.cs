using UnityEngine;

public class TriggerManager : MonoBehaviour
{
    private SingleTrigger[] childTriggers;
    [SerializeField] private bool allActive = false;

    void Start()
    {
        FindAllTriggers();
        Debug.Log($"✅ Manager activo con {childTriggers.Length} triggers hijos");
    }

    void FindAllTriggers()
    {
        childTriggers = GetComponentsInChildren<SingleTrigger>(true);

        if (childTriggers.Length == 0)
        {
            Debug.LogWarning("⚠️ No se encontraron SingleTrigger en los hijos");
        }
    }

    public bool AreAllTriggersActive()
    {
        if (childTriggers == null || childTriggers.Length == 0)
        {
            FindAllTriggers();
        }

        allActive = true;

        foreach (var trigger in childTriggers)
        {
            if (trigger == null) continue;

            if (!trigger.IsActive())
            {
                allActive = false;
                break;
            }
        }

        return allActive;
    }

    public void ResetAllTriggers()
    {
        foreach (var trigger in childTriggers)
        {
            if (trigger != null)
            {
                trigger.ResetTrigger();
            }
        }
        allActive = false;
        Debug.Log("🔄 Todos los triggers reseteados");
    }

    void Update()
    {
        // Solo para debugging - verificar estado cada frame
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"=== ESTADO TRIGGERS ===");
            for (int i = 0; i < childTriggers.Length; i++)
            {
                if (childTriggers[i] != null)
                {
                    Debug.Log($"Trigger {i}: {childTriggers[i].gameObject.name} - Activo: {childTriggers[i].IsActive()}");
                }
            }
            Debug.Log($"Todos activos: {AreAllTriggersActive()}");
        }
    }
}