using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyBackButton : MonoBehaviour
{
    [Header("Settings")]
    public string mainMenuScene = "MainMenu";
    public float  holdDuration  = 0.5f;

    [Header("Hold Progress UI (optional)")]
    public Image holdProgressBar;

    private float holdTimer = 0f;

    void Start()
    {
        if (holdProgressBar != null)
            holdProgressBar.fillAmount = 0f;
    }

    void Update()
    {
        bool eastHeld = false;

        // Direct gamepad check — works before PlayerInput joins
        foreach (var gp in Gamepad.all)
        {
            if (gp.buttonEast.isPressed)
            {
                eastHeld = true;
                break;
            }
        }

        // Keyboard fallback
        if (Keyboard.current != null && Keyboard.current.escapeKey.isPressed)
            eastHeld = true;

        if (eastHeld)
        {
            holdTimer += Time.deltaTime;
            if (holdProgressBar != null)
                holdProgressBar.fillAmount = holdTimer / holdDuration;
            if (holdTimer >= holdDuration)
                GoBack();
        }
        else
        {
            holdTimer = 0f;
            if (holdProgressBar != null)
                holdProgressBar.fillAmount = 0f;
        }
    }

    void GoBack()
    {
        GameData.players.Clear();
        foreach (var pi in PlayerInput.all)
            Destroy(pi.gameObject);
        SceneManager.LoadScene(mainMenuScene);
    }
}