using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InterruptorZonas : MonoBehaviour
{
    [Header("Zonas")]
    public Zona zona1;
    public Zona zona2;
    public Zona zona3;
    public Zona zona4;

    [Header("Activación")]
    public bool activarConClick = true;
    public bool activarConProximidad = true;
    public float rangoInteraccion = 3f;
    public InputActionReference inputAction;

    public UnityEvent alCambiarZona;

    [System.Serializable]
    public class Zona
    {
        public GameObject[] activar;
        public GameObject[] desactivar;
    }

    private int zonaActual = 0;
    private int zonaAnterior = 0;
    private Transform jugador;
    private bool puedeActivar = true; // Para evitar doble activación

    void OnEnable()
    {
        if (inputAction != null)
        {
            inputAction.action.Enable();
            inputAction.action.performed += OnInputAction;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) jugador = player.transform;
    }

    void OnDisable()
    {
        if (inputAction != null)
            inputAction.action.performed -= OnInputAction;
    }

    void OnInputAction(InputAction.CallbackContext ctx)
    {
        // Ignorar si el dispositivo es ratón o teclado
        if (ctx.control.device is Mouse || ctx.control.device is Keyboard)
            return;

        if (!activarConProximidad) return;
        if (jugador == null) return;

        float dist = Vector3.Distance(transform.position, jugador.position);
        if (dist <= rangoInteraccion)
            Interact();
    }

    public void Interact()
    {
        if (!puedeActivar) return;
        puedeActivar = false;

        zonaAnterior = zonaActual;
        zonaActual = zonaActual % 4 + 1;

        Debug.Log($"🔁 Cambio de zona: {zonaAnterior} → {zonaActual}");

        if (zonaAnterior >= 1 && zonaAnterior <= 4)
        {
            Zona ant = ObtenerZona(zonaAnterior);
            AplicarEstado(ant.activar, false);
            AplicarEstado(ant.desactivar, true);
        }

        Zona nueva = ObtenerZona(zonaActual);
        if (nueva != null)
        {
            AplicarEstado(nueva.activar, true);
            AplicarEstado(nueva.desactivar, false);
        }

        alCambiarZona.Invoke();

        Invoke(nameof(Reactivar), 0.1f); // Pequeña pausa para evitar doble input
    }

    void Reactivar()
    {
        puedeActivar = true;
    }

    private Zona ObtenerZona(int num)
    {
        switch (num)
        {
            case 1: return zona1;
            case 2: return zona2;
            case 3: return zona3;
            case 4: return zona4;
            default: return null;
        }
    }

    private void AplicarEstado(GameObject[] objs, bool estado)
    {
        if (objs == null) return;
        foreach (GameObject o in objs)
            if (o != null) o.SetActive(estado);
    }

    void OnMouseDown()
    {
        if (activarConClick)
            Interact();
    }
}