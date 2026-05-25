using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SurvivalScript : MonoBehaviour
{
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

    [Header("Survival Settings")]
    [SerializeField] private int maxHits = 3;

    private Dictionary<GameObject, int> playerHits = new Dictionary<GameObject, int>();

    [SerializeField]
    private float hitInvulnerabilityTime = 0.75f;

    [Header("Hit Feedback")]
    [SerializeField] 
    private Color hitFlashColor = Color.white;
    [SerializeField] 
    private int hitFlashBlinkCount = 2;
    [SerializeField] 
    private float hitFlashHalfDuration = 0.04f;
    [SerializeField] 
    private float invulnerabilityBlinkInterval = 0.12f;

    private Dictionary<GameObject, float> playerInvulnerabilityTimers = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, Coroutine> playerFlashCoroutines = new Dictionary<GameObject, Coroutine>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SetupNextFrame());
    }
    private bool gameEnded = false;

    void Update()
    {
        if (gameEnded) return;
        if (players.Count > 0)
        {
            TryEndGameAfterElimination();
        }

        #if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current.tKey.wasPressedThisFrame)
        {
            StopAllCoroutines();
            winner = players.Count > 0 ? players[0] : null;
            EndGame(winner);
        }
        #endif
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
            playerSurvivalTimes[p] = 0f;
            playersAlive.Add(p);
            playerHits[p] = 0;
            playerInvulnerabilityTimers[p] = 0f;
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

    // ====================
    // PLAYER ELIMINATION
    // ====================

    public void TakeSurvivalHit(GameObject player)
    {
        if (player == null) return;

        if (!playerHits.ContainsKey(player)) return;

        if (Time.time < playerInvulnerabilityTimers[player]) return;

        playerInvulnerabilityTimers[player] = Time.time + hitInvulnerabilityTime;

        playerHits[player]++;
        if (playerFlashCoroutines.ContainsKey(player))
        {
            if (playerFlashCoroutines[player] != null)
            {
                StopCoroutine(playerFlashCoroutines[player]);
            }
        }

        playerFlashCoroutines[player] = StartCoroutine(SurvivalHitFlashRoutine(player));

        Debug.Log($"{player.name} took hit " + $"{playerHits[player]}/{maxHits}");

        if (playerHits[player] >= maxHits)
        {
            PlayerEliminated(player);
            player.SetActive(false);
        }
    }

    //feedback on player hit by object
    private IEnumerator SurvivalHitFlashRoutine(GameObject player)
    {
        if (player == null) yield break;

        SpriteRenderer[] renderers = player.GetComponentsInChildren<SpriteRenderer>();

        if (renderers.Length == 0) yield break;

        Color[] originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
        }

        for (int flash = 0; flash < hitFlashBlinkCount; flash++)
        {
            foreach (var sr in renderers)
            {
                if (sr != null)
                {
                    sr.color = hitFlashColor;
                }
            }

            yield return new WaitForSeconds(hitFlashHalfDuration);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].color = originalColors[i];
                }
            }

            yield return new WaitForSeconds(hitFlashHalfDuration);
        }
        while (player != null && playerInvulnerabilityTimers.ContainsKey(player) && Time.time < playerInvulnerabilityTimers[player])
        {
            foreach (var sr in renderers)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 0f;
                    sr.color = c;
                }
            }

            yield return new WaitForSeconds(invulnerabilityBlinkInterval);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].color = originalColors[i];
                }
            }

            yield return new WaitForSeconds(invulnerabilityBlinkInterval);
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = originalColors[i];
            }
        }
    }

    public void PlayerEliminated(GameObject player)
    {
        if (player == null) return;

        if (!playersEliminated.Contains(player))
        {
            float survivalTime = Time.time - survivalStartTime;
            playerSurvivalTimes[player] = Time.time - survivalStartTime;
            playersEliminated.Add(player);
            Debug.Log($"{player.name} eliminated at {survivalTime}s");
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
        if (winner != null && playerSurvivalTimes.ContainsKey(winner) && playerSurvivalTimes[winner] == 0f)
        playerSurvivalTimes[winner] = Time.time - survivalStartTime;

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
                GameData.winnerIndex = winnerData.playerIndex;
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
}
