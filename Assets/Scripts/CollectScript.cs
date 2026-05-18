using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal.Filters;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectScript : MonoBehaviour
{
    [Header("Win Screen")]
    [SerializeField] GameObject winPanel;
    [SerializeField] Transform podiumContainer;
    [SerializeField] GameObject resultPrefab;

    [Header("UI")]
    [SerializeField] GameObject collectScorePrefab;
    [SerializeField] private TMPro.TextMeshProUGUI[] timerTexts;

    private List<GameObject> players = new List<GameObject>();

    protected CollectManager cm;

    private GameObject winner = null;

    [SerializeField] private float minigameLength = 60f;

    private float timer = 0;

    void Start()
    {
        Time.timeScale = 1f;

        StartCoroutine(SetupNextFrame());
        StartCoroutine(GameTimer());

        StartCoroutine(FindTimerTextsNextFrame());
    }

    IEnumerator FindTimerTextsNextFrame()
    {
        yield return null;

        var allTexts = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);

        var found = new List<TMPro.TextMeshProUGUI>();

        foreach (var t in allTexts)
        {
            if (t.gameObject.name == "TimerText")
                found.Add(t);
        }

        timerTexts = found.ToArray();

        Debug.Log($"CollectScript found {timerTexts.Length} timer texts");
    }

    void UpdateTimerUI(string value)
    {
        if (timerTexts == null) return;

        foreach (var t in timerTexts)
        {
            if (t != null)
                t.text = $"Time Left: {value}";
        }
    }

    IEnumerator SetupNextFrame()
    {
        yield return null;

        cm = GetComponent<CollectManager>();

        foreach (var player in GameData.players)
        {
            players.Add(player.playerGameObject);

            cm.SetupPlayer(player.playerGameObject);

            GameObject ui = Instantiate(collectScorePrefab);

            CollectScoreUI scoreUI = ui.GetComponent<CollectScoreUI>();

            scoreUI.Setup(player.playerGameObject, cm);
        }

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        if (spawnPoints.Length < players.Count)
        {
            Debug.LogError("Not enough spawn points for all players!");
            yield break;
        }

        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = spawnPoints[i].transform.position;
            players[i].transform.rotation = spawnPoints[i].transform.rotation;
        }
    }

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
        timer = minigameLength;

        UpdateTimerUI(Mathf.CeilToInt(timer).ToString());

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            UpdateTimerUI(Mathf.CeilToInt(timer).ToString());

            yield return null;
        }

        timer = 0;

        UpdateTimerUI("0");

        winner = cm.RegisterGameEnd();

        yield return new WaitForSeconds(2f);

        EndGame(winner);
    }

    private void EndGame(GameObject winner)
    {
        Debug.Log("END GAME CALLED");
        Time.timeScale = 0f;

        winPanel.SetActive(true);
        Debug.Log("WIN PANEL ENABLED");

        List<KeyValuePair<GameObject, int>> rankings = cm.GetRankings();

        for (int i = 0; i < rankings.Count; i++)
        {
            Debug.Log("SPAWNING RESULT");
            GameObject player = rankings[i].Key;

            int score = rankings[i].Value;

            GameObject result = Instantiate(resultPrefab, podiumContainer);

            PlayerResultUI ui = result.GetComponent<PlayerResultUI>();

            var playerInput = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();

            int playerIndex = playerInput.playerIndex;

            int colorIndex = GameData.players[playerIndex].colorIndex;

            float height = 300f - (i * 60f);

            ui.Setup(
                i + 1,
                playerIndex,
                colorIndex,
                score,
                height
            );
        }
    }
}