using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Pause : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [Header("Input Actions (asignar desde editor)")]
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private InputActionReference navigateUpAction;
    [SerializeField] private InputActionReference navigateDownAction;
    [SerializeField] private InputActionReference validateAction;

    private bool isPaused = false;
    private int selectedIndex = 0;
    private Button[] buttons;

    void Awake()
    {
        buttons = new Button[] { continueButton, optionsButton, quitButton };
    }

    void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += TogglePause;
        }

        if (validateAction != null)
        {
            validateAction.action.Enable();
            validateAction.action.performed += OnValidate;
        }
    }

    void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= TogglePause;
            pauseAction.action.Disable();
        }

        if (validateAction != null)
        {
            validateAction.action.performed -= OnValidate;
            validateAction.action.Disable();
        }

        UnsubscribeNavigationActions();
    }

    private void TogglePause(InputAction.CallbackContext ctx)
    {
        isPaused = !isPaused;
        ApplyPauseState();
    }

    public void TogglePauseButton()
    {
        isPaused = !isPaused;
        ApplyPauseState();
    }

    private void ApplyPauseState()
    {
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;

        if (isPaused)
        {
            selectedIndex = 0;
            SelectCurrentButton();
            SubscribeNavigationActions();
        }
        else
        {
            UnsubscribeNavigationActions();
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void SubscribeNavigationActions()
    {
        if (navigateUpAction != null)
        {
            navigateUpAction.action.Enable();
            navigateUpAction.action.performed += OnNavigateUp;
        }

        if (navigateDownAction != null)
        {
            navigateDownAction.action.Enable();
            navigateDownAction.action.performed += OnNavigateDown;
        }
    }

    private void UnsubscribeNavigationActions()
    {
        if (navigateUpAction != null)
        {
            navigateUpAction.action.performed -= OnNavigateUp;
            navigateUpAction.action.Disable();
        }

        if (navigateDownAction != null)
        {
            navigateDownAction.action.performed -= OnNavigateDown;
            navigateDownAction.action.Disable();
        }
    }

    private void OnNavigateUp(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;
        selectedIndex = (selectedIndex + buttons.Length - 1) % buttons.Length;
        SelectCurrentButton();
    }

    private void OnNavigateDown(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;
        selectedIndex = (selectedIndex + 1) % buttons.Length;
        SelectCurrentButton();
    }

    private void SelectCurrentButton()
    {
        if (buttons != null && buttons.Length > 0 && selectedIndex >= 0 && selectedIndex < buttons.Length)
            EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
    }

    private void OnValidate(InputAction.CallbackContext ctx)
    {
        if (!isPaused || buttons == null || buttons.Length == 0) return;
        buttons[selectedIndex].onClick.Invoke();
    }

    public void ContinueGame()
    {
        isPaused = false;
        ApplyPauseState();
    }

    public void OpenOptions()
    {
        Debug.Log("Abrir panel de opciones");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}