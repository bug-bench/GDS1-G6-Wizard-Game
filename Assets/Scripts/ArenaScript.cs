using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class ArenaScript : MonoBehaviour
{
    [SerializeField] private string winScene = "WinScene";

    private GameObject[] players;
    private List<GameObject> playersAlive = new List<GameObject>();
    private List<GameObject> playersEliminated = new List<GameObject>();

    void Start()
    {
        StartCoroutine(FindPlayersNextFrame());
    }

    private IEnumerator FindPlayersNextFrame()
    {
        yield return null;
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
            playersAlive.Add(player);
        Debug.Log($"ArenaScript tracking {playersAlive.Count} players");
    }

    public void PlayerEliminated(GameObject player)
    {
        if (playersAlive.Contains(player) && player.GetComponent<PlayerStats>().IsAliveArena == false)
        {
            playersAlive.Remove(player);
            playersEliminated.Add(player);
            Debug.Log(player.name + " eliminated. Remaining: " + playersAlive.Count);
        }
        else
        {
            Debug.LogWarning(player.name + " not found in playersAlive.");
        }

        if (playersAlive.Count == 1)
        {
            Debug.Log("Winner: " + playersAlive[0].name);
            EndGame(playersEliminated, playersAlive[0]);
        }
        else if (playersAlive.Count == 0)
        {
            Debug.Log("Draw!");
            EndGame(playersEliminated);
        }
    }

    private void EndGame(List<GameObject> eliminations)
    {
        GameData.winnerIndex = -1;
        SceneManager.LoadScene(winScene);
    }

    private void EndGame(List<GameObject> eliminations, GameObject winner)
    {
        var playerInput = winner.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        GameData.winnerIndex = playerInput != null ? playerInput.playerIndex : 0;

        // ADD THIS
        foreach (var p in GameData.players)
            Debug.Log($"EndGame — Player {p.playerIndex} kills: {p.kills}, damage: {p.damageDealt}");

        SceneManager.LoadScene(winScene);
    }
}