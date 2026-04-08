using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class ArenaScript : MonoBehaviour
{
    [SerializeField] private string winScene = "WinScene";
    [SerializeField] private bool gameWon = false;

    private GameObject[] players;
    private List<GameObject> playersAlive = new List<GameObject>();
    private List<GameObject> playersEliminated = new List<GameObject>();

    void Start()
    {
        StartCoroutine(FindPlayersNextFrame());
    }

    void Update()
    {
        if (gameWon)
        {
            foreach (GameObject player in players)
            {
                SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
                sr.enabled = false;
                PlayerInput pi = player.GetComponent<PlayerInput>();
                pi.DeactivateInput();
            }
            EndGame(playersEliminated, playersAlive[0]);
        }
    }

    private IEnumerator FindPlayersNextFrame()
    {
        yield return null;
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
            playersAlive.Add(player);
        Debug.Log($"ArenaScript tracking {playersAlive.Count} players");

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints.Length < players.Length)
        {
            Debug.LogError("Not enough spawn points for all players!");
            yield break;
        }

        // Optional: randomize spawn points
        //ShuffleArray(spawnPoints);

        for (int i = 0; i < players.Length; i++)
        {
            players[i].transform.position = spawnPoints[i].transform.position;
            players[i].transform.rotation = spawnPoints[i].transform.rotation;
        }
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