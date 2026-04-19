using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    [SerializeField] private Color[] colors = {
        UseHexColor.HexColor("C2453A"),
        UseHexColor.HexColor("3A6FBF"),
        UseHexColor.HexColor("3DA65A"),
        UseHexColor.HexColor("D4A83A"),
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

            if (data.device == null)
            {
                Debug.LogWarning($"Player {data.playerIndex} has no saved device — input may not work.");
                continue;
            }

            PlayerInput playerInput = PlayerInput.Instantiate(
                playerPrefab,
                playerIndex: data.playerIndex,
                controlScheme: null,
                splitScreenIndex: GameData.useSplitScreen ? data.playerIndex : -1,
                pairWithDevice: data.device
            );

            playerInput.transform.position = spawnPos;

            var sr = playerInput.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) {
                sr.color = colors[data.colorIndex];
                data.playerSprite = sr.sprite; // save sprite 
            }

            // 更新战斗脚本里的原始颜色，防止闪烁后变回白色
            var combat = playerInput.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                combat.UpdateOriginalBlinkColors();
            }

            var go = playerInput.gameObject;
            if (go != null)
            {
                GameData.players[i].playerGameObject = go;
            }

            if (!GameData.useSplitScreen)
            {
                var playerCam = playerInput.GetComponentInChildren<Camera>();
                if (playerCam != null)
                    playerCam.gameObject.SetActive(false);
            }

            var controller = playerInput.GetComponent<PlayerController>();
            if (controller != null)
                controller.Init(data);

            // ADD THIS
            Phase2StatCard card = playerInput.GetComponentInChildren<Phase2StatCard>();
            if (card != null)
            {
                var stats = playerInput.GetComponent<PlayerStats>();
                card.Init(stats, data);
            }
            else
            {
                Debug.LogWarning($"Player {data.playerIndex} has no Phase2StatCard in children");
            }
        }
    }
}