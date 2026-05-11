using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal.Filters;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectScript : MonoBehaviour
{
    [SerializeField] private string winScene = "WinScene";

    private List<GameObject> players = new List<GameObject>();
    protected CollectManager cm;
    private GameObject winner = null;
    [SerializeField] private float minigameLength = 60f;
    private float timer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SetupNextFrame());
        StartCoroutine(GameTimer());
    }

    IEnumerator SetupNextFrame()
    {
        yield return null;
        cm = GetComponent<CollectManager>();
        foreach (var player in GameData.players)
        {
            players.Add(player.playerGameObject);
            cm.SetupPlayer(player.playerGameObject);
        }
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
    }

    // Update is called once per frame
    void Update()
    {
        if (players.Count <= 0)
        {
            foreach (var player in GameData.players)
            {
                players.Add(player.playerGameObject);
            }
        }
    }

    public int GetTimer()
    {
        return Mathf.CeilToInt(timer);
    }

    IEnumerator GameTimer()
    {
        //setup time
        yield return new WaitForSeconds(5f);
        //Game time
        timer = minigameLength;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        timer = 0;
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
