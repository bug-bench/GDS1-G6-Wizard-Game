using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoinManager : MonoBehaviour
{
    public GameObject cardPrefab;     // assign in Inspector
    public Transform cardContainer;   // assign in Inspector

    private void Start()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;

        PlayerInputManager.instance.onPlayerJoined += (player) =>
        {
            Debug.Log("Player joined: " + player.playerIndex);
        };
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
    }
}