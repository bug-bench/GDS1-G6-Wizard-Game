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
    [SerializeField] private TimerUI centralTimer;
    [SerializeField] private TMPro.TextMeshProUGUI winnerText;


    private List<GameObject> players = new List<GameObject>();

    protected CollectManager cm;

    private GameObject winner = null;

    [SerializeField] private float minigameLength = 60f;
    private float timer = 0;

    void Start()
    {
        Time.timeScale = 1f;

        StartCoroutine(SetupNextFrame());
    }

    void UpdateTimerUI(float value)
    {
        if (centralTimer == null) return;
        centralTimer.UpdateTimer(value);
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

        var countdowns = FindObjectsByType<CountdownUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int total = countdowns.Length;
        int done  = 0;

        if (total == 0)
        {
            BeginGame();
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

    void Update()
    {
        if (players.Count <= 0)
        {
            foreach (var player in GameData.players)
            {
                players.Add(player.playerGameObject);
            }
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

    public int GetTimer()
    {
        return Mathf.CeilToInt(timer);
    }

    public void BeginGame()
    {
        StartCoroutine(GameTimer());
    }

    IEnumerator GameTimer()
    {
        timer = minigameLength;
        centralTimer?.Init(minigameLength);

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            centralTimer?.UpdateTimer(timer);
            BGMManager.Instance?.SetTimeRemaining(timer);
            yield return null;
        }

        timer = 0;

        centralTimer?.Init(minigameLength);

        winner = cm.RegisterGameEnd();

        yield return new WaitForSeconds(2f);

        EndGame(winner);
    }

    private void EndGame(GameObject winner)
    {
        Debug.Log("END GAME CALLED");
        Time.timeScale = 0f;

        // Set winner
        var winnerData = GameData.players.Find(p => p.playerGameObject == winner);
        if (winnerData != null)
        {
            GameData.winnerIndex = winnerData.playerIndex;
            if (winnerText != null)
                winnerText.text = $"P{winnerData.playerIndex + 1} Wins!";
        }
        else if (winnerText != null)
            winnerText.text = "Draw!";

        winPanel.SetActive(true);

        List<KeyValuePair<GameObject, int>> rankings = cm.GetRankings();
        float containerHeight = podiumContainer.GetComponent<RectTransform>().rect.height;
        int topScore = rankings.Count > 0 ? rankings[0].Value : 1;

        for (int i = 0; i < rankings.Count; i++)
        {
            GameObject player = rankings[i].Key;
            int score         = rankings[i].Value;

            var data = GameData.players.Find(p => p.playerGameObject == player);
            if (data == null) continue;

            GameObject result = Instantiate(resultPrefab, podiumContainer);
            PlayerResultUI ui = result.GetComponent<PlayerResultUI>();
            if (ui == null) continue;

            float ratio  = topScore > 0 ? (float)score / topScore : 0f;
            float height = containerHeight * ratio;

            ui.Setup(i + 1, data.playerIndex, data.colorIndex, score.ToString(), height);
        }
    }
}