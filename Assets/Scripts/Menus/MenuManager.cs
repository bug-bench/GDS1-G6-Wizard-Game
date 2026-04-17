using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] string scene;

    public void OnPlayClicked()
    {
        GameData.players = new List<PlayerData>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Destroy(player);
        }
        SceneManager.LoadScene(scene);
    }

    public void OnExitClicked()
    {
        Debug.Log("Quitting game..."); // won't quit in editor, only in build
        Application.Quit();
    }
}