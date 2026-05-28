// =============================================
// PlayerJoinManager.cs
// =============================================

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerJoinManager : MonoBehaviour
{
    [SerializeField] int maxPlayers = 4;
    [SerializeField] int minPlayers = 2;

    public GameObject cardPrefab;
    public GameObject emptyCardPrefab;
    public Transform cardContainer;
    public bool useSplitScreen = true;
    [SerializeField] string scene = "Phase1";
    public List<PlayerCard> playerCards = new List<PlayerCard>();
    private List<EmptyCardUI> emptyCards = new List<EmptyCardUI>();
    private HashSet<int> joinedDeviceIds = new HashSet<int>();
    private bool gameStarting = false;

    private void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        for (int i = 0; i < minPlayers; i++)
            SpawnEmptyCard();
    }

    private void SpawnEmptyCard()
    {
        if (emptyCards.Count + playerCards.Count >= maxPlayers) return;

        GameObject empty = Instantiate(emptyCardPrefab, cardContainer);
        EmptyCardUI ui   = empty.GetComponent<EmptyCardUI>();
        if (ui != null) emptyCards.Add(ui);
    }

    private void OnDestroy()
    {
        if (PlayerInputManager.instance != null)
            PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
    }

    private void Update()
    {
        if (gameStarting) return;
        if (playerCards.Count == 0) return;
        if (playerCards.Count < minPlayers) return; 

        foreach (var card in playerCards)
            if (!card.isReady) return;

        gameStarting = true;
        StartGame();
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        int deviceId = player.devices[0].deviceId;

        if (joinedDeviceIds.Contains(deviceId))
        {
            Destroy(player.gameObject);
            return;
        }

        if (playerCards.Count >= maxPlayers)
        {
            Destroy(player.gameObject);
            return;
        }

        joinedDeviceIds.Add(deviceId);

        // Hide first empty card
        if (emptyCards.Count > 0)
        {
            EmptyCardUI firstEmpty = emptyCards[0];
            emptyCards.RemoveAt(0);
            firstEmpty.Hide();
        }

        // Spawn real card
        GameObject cardGO = Instantiate(cardPrefab, cardContainer);
        cardGO.transform.SetSiblingIndex(playerCards.Count);
        PlayerCard card = cardGO.GetComponent<PlayerCard>();
        card.SetPlayer(player);
        playerCards.Add(card);

        // Only spawn new empty card if we have room AND
        // current total (real + empty) is less than maxPlayers
        if (playerCards.Count >= minPlayers)
        {
            int totalSlots = playerCards.Count + emptyCards.Count;
            if (totalSlots < maxPlayers)
                SpawnEmptyCard();
        }
    }

    private void StartGame()
    {
        GameData.players.Clear();
        GameData.useSplitScreen = useSplitScreen;

        foreach (var card in playerCards)
        {
            var player = card.GetPlayer();
            GameData.players.Add(new PlayerData
            {
                playerIndex = player.playerIndex,
                colorIndex = card.GetColorIndex(),
                device = player.devices[0] // save actual device reference
            });
        }

        SceneManager.LoadScene(scene);
    }
}