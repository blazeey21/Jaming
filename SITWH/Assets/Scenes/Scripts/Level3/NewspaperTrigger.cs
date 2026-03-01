using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class NewspaperTrigger : MonoBehaviour
{
    [SerializeField] private GameObject canvasToActivate; // Canvas a activar
    [SerializeField] private InputActionReference interactAction; // Input Action tipo "Interact"
    [SerializeField] public  CrumpsLogic Health; // Referencia a tu script de vida
    [SerializeField] private float delayAfterDeath = 2f; // Segundos antes de permitir interacción

    public FMODUnity.EventReference ambienceEvent;
    private bool canInteract = false;

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }

    private void Update()
    {
        // Comprobamos si la vida llegó a 0 y aún no empezamos la espera
        if (!canInteract && Health.health<= 0)
        {
            StartCoroutine(EnableInteractionAfterDelay());
        }
    }

    private IEnumerator EnableInteractionAfterDelay()
    {
        yield return new WaitForSeconds(delayAfterDeath);
        canInteract = true; // Ya se puede interactuar
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (canInteract && canvasToActivate != null)
        {
            canvasToActivate.SetActive(true);
            FMODUnity.RuntimeManager.PlayOneShot(ambienceEvent);
        }
    }
}