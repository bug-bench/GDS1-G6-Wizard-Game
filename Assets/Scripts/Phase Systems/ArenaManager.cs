using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ArenaManager : MonoBehaviour
{
    [SerializeField] private string winScene = "WinScene";

    private List<PlayerCombat> players = new List<PlayerCombat>();

    private void Start()
    {
        // This finds all the players after they spawn
        Invoke(nameof(FindPlayers), 0.1f);
    }

    private void FindPlayers()
    {
        var found = FindObjectsByType<PlayerCombat>(FindObjectsSortMode.None);
        foreach (var p in found)
            players.Add(p);

        Debug.Log($"ArenaManager tracking {players.Count} players");
    }

    private void Update()
    {
        CheckForWinner();
    }

    private void CheckForWinner()
    {
        if (players.Count == 0) return;

        List<PlayerCombat> alive = players.FindAll(p => p != null && p.gameObject.activeSelf);

        if (alive.Count == 1)
        {
            var winner = alive[0];
            var playerInput = winner.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            GameData.winnerIndex = playerInput != null ? playerInput.playerIndex : 0;

            Debug.Log($"Player {GameData.winnerIndex + 1} wins!");
            SceneManager.LoadScene(winScene);
        }
        // else if (alive.Count == 0)
        // {
        //     // In case everyone dies at the same time, it's a draw
        //     GameData.winnerIndex = -1;
        //     SceneManager.LoadScene(winScene);
        // }
    }
    private void ShowWinner()
    {
        Debug.Log($"winnerIndex: {GameData.winnerIndex}, players in GameData: {GameData.players.Count}");
        
        var data = GameData.players.Find(p => p.playerIndex == GameData.winnerIndex);
        Debug.Log($"winner data found: {data != null}, kills: {data?.kills}, damage: {data?.damageDealt}");
    }
}