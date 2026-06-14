using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [SerializeField] private string mainMenuSceneName = "Main";

    public bool IsPaused => isPaused;

    private bool isPaused;
    private bool isGameOver;
    private PlayerHudController hud;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        hud = FindAnyObjectByType<PlayerHudController>();
    }

    private void Start()
    {
        SetPaused(false);
    }

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        Time.timeScale = 1f;
    }

    public void Pause()
    {
        if (isGameOver)
        {
            return;
        }

        SetPaused(true);
    }

    public void Resume()
    {
        if (isGameOver)
        {
            return;
        }

        SetPaused(false);
    }

    public void TogglePause()
    {
        if (isGameOver)
        {
            return;
        }

        SetPaused(!isPaused);
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

    public void ShowGameOver()
    {
        isGameOver = true;
        SetPaused(false);
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (hud == null)
        {
            hud = FindAnyObjectByType<PlayerHudController>();
        }

        if (hud != null)
        {
            if (paused)
            {
                hud.ShowPauseMenu();
            }
            else
            {
                hud.HidePauseMenu();
            }
        }
    }
}
