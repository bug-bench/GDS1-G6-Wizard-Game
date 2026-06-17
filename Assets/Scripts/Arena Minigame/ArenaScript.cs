using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class ArenaScript : MonoBehaviour
{
    [SerializeField] private string winScene = "WinScene";

    [Header("Debug")]
    [SerializeField] 
    private bool singlePlayerDebugMode = false;
    [SerializeField] 
    private bool forceDebugWin = false;

    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> playersEliminated = new List<GameObject>();

    private bool isEnding = false;
    private bool playersReady = false;

    void Start()
    {
        StartCoroutine(FindPlayersNextFrame());
    }

    void Update()
    {
        HandleDebugWin();
        
        if (isEnding || !playersReady) return;

        TryEndGameAfterElimination();
    }

    private IEnumerator FindPlayersNextFrame()
    {
        yield return null;

        players.Clear();
        playersEliminated.Clear();

        foreach (var playerData in GameData.players)
        {
            if (playerData.playerGameObject == null) continue;

            if (!players.Contains(playerData.playerGameObject))
            {
                players.Add(playerData.playerGameObject);
            }
        }

        Debug.Log($"ArenaScript tracking {players.Count} players");

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        if (spawnPoints.Length < players.Count)
        {
            Debug.LogError("Not enough spawn points for all players!");
            yield break;
        }

        ShuffleArray(spawnPoints);

        for (int i = 0; i < players.Count; i++)
        {
            players[i].SetActive(true);
            players[i].transform.position = spawnPoints[i].transform.position;
            players[i].transform.rotation = spawnPoints[i].transform.rotation;

            PlayerInput pi = players[i].GetComponent<PlayerInput>();
            if (pi != null)
            {
                pi.ActivateInput();
            }

            SpriteRenderer sr = players[i].GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
            }
        }

        var countdowns = FindObjectsByType<CountdownUI>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        if (countdowns.Length > 0)
        {
            int done = 0;

            foreach (var cd in countdowns)
            {
                cd.Play(() =>
                {
                    done++;
                });
            }
        }

        playersReady = true;
    }

    // for use when debugging
    public void HandleDebugWin()
    {
        if (!singlePlayerDebugMode) return;
        if (!forceDebugWin) return;
        if (isEnding) return;
        if (players.Count == 0) return;

        forceDebugWin = false;
        isEnding = true;

        Debug.Log("Debug win triggered by Main User.");

        EndGame(players[0]);
    }

    public void PlayerEliminated(GameObject player)
    {
        if (player == null || isEnding) return;

        if (!players.Contains(player))
        {
            Debug.LogWarning($"{player.name} was eliminated but is not tracked by ArenaScript.");
            return;
        }

        if (!playersEliminated.Contains(player))
        {
            playersEliminated.Add(player);
            Debug.Log($"{player.name} eliminated.");
        }

        PlayerInput pi = player.GetComponent<PlayerInput>();
        if (pi != null)
        {
            pi.DeactivateInput();
        }

        SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }

        TryEndGameAfterElimination();
    }

    private List<GameObject> GetAlivePlayers()
    {
        List<GameObject> alivePlayers = new List<GameObject>();

        foreach (GameObject player in players)
        {
            if (player == null) continue;
            if (!player.activeInHierarchy) continue;
            if (playersEliminated.Contains(player)) continue;

            alivePlayers.Add(player);
        }

        return alivePlayers;
    }

    private void TryEndGameAfterElimination()
    {
        if (isEnding) return;
        if (players.Count == 0) return;

        List<GameObject> alivePlayers = GetAlivePlayers();

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

    private void EndGame(GameObject winner)
    {
        PlayerInput playerInput = winner.GetComponent<PlayerInput>();
        GameData.winnerIndex = playerInput != null ? playerInput.playerIndex : 0;

        foreach (var p in GameData.players)
        {
            Debug.Log($"EndGame — Player {p.playerIndex} kills: {p.kills}, damage: {p.damageDealt}");
        }

        SceneManager.LoadScene(winScene);
    }

    private void EndGame(List<GameObject> eliminations)
    {
        GameData.winnerIndex = -1;

        foreach (GameObject player in players)
        {
            if (player == null) continue;

            SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
            }

            PersistentObject persistent = player.GetComponent<PersistentObject>();
            if (persistent != null)
            {
                Destroy(persistent);
            }

            Destroy(player);
        }

        SceneManager.LoadScene(winScene);
    }
}