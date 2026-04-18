using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SurvivalScript : MonoBehaviour
{
    [SerializeField] private string winScene = "WinScene";

    private GameObject[] players;
    private List<GameObject> playersAlive = new List<GameObject>();
    private List<GameObject> playersEliminated = new List<GameObject>();

    private SurvivalHazard[] hazards;

    private int hazardsFinishedThisLoop = 0;
    private int totalHazards;

    private int loopCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SetupNextFrame());
    }

    IEnumerator SetupNextFrame()
    {
        yield return null;

        players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            playersAlive.Add(p);
        }

        hazards = FindObjectsByType<SurvivalHazard>(FindObjectsSortMode.None);
        totalHazards = hazards.Length;

        foreach (var hazard in hazards)
        {
            hazard.SetManager(this);
        }

        Debug.Log($"Survival started with {playersAlive.Count} players and {totalHazards} hazards");
    }

    // ====================
    // PLAYER ELIMINATION
    // ====================

    public void PlayerEliminated(GameObject player)
    {
        if (!playersAlive.Contains(player)) return;

        playersAlive.Remove(player);
        playersEliminated.Add(player);

        Debug.Log(player.name + " eliminated. Remaining: " + playersAlive.Count);

        if (playersAlive.Count == 1)
        {
            EndGame(playersAlive[0]);
        }
        else if (playersAlive.Count == 0)
        {
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
