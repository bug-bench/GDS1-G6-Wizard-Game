using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PotatoManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PotatoPickup potatoPickup;
    [SerializeField] private GameObject potatoTimerPrefab;

    [Header("Game")]
    [SerializeField] private float respawnDelay = 5f;

    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> eliminatedPlayers = new List<GameObject>();

    private bool playersReady = false;
    private bool gameRunning = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SetupNextFrame());
    }

    private IEnumerator SetupNextFrame()
    {
        yield return null;

        players.Clear();

        foreach (var player in GameData.players)
        {
            if (player.playerGameObject == null) continue;

            if (!players.Contains(player.playerGameObject))
            {
                players.Add(player.playerGameObject);
            }

            GameObject ui = Instantiate(potatoTimerPrefab);

            PotatoTimerUI timerUI = ui.GetComponent<PotatoTimerUI>();

            timerUI.Setup(player.playerGameObject, potatoPickup);
        }

        Debug.Log($"PotatoManager tracking {players.Count} players");

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        if (spawnPoints.Length < players.Count)
        {
            Debug.LogError("Not enough spawn points for all players!");
            yield break;
        }

        ShuffleArray(spawnPoints);

        for (int i = 0; i < players.Count; i++)
        {
            GameObject player = players[i];

            player.SetActive(true);

            player.transform.SetPositionAndRotation(spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);

            PlayerInput input = player.GetComponent<PlayerInput>();
            if (input != null)
            {
                input.ActivateInput();
            }

            SpriteRenderer sprite = player.GetComponentInChildren<SpriteRenderer>();

            if (sprite != null)
            {
                sprite.enabled = true;
            }
        }

        var countdowns = FindObjectsByType<CountdownUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (countdowns.Length == 0)
        {
            playersReady = true;
            yield break;
        }

        int completed = 0;

        foreach (var countdown in countdowns)
        {
            countdown.Play(() =>
            {
                completed++;

                if (completed >= countdowns.Length)
                {
                    BeginGame();
                }
            });
        }
    }

    private void ShuffleArray(GameObject[] array)
    {
        for ( int i = 0; i < array.Length; i++)
        {
            int randomIndex = Random.Range(i, array.Length);

            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }
    }

    private void BeginGame()
    {
        playersReady = true;
        gameRunning = true;

        Debug.Log("Hot Potato game started.");

        GameObject firstHolder = ChooseRandomAlivePlayer();

        if (firstHolder != null)
        {
            potatoPickup.AssignPlayer(firstHolder);
            potatoPickup.ResetTimer();
        }
    }

    private GameObject ChooseRandomAlivePlayer()
    {
        List<GameObject> alivePlayers = GetAlivePlayers();

        if (alivePlayers.Count == 0) return null;

        int randomIndex = Random.Range(0, alivePlayers.Count);

        return alivePlayers[randomIndex];
    }

    public void PlayerEliminated(GameObject player)
    {
        if (player == null) return;

        if (eliminatedPlayers.Contains(player)) return;

        eliminatedPlayers.Add(player);

        Debug.Log($"{player.name} eliminated.");

        TryEndGame();

        if (!gameRunning) return;

        StartCoroutine(BeginNextRound());
    }

    private IEnumerator BeginNextRound()
    {
        potatoPickup.RemovePlayer();

        yield return new WaitForSeconds(respawnDelay);

        GameObject nextHolder = ChooseRandomAlivePlayer();

        if (nextHolder == null) yield break;

        potatoPickup.ResetTimer();

        potatoPickup.AssignPlayer(nextHolder);
    }

    private void TryEndGame()
    {
        List<GameObject> alivePlayers = GetAlivePlayers();

        if (alivePlayers.Count <= 1)
        {
            gameRunning = false;

            GameObject winner = alivePlayers.Count == 1 ? alivePlayers[0] : null;

            EndGame(winner);
        }
    }

    // Here is a dummy function for you to complete your winner card carson:
    private void EndGame(GameObject winner)
    {
        Debug.Log("Hot Potato finished.");

        if (winner != null)
        {
            Debug.Log($"{winner.name} wins!");
        }
        else
        {
            Debug.Log("Draw!");
        }
    }

    private List<GameObject> GetAlivePlayers()
    {
        List<GameObject> alivePlayers = new();

        foreach (GameObject player in players)
        {
            if (player == null) continue;

            if (!player.activeInHierarchy) continue;

            if (eliminatedPlayers.Contains(player)) continue;

            alivePlayers.Add(player);
        }
        return alivePlayers;
    }
}
