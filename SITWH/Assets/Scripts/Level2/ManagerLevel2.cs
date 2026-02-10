using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ManagerLevel2 : MonoBehaviour
{
    public GameObject[] elementosUI;
    public GameObject Puertanivel2;
    public GameObject panelNumeros;
    public TMP_Text textPista;
    public TMP_Text textRespuesta;
    public TMP_Text[] textNumeros;
    public int[] numeroCorrecto = new int[4];
    public string[] respuestasNegativas;
    [SerializeField] private PlayerLogic player;


    [SerializeField] private InputActionProperty closeAction;
    [SerializeField] private InputActionProperty moveNumberAction;
    [SerializeField] private InputActionProperty incrementAction;
    [SerializeField] private InputActionProperty decrementAction;
    [SerializeField] private InputActionProperty validarAction;

    private bool isPlayerInTrigger = false;
    private int[] numeroActual = new int[4];
    private int indiceSeleccionado = 0;
    private bool panelActivo = false;
    private bool validado = false;

    private void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            numeroActual[i] = 0;
        }

        DesactivarElementosUI();

        if (panelNumeros != null) panelNumeros.SetActive(false);
        if (Puertanivel2 != null) Puertanivel2.SetActive(true);

        if (closeAction.action != null)
        {
            closeAction.action.Enable();
            closeAction.action.performed += OnClosePerformed;
        }

        if (moveNumberAction.action != null)
        {
            moveNumberAction.action.Enable();
            moveNumberAction.action.performed += OnMoveNumber;
        }

        if (incrementAction.action != null)
        {
            incrementAction.action.Enable();
            incrementAction.action.performed += OnIncrement;
        }

        if (decrementAction.action != null)
        {
            decrementAction.action.Enable();
            decrementAction.action.performed += OnDecrement;
        }

        if (validarAction.action != null)
        {
            validarAction.action.Enable();
            validarAction.action.performed += OnValidar;
        }
    }

    private void OnDestroy()
    {
        if (closeAction.action != null)
            closeAction.action.performed -= OnClosePerformed;

        if (moveNumberAction.action != null)
            moveNumberAction.action.performed -= OnMoveNumber;

        if (incrementAction.action != null)
            incrementAction.action.performed -= OnIncrement;

        if (decrementAction.action != null)
            decrementAction.action.performed -= OnDecrement;

        if (validarAction.action != null)
            validarAction.action.performed -= OnValidar;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            ActivarPanel();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            CerrarTodo();
        }
    }

    private void OnClosePerformed(InputAction.CallbackContext context)
    {
        if (isPlayerInTrigger && panelActivo)
        {
            CerrarTodo();
        }
    }

    public void CerrarTodo()
    {
        DesactivarElementosUI();
        DesactivarPanel();
    }

    public void ActivarPanel()
    {
        panelActivo = true;
        player.CanMove = false;
        Time.timeScale = 0f;


        foreach (GameObject elemento in elementosUI)
        {
            if (elemento != null)
                elemento.SetActive(true);
        }

        if (panelNumeros != null) panelNumeros.SetActive(true);
        indiceSeleccionado = 0;
        ActualizarSeleccionVisual();
    }

    public void DesactivarPanel()
    {
        player.CanMove = true;
        Time.timeScale = 1f;
        panelActivo = false;
        Time.timeScale = 1f;

        foreach (GameObject elemento in elementosUI)
        {
            if (elemento != null)
                elemento.SetActive(false);
        }

        if (panelNumeros != null) panelNumeros.SetActive(false);

    }
    


    private void DesactivarElementosUI()
    {
        foreach (GameObject elemento in elementosUI)
        {
            if (elemento != null)
            {
                elemento.SetActive(false);
            }
        }
    }

    private void OnMoveNumber(InputAction.CallbackContext context)
    {
        if (!panelActivo || validado) return;

        float movimiento = context.ReadValue<float>();

        if (movimiento > 0)
        {
            indiceSeleccionado = (indiceSeleccionado + 1) % 4;
        }
        else if (movimiento < 0)
        {
            indiceSeleccionado = (indiceSeleccionado - 1 + 4) % 4;
        }

        ActualizarSeleccionVisual();
    }

    private void OnIncrement(InputAction.CallbackContext context)
    {
        if (!panelActivo || validado) return;

        if (context.phase == InputActionPhase.Performed)
        {
            numeroActual[indiceSeleccionado] = (numeroActual[indiceSeleccionado] + 1) % 10;
            ActualizarNumeroUI(indiceSeleccionado);
        }
    }

    private void OnDecrement(InputAction.CallbackContext context)
    {
        if (!panelActivo || validado) return;

        if (context.phase == InputActionPhase.Performed)
        {
            numeroActual[indiceSeleccionado] = (numeroActual[indiceSeleccionado] - 1 + 10) % 10;
            ActualizarNumeroUI(indiceSeleccionado);
        }
    }

    private void OnValidar(InputAction.CallbackContext context)
    {
        if (!panelActivo || validado) return;

        if (context.phase == InputActionPhase.Performed)
        {
            ValidarNumero();
        }
    }

    private void ActualizarNumeroUI(int indice)
    {
        if (indice >= 0 && indice < textNumeros.Length && textNumeros[indice] != null)
        {
            textNumeros[indice].text = numeroActual[indice].ToString();
        }
    }

    private void ActualizarSeleccionVisual()
    {
        for (int i = 0; i < textNumeros.Length; i++)
        {
            if (textNumeros[i] != null)
            {
                if (i == indiceSeleccionado)
                {
                    textNumeros[i].color = Color.yellow;
                }
                else
                {
                    textNumeros[i].color = Color.white;
                }
            }
        }
    }

    private void ValidarNumero()
    {
        bool esCorrecto = true;

        for (int i = 0; i < 4; i++)
        {
            if (numeroActual[i] != numeroCorrecto[i])
            {
                esCorrecto = false;
                break;
            }
        }

        if (esCorrecto)
        {
            validado = true;
            if (textRespuesta != null) textRespuesta.text = "✓";

            if (Puertanivel2 != null)
            {
                Puertanivel2.SetActive(false);
            }

            foreach (TMP_Text num in textNumeros)
            {
                if (num != null) num.color = Color.green;
            }
            BoxCollider collider = GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.enabled = false;
            }


        }
        else
        {
            if (textRespuesta != null)
            {
                if (respuestasNegativas != null && respuestasNegativas.Length > 0)
                {
                    int randomIndex = Random.Range(0, respuestasNegativas.Length);
                    textRespuesta.text = respuestasNegativas[randomIndex];
                }
                else
                {
                    textRespuesta.text = "X";
                }
            }
        }
    }
   
    public void SetNumeroCorrecto(int[] nuevoCodigo)
    {
        if (nuevoCodigo.Length == 4)
        {
            numeroCorrecto = nuevoCodigo;
        }
    }

    public void SetPista(string pista)
    {
        if (textPista != null)
        {
            textPista.text = pista;
        }
    }

    public bool IsValidado()
    {
        return validado;
    }
}