using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] string lobbyScene = "Lobby";

    public void OnPlayClicked()
    {
        SceneManager.LoadScene(lobbyScene);
    }

    public void OnExitClicked()
    {
        Debug.Log("Quitting game..."); // won't quit in editor, only in build
        Application.Quit();
    }
}