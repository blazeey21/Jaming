using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class NewspaperTrigger : MonoBehaviour
{
    [SerializeField] private GameObject canvasToActivate; 
    [SerializeField] private InputActionReference interactAction; 
    [SerializeField] public  CrumpsLogic Health; 
    [SerializeField] private float delayAfterDeath = 2f;
   

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
        if (!canInteract && Health.health<= 0)
        {
            StartCoroutine(EnableInteractionAfterDelay());
        }
    }

    private IEnumerator EnableInteractionAfterDelay()
    {
        yield return new WaitForSeconds(delayAfterDeath);
        canInteract = true; 
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