// =============================================
// PlayerJoinManager.cs
// =============================================

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerJoinManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform cardContainer;
    public bool useSplitScreen = true;
    [SerializeField] string scene = "Phase1";
    private List<PlayerCard> playerCards = new List<PlayerCard>();
    private HashSet<int> joinedDeviceIds = new HashSet<int>();
    private bool gameStarting = false;

    private void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
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

        foreach (var card in playerCards)
            if (!card.isReady) return;

        gameStarting = true;
        StartGame();
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        int deviceId = player.devices[0].deviceId;

        Debug.Log($"OnPlayerJoined — deviceId: {deviceId}, already joined: {joinedDeviceIds.Contains(deviceId)}, total cards: {playerCards.Count}");

        if (joinedDeviceIds.Contains(deviceId))
        {
            Destroy(player.gameObject);
            return;
        }

        joinedDeviceIds.Add(deviceId);

        GameObject cardGO = Instantiate(cardPrefab, cardContainer);
        PlayerCard card = cardGO.GetComponent<PlayerCard>();
        card.SetPlayer(player);
        playerCards.Add(card);
    }

    private void StartGame()
    {
        GameData.players.Clear();
        GameData.useSplitScreen = useSplitScreen; 

        foreach (var card in playerCards)
        {
            GameData.players.Add(new PlayerData
            {
                playerIndex = card.GetPlayer().playerIndex,
                colorIndex = card.GetColorIndex()
            });
        }

        SceneManager.LoadScene(scene);
    }
}