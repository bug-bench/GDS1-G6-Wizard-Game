using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SurvivalScript : MonoBehaviour
{
    [SerializeField] private string winScene = "WinScene";

    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> playersAlive = new List<GameObject>();
    private List<GameObject> playersEliminated = new List<GameObject>();

    private List<SurvivalHazard> hazards = new List<SurvivalHazard>();

    private int hazardsFinishedThisLoop = 0;
    private int totalHazards;

    private int loopCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SetupNextFrame());
    }

    void Update()
    {
        if (players.Count > 0)
        {
            TryEndGameAfterElimination();
        }
    }

    IEnumerator SetupNextFrame()
    {
        yield return null;

        foreach (var player in GameData.players)
        {
            players.Add(player.playerGameObject);
        }
        foreach (GameObject p in players)
        {
            playersAlive.Add(p);
        }

        hazards.Clear();

        var foundHazards = FindObjectsByType <SurvivalHazard>(FindObjectsSortMode.None);

        foreach (var hazard in foundHazards)
        {
            hazard.SetManager(this);
            hazards.Add(hazard);
        }

        totalHazards = hazards.Count;

        Debug.Log($"Survival started with {playersAlive.Count} players and {totalHazards} hazards");
    }

    public void RegisterHazard(SurvivalHazard hazard)
    {
        if (!hazards.Contains(hazard))
        {
            hazards.Add(hazard);
            totalHazards = hazards.Count;
        }
    }

    // ====================
    // PLAYER ELIMINATION
    // ====================

    public void PlayerEliminated(GameObject player)
    {
        if (player == null) return;

        if (!playersEliminated.Contains(player))
        {
            playersEliminated.Add(player);
        }

        var alivePlayers = new List<GameObject>();

        foreach (var p in players)
        {
            if (p != null && p.activeInHierarchy)
            {
                alivePlayers.Add(p);
            }
        }
        // if (!playersAlive.Contains(player)) return;

        // playersAlive.Remove(player);
        // playersEliminated.Add(player);

        Debug.Log(player.name + " eliminated. Remaining: " + playersAlive.Count);

        TryEndGameAfterElimination();
        // if (playersAlive.Count == 1)
        // {
        //     EndGame(playersAlive[0]);
        // }
        // else if (playersAlive.Count == 0)
        // {
        //     EndGame(null);
        // }
    }

    List<GameObject> GetAlivePlayers()
    {
        List<GameObject> alive = new List<GameObject>();

        foreach (var p in players)
        {
            if (p != null && p.activeInHierarchy)
            {
                alive.Add(p);
            }
        }

        return alive;
    }

    void TryEndGameAfterElimination()
    {
        var alivePlayers = GetAlivePlayers();

        if (alivePlayers.Count == 1)
        {
            GameObject winner = alivePlayers[0];
            Debug.Log("Winner: " + winner.name);
            EndGame(winner);
        }
        else if (alivePlayers.Count == 0)
        {
            Debug.Log("Draw!");
            EndGame(null);
        }
    }

    // ====================
    // HAZARD LOOP SYSTEM
    // ====================

    public void HazardFinished()
    {
        hazardsFinishedThisLoop++;

        if (hazardsFinishedThisLoop >= totalHazards)
        {
            StartCoroutine(NextLoop());
        }
    }

    IEnumerator NextLoop()
    {
        hazardsFinishedThisLoop = 0;
        loopCount++;

        Debug.Log("New loop: " + loopCount);

        float difficultyMultiplier = Mathf.Pow(1.15f, loopCount); // exponential scaling per loop

        foreach (var hazard in hazards)
        {
            hazard.IncreaseDifficulty(difficultyMultiplier);
            hazard.ResetToStart();
        }

        yield return null;
    }

    // ====================
    // END GAME
    // ====================

    void EndGame(GameObject winner)
    {
        if (winner != null)
        {
            var input = winner.GetComponent<PlayerInput>();
            GameData.winnerIndex = input != null ? input.playerIndex : 0;
        }
        else
        {
            GameData.winnerIndex = -1;
        }

        foreach (var p in GameData.players)
        {
            Debug.Log($"Survival Ended - Player {p.playerIndex} kills: {p.kills}, damage: {p.damageDealt}");
        }

        SceneManager.LoadScene(winScene);
    }
}
