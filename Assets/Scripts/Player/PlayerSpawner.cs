using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints; // assign in Inspector per scene

    [SerializeField] Color[] colors = {
    new Color(0.76f, 0.27f, 0.27f), // muted red
    new Color(0.27f, 0.47f, 0.76f), // muted blue
    new Color(0.31f, 0.65f, 0.42f), // muted green
    new Color(0.85f, 0.70f, 0.30f), // muted gold/yellow
    };

    private void Start()
    {
        SpawnAllPlayers();
    }

    private void SpawnAllPlayers()
    {
        for (int i = 0; i < GameData.players.Count; i++)
        {
            var data = GameData.players[i];

            Vector3 spawnPos = spawnPoints != null && i < spawnPoints.Length
                ? spawnPoints[i].position
                : new Vector3(i * 2, 0, 0);

            PlayerInput playerInput = PlayerInput.Instantiate(
                playerPrefab,
                playerIndex: data.playerIndex,
                controlScheme: null,
                splitScreenIndex: GameData.useSplitScreen ? data.playerIndex : -1,
                pairWithDevice: InputSystem.devices[data.playerIndex]
            );

            playerInput.transform.position = spawnPos;

            // Apply color
            var sr = playerInput.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.color = colors[data.colorIndex];

            // If not split screen, disable the player's child camera and canvas
            // so the shared Phase2Camera takes over
            if (!GameData.useSplitScreen)
            {
                var playerCam = playerInput.GetComponentInChildren<Camera>();
                if (playerCam != null)
                    playerCam.gameObject.SetActive(false);
            }

            var controller = playerInput.GetComponent<PlayerController>();
            if (controller != null)
                controller.Init(data);
        }
    }
}