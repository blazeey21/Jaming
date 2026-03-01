using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public GameObject pausePanel;
    public Button continueButton;
    public Button optionsButton;
    public Button quitButton;

    public InputActionReference pauseAction;
    public InputActionReference navigateUpAction;
    public InputActionReference navigateDownAction;

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
    }

    void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= TogglePause;
            pauseAction.action.Disable();
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
        SetCursorState(isPaused);

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

    private void SetCursorState(bool paused)
    {
        if (paused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
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

        if (selectedIndex == 0) // Continue
            selectedIndex = 2; // Va a Exit
        else if (selectedIndex == 1) // Controls
            selectedIndex = 0; // Va a Continue
        else if (selectedIndex == 2) // Exit
            selectedIndex = 1; // Va a Controls

        SelectCurrentButton();
    }

    private void OnNavigateDown(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;

        if (selectedIndex == 0) // Continue
            selectedIndex = 1; // Va a Controls
        else if (selectedIndex == 1) // Controls
            selectedIndex = 2; // Va a Exit
        else if (selectedIndex == 2) // Exit
            selectedIndex = 0; // Va a Continue

        SelectCurrentButton();
    }

    private void SelectCurrentButton()
    {
        if (buttons != null && buttons.Length > 0 && selectedIndex >= 0 && selectedIndex < buttons.Length)
        {
            EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
        }
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