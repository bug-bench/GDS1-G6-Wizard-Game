using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectScript : MonoBehaviour
{
    [SerializeField] private string winScene = "WinScene";

    private List<GameObject> players = new List<GameObject>();
    protected CollectManager cm;
    private GameObject winner = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var player in GameData.players)
        {
            players.Add(player.playerGameObject);
        }
        StartCoroutine(GameTimer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator GameTimer()
    {
        //setup time
        yield return new WaitForSeconds(5f);
        //Game time
        yield return new WaitForSeconds(150f);
        winner = cm.RegisterGameEnd();
        //Counting time
        yield return new WaitForSeconds(2f);
        EndGame(winner);
    }

    private void EndGame(GameObject winner)
    {
        var playerInput = winner.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        GameData.winnerIndex = playerInput != null ? playerInput.playerIndex : 0;

        // ADD THIS
        foreach (var p in GameData.players)
            Debug.Log($"EndGame — Player {p.playerIndex} kills: {p.kills}, damage: {p.damageDealt}");

        SceneManager.LoadScene(winScene);
    }
}
