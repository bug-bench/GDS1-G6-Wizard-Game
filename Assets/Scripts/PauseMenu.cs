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
        Debug.Log($"TogglePause called, IsPaused becoming: {!IsPaused}\n{System.Environment.StackTrace}");
        IsPaused = !IsPaused;
        pausePanel.SetActive(IsPaused);
        Time.timeScale = IsPaused ? 0f : 1f;

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
        pausePanel.SetActive(false); // ADD THIS
        Time.timeScale = 1f;
        GameData.players.Clear();
        GameData.winnerIndex = -1;
        SceneManager.LoadScene(mainMenuScene);
    }
}