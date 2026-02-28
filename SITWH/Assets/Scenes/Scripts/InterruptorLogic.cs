using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using FMODUnity;

public class InterruptorZonas : MonoBehaviour
{
    [Header("Zonas")]
    public Zona zona1;
    public Zona zona2;
    public Zona zona3;
    public Zona zona4;
    public Zona zona5;

    [Header("Activación")]
    public bool activarConClick = true;
    public bool activarConProximidad = true;
    public float rangoInteraccion = 3f;
    public InputActionReference inputAction;

    [Header("Sonidos")]
    public EventReference sonidoEspecial; // Sonido cuando todas las zonas menos la 5 están activas

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
    private bool puedeActivar = true;

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
        if (ctx.control.device is Mouse || ctx.control.device is Keyboard) return;
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

        const int totalZonas = 5;

        zonaAnterior = zonaActual;
        zonaActual = zonaActual % totalZonas + 1;

        if (zonaAnterior >= 1 && zonaAnterior <= 5)
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

        // 🔊 Verificar si todas las zonas menos la 5 están activas
        if (TodasZonasMenosCincoActivas())
            ReproducirSonidoEspecial();

        Invoke(nameof(Reactivar), 0.1f);
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
            case 5: return zona5;
            default: return null;
        }
    }

    private void AplicarEstado(GameObject[] objs, bool estado)
    {
        if (objs == null) return;
        foreach (GameObject o in objs)
            if (o != null) o.SetActive(estado);
    }

    private bool TodasZonasMenosCincoActivas()
    {
        bool z1 = EstaZonaActiva(zona1);
        bool z2 = EstaZonaActiva(zona2);
        bool z3 = EstaZonaActiva(zona3);
        bool z4 = EstaZonaActiva(zona4);
        bool z5 = EstaZonaActiva(zona5);

        return z1 && z2 && z3 && z4 && !z5;
    }

    private bool EstaZonaActiva(Zona z)
    {
        if (z == null || z.activar == null || z.activar.Length == 0) return false;
        foreach (var go in z.activar)
            if (go != null && !go.activeInHierarchy) return false;
        return true;
    }

    private void ReproducirSonidoEspecial()
    {
        if (!sonidoEspecial.IsNull)
        {
            var instancia = RuntimeManager.CreateInstance(sonidoEspecial);
            instancia.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            instancia.start();
            instancia.release();
            Debug.Log("Sonido especial reproducido porque todas las zonas menos la 5 están activas");
        }
    }

    void OnMouseDown()
    {
        if (activarConClick)
            Interact();
    }
}