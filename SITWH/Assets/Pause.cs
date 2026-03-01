using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Pause : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private InputActionReference navigateUpAction;
    [SerializeField] private InputActionReference navigateDownAction;
    [SerializeField] private InputActionReference validateAction;

    private bool isPaused = false;
    private int selectedIndex = 0;
    private Button[] buttons;

    void Awake()
    {
        buttons = new Button[]
        {
            continueButton, // 0 arriba
            quitButton      // 1 abajo
        };
    }

    void Update()
    {
        if (!isPaused) return;
        SyncIndexWithEventSystem();
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
            validateAction.action.performed += OnValidate1;
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
            validateAction.action.performed -= OnValidate1;
            validateAction.action.Disable();
        }

        UnsubscribeNavigationActions();
    }

    private void TogglePause(InputAction.CallbackContext ctx)
    {
        isPaused = !isPaused;
        ApplyPauseState();
    }

    private void ApplyPauseState()
    {
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        AudioListener.pause = isPaused;

        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;

        if (isPaused)
        {
            selectedIndex = 0;
            ForceSelect();
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

    private void SyncIndexWithEventSystem()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        if (current == null) return;

        for (int i = 0; i < buttons.Length; i++)
            if (buttons[i].gameObject == current)
                selectedIndex = i;
    }

    private void ForceSelect()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
    }

    private void OnNavigateUp(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;

        if (selectedIndex > 0)
        {
            selectedIndex--;
            ForceSelect();
        }
    }

    private void OnNavigateDown(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;

        if (selectedIndex < buttons.Length - 1)
        {
            selectedIndex++;
            ForceSelect();
        }
    }

    private void OnValidate1(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;
        buttons[selectedIndex].onClick.Invoke();
    }

    public void ContinueGame()
    {
        isPaused = false;
        ApplyPauseState();
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