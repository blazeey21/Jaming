using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InterruptorZonas : MonoBehaviour
{
    public Zona[] zonas = new Zona[4];
    public bool activarConClick = true;
    public bool activarConTrigger = true;
    public InputActionReference inputAction;
    public UnityEvent alCambiarZona;

    [System.Serializable]
    public class Zona
    {
        public GameObject[] activar;
        public GameObject[] desactivar;
    }

    private int zonaActual = -1;
    private int zonaAnterior = -1;
    private bool jugadorDentro = false;

    void OnEnable()
    {
        if (inputAction != null)
            inputAction.action.performed += OnInputAction;
    }

    void OnDisable()
    {
        if (inputAction != null)
            inputAction.action.performed -= OnInputAction;
    }

    void OnInputAction(InputAction.CallbackContext ctx)
    {
        if (activarConTrigger && !jugadorDentro) return;
        Interact();
    }

    public void Interact()
    {
        zonaAnterior = zonaActual;
        zonaActual = (zonaActual + 1) % 4;

        if (zonaAnterior >= 0)
        {
            Aplicar(zonas[zonaAnterior].activar, false);
            Aplicar(zonas[zonaAnterior].desactivar, true);
        }

        Aplicar(zonas[zonaActual].activar, true);
        Aplicar(zonas[zonaActual].desactivar, false);

        alCambiarZona.Invoke();
    }

    void Aplicar(GameObject[] objs, bool estado)
    {
        if (objs == null) return;
        foreach (var o in objs) if (o) o.SetActive(estado);
    }

    void OnMouseDown()
    {
        if (activarConClick) Interact();
    }

    void OnTriggerEnter(Collider other)
    {
        if (activarConTrigger && other.CompareTag("Player"))
            jugadorDentro = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (activarConTrigger && other.CompareTag("Player"))
            jugadorDentro = false;
    }
}