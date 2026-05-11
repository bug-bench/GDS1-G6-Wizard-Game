using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;

    [SerializeField] private string mainMenuScene = "MainMenu";

    public static bool IsPaused { get; private set; }
    public static float LastUnpauseTime { get; private set; }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Reset pause state every time a new scene loads
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsPaused)
        {
            IsPaused = false;
            Time.timeScale = 1f;
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        // Never pause on main menu or lobby
        if (scene.name == mainMenuScene || scene.name == "Lobby")
        {
            IsPaused = false;
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == mainMenuScene) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();

        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.startButton.wasPressedThisFrame)
            {
                TogglePause();
                break;
            }
        }
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        pausePanel.SetActive(IsPaused);
        Time.timeScale = IsPaused ? 0f : 1f;

        if (!IsPaused)
            LastUnpauseTime = Time.unscaledTime;

        if (IsPaused)
        {
            Button firstButton = pausePanel.GetComponentInChildren<Button>();
            if (firstButton != null)
                EventSystem.current?.SetSelectedGameObject(firstButton.gameObject);
        }
        else
        {
            EventSystem.current?.SetSelectedGameObject(null);
        }
    }

    public void OnResumePressed()
    {
        if (IsPaused) TogglePause();
    }

    public void OnMainMenuPressed()
    {
        IsPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        GameData.players.Clear();
        GameData.winnerIndex = -1;
        SceneManager.LoadScene(mainMenuScene);
    }
}