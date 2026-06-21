using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ArenaScript : MonoBehaviour
{
    [SerializeField] private string winScene = "WinScene";

    [Header("Win Panel")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Image playerWonImg;
    [SerializeField] private TextMeshProUGUI playerWonTxt;
    [SerializeField] private TextMeshProUGUI damageDealtTxt;
    [SerializeField] private TextMeshProUGUI eliminationsTxt;

    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> playersEliminated = new List<GameObject>();

    private bool isEnding    = false;
    private bool playersReady = false;

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        StartCoroutine(FindPlayersNextFrame());
    }

    void Update()
    {
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
                players.Add(playerData.playerGameObject);
        }

        Debug.Log($"ArenaScript tracking {players.Count} players");

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints.Length < players.Count)
        {
            Debug.LogError("Not enough spawn points!");
            yield break;
        }

        ShuffleArray(spawnPoints);

        for (int i = 0; i < players.Count; i++)
        {
            players[i].SetActive(true);
            players[i].transform.position = spawnPoints[i].transform.position;
            players[i].transform.rotation = spawnPoints[i].transform.rotation;

            var pi = players[i].GetComponent<PlayerInput>();
            if (pi != null) pi.ActivateInput();

            var sr = players[i].GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
        }

        var countdowns = FindObjectsByType<CountdownUI>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (countdowns.Length > 0)
        {
            int done = 0;
            foreach (var cd in countdowns)
                cd.Play(() => done++);
        }

        playersReady = true;
    }

    public void PlayerEliminated(GameObject player)
    {
        if (player == null || isEnding) return;

        if (!players.Contains(player))
        {
            Debug.LogWarning($"{player.name} not tracked by ArenaScript.");
            return;
        }

        if (!playersEliminated.Contains(player))
        {
            playersEliminated.Add(player);
            Debug.Log($"{player.name} eliminated.");
        }

        var pi = player.GetComponent<PlayerInput>();
        if (pi != null) pi.DeactivateInput();

        var sr = player.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        TryEndGameAfterElimination();
    }

    private List<GameObject> GetAlivePlayers()
    {
        var alive = new List<GameObject>();
        foreach (var player in players)
        {
            if (player == null) continue;
            if (!player.activeInHierarchy) continue;
            if (playersEliminated.Contains(player)) continue;
            alive.Add(player);
        }
        return alive;
    }

    private void TryEndGameAfterElimination()
    {
        if (isEnding || players.Count == 0) return;

        var alive = GetAlivePlayers();

        if (alive.Count == 1)
        {
            isEnding = true;
            EndGame(alive[0]);
        }
        else if (alive.Count == 0)
        {
            isEnding = true;
            EndGame((GameObject)null);
        }
    }

    private void ShuffleArray(GameObject[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rand    = Random.Range(i, array.Length);
            var temp    = array[i];
            array[i]    = array[rand];
            array[rand] = temp;
        }
    }

    private void EndGame(GameObject winner)
    {
        Time.timeScale = 0f;

        // Freeze all players
        foreach (var p in GameData.players)
        {
            if (p.playerGameObject == null) continue;
            var pi = p.playerGameObject.GetComponent<PlayerInput>();
            if (pi != null) pi.DeactivateInput();
            var rb = p.playerGameObject.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        if (winner != null)
        {
            var pi = winner.GetComponent<PlayerInput>();
            GameData.winnerIndex = pi != null ? pi.playerIndex : 0;
        }
        else
        {
            GameData.winnerIndex = -1;
        }

        ShowWinPanel(winner);
    }

    private void ShowWinPanel(GameObject winner)
    {
        if (winPanel == null) return;
        winPanel.SetActive(true);

        if (winner == null)
        {
            if (playerWonTxt    != null) playerWonTxt.text    = "DRAW!";
            if (damageDealtTxt  != null) damageDealtTxt.text  = "";
            if (eliminationsTxt != null) eliminationsTxt.text = "";
            if (playerWonImg    != null) playerWonImg.enabled = false;
            return;
        }

        var data = GameData.players.Find(p => p.playerIndex == GameData.winnerIndex);
        if (data == null)
        {
            if (playerWonTxt != null) playerWonTxt.text = "NO DATA";
            return;
        }

        if (playerWonImg != null)
        {
            playerWonImg.enabled = true;
            playerWonImg.color   = PlayerData.PlayerColors.GetColor(data.colorIndex);
            if (data.playerSprite != null)
                playerWonImg.sprite = data.playerSprite;
        }

        if (playerWonTxt    != null) playerWonTxt.text    = $"P{data.playerIndex + 1} WINS!";
        if (damageDealtTxt  != null) damageDealtTxt.text  = $"Damage: {Mathf.RoundToInt(data.damageDealt)}";
        if (eliminationsTxt != null) eliminationsTxt.text = $"Eliminations: {data.kills}";

        foreach (var p in GameData.players)
            Debug.Log($"Player {p.playerIndex} — kills: {p.kills}, damage: {p.damageDealt}");
    }
}