using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class ArenaScript : MonoBehaviour
{
    [SerializeField] private string winScene = "WinScene";
    [SerializeField] private bool gameWon = false;

    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> playersAlive = new List<GameObject>();
    private List<GameObject> playersEliminated = new List<GameObject>();

    private bool isEnding = false;

    void Start()
    {
        StartCoroutine(FindPlayersNextFrame());
    }

    void Update()
    {
        if (isEnding) return;

         var alivePlayers = GetAlivePlayers();
         if (alivePlayers.Count == 1)
         {
             isEnding = true;
             GameObject winner = alivePlayers[0];
             Debug.Log("Winner: " + winner.name);
             EndGame(winner);
         }
         else if (alivePlayers.Count == 0)
         {
             isEnding = true;
             Debug.Log("Draw!");
             EndGame(playersEliminated);
         }
        if (gameWon && !isEnding)
        {
            isEnding = true;
            foreach (GameObject player in players)
            {
                SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
                sr.enabled = false;
                PlayerInput pi = player.GetComponent<PlayerInput>();
                pi.DeactivateInput();
            }
            EndGame(playersAlive[0]);
        }
        TryEndGameAfterElimination();
    }

    private IEnumerator FindPlayersNextFrame()
    {
        yield return null;
        foreach (var player in GameData.players)
        {
            players.Add(player.playerGameObject);
        }
        foreach (GameObject player in players)
            playersAlive.Add(player);
        Debug.Log($"ArenaScript tracking {playersAlive.Count} players");

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints.Length < players.Count)
        {
            Debug.LogError("Not enough spawn points for all players!");
            yield break;
        }

        // Optional: randomize spawn points
        //ShuffleArray(spawnPoints);

        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = spawnPoints[i].transform.position;
            players[i].transform.rotation = spawnPoints[i].transform.rotation;
        }

        var countdowns = FindObjectsByType<CountdownUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int total = countdowns.Length;
        int done  = 0;

        if (total == 0) yield break;

        foreach (var cd in countdowns)
        {
            cd.Play(() =>
            {
                done++;
            });
        }
    }

    public void PlayerEliminated(GameObject player)
    {
        if (player == null) return;

        if (!playersEliminated.Contains(player))
        {
            playersEliminated.Add(player);
            Debug.Log(player.name + " eliminated.");
        }

        TryEndGameAfterElimination();
        // playersAlive.RemoveAll(p => p == null);

        // if (player == null)
        // {
        //     Debug.LogWarning("PlayerEliminated called with null reference.");
        //     TryEndGameAfterElimination();
        //     return;
        // }

        // PlayerStats stats = player.GetComponent<PlayerStats>();
        // if (playersAlive.Contains(player) && stats != null && stats.IsAliveArena == false)
        // {
        //     playersAlive.Remove(player);
        //     playersEliminated.Add(player);
        //     Debug.Log(player.name + " eliminated. Remaining: " + playersAlive.Count);
        // }
        // else
        // {
        //     Debug.LogWarning(player.name + " not found in playersAlive or still marked alive.");
        // }

        // TryEndGameAfterElimination();
    }

    List<GameObject> GetAlivePlayers()
    {
        List<GameObject> alive = new List<GameObject>();
        if (players.Count == 0)
        {
            TryFindPlayers();
        }
        if (players.Count == 0) return new List<GameObject>();
        

        foreach (var p in players)
        {
            if (p != null && p.activeInHierarchy)
            {
                alive.Add(p);
            }
        }

        return alive;
    }

    void TryFindPlayers()
    {
        foreach (var player in GameData.players)
        {
            if (!players.Contains(player.playerGameObject))
            {
                players.Add(player.playerGameObject);
            }
        }
    }

    void TryEndGameAfterElimination()
    {
        if (players.Count == 0) return;

        var alivePlayers = GetAlivePlayers();

        if (alivePlayers.Count == 1 && !isEnding)
        {
            isEnding = true;
            GameObject winner = alivePlayers[0];
            Debug.Log("Winner: " + winner.name);
            EndGame(winner);
        }
        else if (alivePlayers.Count == 0 && !isEnding)
        {
            isEnding = true;
            Debug.Log("Draw!");
            EndGame(playersEliminated);
        }
        // playersAlive.RemoveAll(p => p == null);

        // if (playersAlive.Count == 1)
        // {
        //     GameObject winner = playersAlive[0];
        //     if (winner == null)
        //     {
        //         playersAlive.RemoveAll(p => p == null);
        //         if (playersAlive.Count == 0)
        //             EndGame(playersEliminated);
        //         return;
        //     }

        //     Debug.Log("Winner: " + winner.name);
        //     EndGame(playersEliminated, winner);
        // }
        // else if (playersAlive.Count == 0)
        // {
        //     Debug.Log("Draw!");
        //     EndGame(playersEliminated);
        // }
    }

    private void ShuffleArray(GameObject[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rand = Random.Range(i, array.Length);
            GameObject temp = array[i];
            array[i] = array[rand];
            array[rand] = temp;
        }
    }

    private void EndGame(List<GameObject> eliminations)
    {
        GameData.winnerIndex = -1;
        foreach (GameObject player in players)
        {
            SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
            sr.enabled = false;
            Destroy(player.GetComponent<PersistentObject>());
            Destroy(player.gameObject);
        }
        SceneManager.LoadScene(winScene);
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