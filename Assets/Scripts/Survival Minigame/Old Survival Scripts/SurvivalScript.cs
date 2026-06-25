using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SurvivalScript : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField]
    private bool manualWinDebugMode = false;
    [SerializeField]
    private bool forceDebugWin = false;

    [Header("Win Screen")]
    [SerializeField] private GameObject resultsPanel;
    // [SerializeField] private UnityEngine.UI.Image winnerImage;
    [SerializeField] private TMPro.TextMeshProUGUI winnerText;
    [SerializeField] private Transform podiumContainer;
    [SerializeField] private GameObject resultPrefab;

    private GameObject winner = null;


    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> playersAlive = new List<GameObject>();
    private List<GameObject> playersEliminated = new List<GameObject>();
    // Add with other private fields
    private float survivalStartTime;
    private Dictionary<GameObject, float> playerSurvivalTimes = new Dictionary<GameObject, float>();

    private List<SurvivalHazard> hazards = new List<SurvivalHazard>();

    private int hazardsFinishedThisLoop = 0;
    private int totalHazards;
    

    private int loopCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SetupNextFrame());
    }
    private bool gameEnded = false;

    void Update()
    {
        if (gameEnded) return;

        HandleDebugWin();

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

        if (GameData.players.Count <= 1)
        {
            manualWinDebugMode = true;

            Debug.Log(
                $"Survival: Auto-enabled Manual Win Debug Mode " +
                $"because only {GameData.players.Count} player(s) were detected."
            );
        }

        foreach (GameObject p in players)
        {
            playerSurvivalTimes[p] = 0f;
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

        var countdowns = FindObjectsByType<CountdownUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int total = countdowns.Length;
        int done  = 0;

        if (total == 0)
        {
            BeginGame(); // your existing start logic
            yield break;
        }

        foreach (var cd in countdowns)
        {
            cd.Play(() =>
            {
                done++;
                if (done >= total)
                    BeginGame();
            });
        }
    }

    void BeginGame()
    {
        // Enable hazards
        survivalStartTime = Time.time;
        Debug.Log($"BeginGame — survivalStartTime: {survivalStartTime}");
        foreach (var hazard in hazards)
            hazard.enabled = true;
    }

    public void RegisterHazard(SurvivalHazard hazard)
    {
        if (!hazards.Contains(hazard))
        {
            hazards.Add(hazard);
            totalHazards = hazards.Count;
        }
    }

    public bool IsUsingMouseDebugTarget()
    {
        return manualWinDebugMode && GameData.players.Count <= 0;
    }

    // ====================
    // PLAYER ELIMINATION
    // ====================

    public void PlayerEliminated(GameObject player)
    {
        if (player == null) return;

        if (!playersEliminated.Contains(player))
        {
            float survivalTime = Time.time - survivalStartTime;
            playerSurvivalTimes[player] = Time.time - survivalStartTime;
            playersEliminated.Add(player);
            Debug.Log($"{player.name} eliminated at {survivalTime}s (survivalStartTime: {survivalStartTime}, Time.time: {Time.time})");
            if (player.activeInHierarchy)
            {
                player.SetActive(false);
            }
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

        Debug.Log(player.name + " eliminated. Remaining: " + GetAlivePlayers().Count);

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
            bool SuppressAutomaticWin = manualWinDebugMode && GameData.players.Count <= 1;
            if (SuppressAutomaticWin) return;

            gameEnded = true;
            GameObject winner = alivePlayers[0];
            
            // Record winner's time NOW — when last opponent dies
            playerSurvivalTimes[winner] = Time.time - survivalStartTime;
            Debug.Log($"Winner: {winner.name} at {playerSurvivalTimes[winner]}s");
            EndGame(winner);
        }
        else if (alivePlayers.Count == 0)
        {
            gameEnded = true;
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
        // Record winner's survival time
        if (winner != null && playerSurvivalTimes.ContainsKey(winner))
        {
            playerSurvivalTimes[winner] = Time.time - survivalStartTime;
            float elapsed = Time.time - survivalStartTime;
            Debug.Log($"Winner {winner.name} survived {elapsed}s + 1 bonus");
        }
        Time.timeScale = 0f;

        foreach (var p in GameData.players)
        {
            if (p.playerGameObject == null) continue;
            var input = p.playerGameObject.GetComponent<PlayerInput>();
            if (input != null) input.DeactivateInput();
            var rb = p.playerGameObject.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        if (winner != null)
        {
            var winnerData = GameData.players.Find(p => p.playerGameObject == winner);
            if (winnerData != null)
            {
                GameData.winnerIndex = winnerData.playerIndex;
                playerSurvivalTimes[winner] = (Time.time - survivalStartTime) + 1f;
            }
        }
        if (winnerText != null)
        winnerText.text = winner != null
            ? $"P{GameData.winnerIndex + 1} Wins!"
            : "Draw!";

        resultsPanel.SetActive(true);

        // Build rankings — sort by survival time descending
        List<KeyValuePair<GameObject, float>> rankings = new List<KeyValuePair<GameObject, float>>();
        foreach (var p in players)
        {
            float t = playerSurvivalTimes.ContainsKey(p) ? playerSurvivalTimes[p] : 0f;
            rankings.Add(new KeyValuePair<GameObject, float>(p, t));
        }
        rankings.Sort((a, b) => b.Value.CompareTo(a.Value));

        // Clear old UI
        foreach (Transform child in podiumContainer)
            Destroy(child.gameObject);

        float containerHeight = podiumContainer.GetComponent<RectTransform>().rect.height;
        float topTime = rankings.Count > 0 ? rankings[0].Value : 1f;

        for (int i = 0; i < rankings.Count; i++)
        {
            GameObject player = rankings[i].Key;
            float survivalTime = rankings[i].Value;

            var data = GameData.players.Find(p => p.playerGameObject == player);
            if (data == null) continue;

            GameObject result = Instantiate(resultPrefab, podiumContainer);
            PlayerResultUI ui = result.GetComponent<PlayerResultUI>();
            if (ui == null) continue;

            int mins = Mathf.FloorToInt(survivalTime / 60f);
            int secs = Mathf.FloorToInt(survivalTime % 60f);
            string timeStr = $"{mins:00}:{secs:00}";

            // 1st place always full height, others scale down but minimum 20%
            float ratio = topTime > 0 ? survivalTime / topTime : 0f;
            float minHeight = containerHeight * 0.2f;
            float maxHeight = containerHeight;
            float height = i == 0 ? maxHeight : Mathf.Max(minHeight, maxHeight * ratio);

            ui.Setup(i + 1, data.playerIndex, data.colorIndex, timeStr, height);
        }
    }

    private void HandleDebugWin()
    {
        if (!manualWinDebugMode) return;
        if (!forceDebugWin) return;

        forceDebugWin = false;
        gameEnded = true;

        Debug.Log("Debug win triggered by Main User");

        StopAllCoroutines();

        GameObject winner = players.Count > 0 ? players[0] : null;

        EndGame(winner);
    }

    public bool IsManualWinDebugMode()
    {
        return manualWinDebugMode;
    }
}
