using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] string scene;

    public void OnPlayClicked()
    {
        SceneManager.LoadScene(scene);
    }

    public void OnExitClicked()
    {
        Debug.Log("Quitting game..."); // won't quit in editor, only in build
        Application.Quit();
    }
}