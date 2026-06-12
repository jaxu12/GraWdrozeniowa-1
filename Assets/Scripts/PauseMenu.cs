using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "Main";

    private bool isPaused = false;

    private void Awake()
    {
        AutoAssignButtons();
        BindButtons();
    }

    private void Start()
    {
        SetPaused(false);
    }

    private void OnDestroy()
    {
        UnbindButtons();
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void Pause()
    {
        SetPaused(true);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
            return;
        }

        Pause();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(paused);
            return;
        }

        Debug.LogError("PauseMenu: pauseMenuUI is not assigned.", this);
    }

    private void AutoAssignButtons()
    {
        if (pauseButton == null)
        {
            pauseButton = FindButtonByName("Pause");
        }

        if (resumeButton == null)
        {
            resumeButton = FindButtonByName("Resume");
        }

        if (restartButton == null)
        {
            restartButton = FindButtonByName("Restart");
        }

        if (mainMenuButton == null)
        {
            mainMenuButton = FindButtonByName("MainMenu");
        }
    }

    private Button FindButtonByName(string buttonName)
    {
        Scene activeScene = SceneManager.GetActiveScene();

        foreach (GameObject root in activeScene.GetRootGameObjects())
        {
            Button[] buttons = root.GetComponentsInChildren<Button>(true);

            foreach (Button button in buttons)
            {
                if (button.name == buttonName)
                {
                    return button;
                }
            }
        }

        return null;
    }

    private void BindButtons()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePause);
            pauseButton.onClick.AddListener(TogglePause);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(Resume);
            resumeButton.onClick.AddListener(Resume);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartLevel);
            restartButton.onClick.AddListener(RestartLevel);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(LoadMainMenu);
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }
    }

    private void UnbindButtons()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePause);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(Resume);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartLevel);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(LoadMainMenu);
        }
    }
}
