using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class PlayerJoinManager : MonoBehaviour
{
    public GameObject cardPrefab;     // assign in Inspector
    public Transform cardContainer;   // assign in Inspector
    public List<PlayerCard> playerCards = new List<PlayerCard>();
    bool gameStarting = false;

    private void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;

        PlayerInputManager.instance.onPlayerJoined += (player) =>
        {
            Debug.Log("Player joined: " + player.playerIndex);
        };
    }

    void Update()
    {
        if (gameStarting) return;
        if (playerCards.Count == 0) return;

        foreach (var card in playerCards)
        {
            if (!card.isReady)
                return;
        }

        gameStarting = true;
        StartGame();
    }

    private void OnDestroy()
    {
        if (PlayerInputManager.instance != null)
            PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        // spawn the card as child of container
        GameObject card = Instantiate(cardPrefab, cardContainer);

        // set the input for the card so it can read left/right/ready
        PlayerCard cardScript = card.GetComponent<PlayerCard>();
        cardScript.SetPlayer(player);

        playerCards.Add(cardScript);
    }

    void StartGame()
    {
        SceneManager.LoadScene("Phase1");
    }
}