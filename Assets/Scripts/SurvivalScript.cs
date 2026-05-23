using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SurvivalScript : MonoBehaviour
{
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private UnityEngine.UI.Image winnerImage;
    [SerializeField] private TMPro.TextMeshProUGUI winnerText;

    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> playersAlive = new List<GameObject>();
    private List<GameObject> playersEliminated = new List<GameObject>();

    private List<SurvivalHazard> hazards = new List<SurvivalHazard>();

    private int hazardsFinishedThisLoop = 0;
    private int totalHazards;

    private int loopCount = 0;

    [Header("Survival Settings")]
    [SerializeField] private int maxHits = 3;

    private Dictionary<GameObject, int> playerHits = new Dictionary<GameObject, int>();

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
            playerHits[p] = 0;
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

    public void TakeSurvivalHit(GameObject player)
    {
        if (player == null) return;

        if (!playerHits.ContainsKey(player)) return;

        playerHits[player]++;

        Debug.Log($"{player.name} took hit " + $"{playerHits[player]}/{maxHits}");

        if (playerHits[player] >= maxHits)
        {
            PlayerEliminated(player);
            player.SetActive(false);
        }
    }

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
        if (winner == null)
        {
            Debug.Log("No winner (draw)");
            resultsPanel.SetActive(true);
            winnerText.text = "DRAW";
            winnerImage.enabled = false;
            return;
        }

        var data = GameData.players.Find(p => p.playerGameObject == winner);

        if (data == null)
        {
            Debug.LogError("Winner data not found in GameData");
            return;
        }

        GameData.winnerIndex = data.playerIndex;

        Debug.Log($"Winner: P{data.playerIndex + 1}");

        // freeze game
        Time.timeScale = 0f;

        foreach (var p in GameData.players)
        {
            var input = p.playerGameObject.GetComponent<PlayerInput>();
            if (input != null) input.DeactivateInput();

            var rb = p.playerGameObject.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // show UI
        resultsPanel.SetActive(true);

        winnerText.text = $"P{data.playerIndex + 1} WINS!";

        // image + color
        winnerImage.sprite = data.playerSprite;
        winnerImage.color = PlayerData.PlayerColors.GetColor(data.colorIndex);
    }
}
