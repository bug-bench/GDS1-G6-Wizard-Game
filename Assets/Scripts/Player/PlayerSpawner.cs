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
        if (GameData.players.Count == 0 || GameData.players[0].playerGameObject == null)
        {
            SpawnAllPlayers();
        }
        else
        {
            SpawnExistingPlayers();
        }
    }

    private Vector3 GetRandomSpawnPosition(int fallbackIndex)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            return spawnPoints[randomIndex].position;
        }

        return new Vector3(fallbackIndex * 2, 0, 0);
    }

    private void SpawnAllPlayers()
    {
        for (int i = 0; i < GameData.players.Count; i++)
        {
            var data = GameData.players[i];

            Vector3 spawnPos = GetRandomSpawnPosition(i);

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
            if (sr != null)
            {
                sr.color = colors[data.colorIndex];
                data.playerSprite = sr.sprite; // Save Sprite
            }

             // 更新战斗脚本里的原始颜色，防止闪烁后变回白色
             // Update the original colors in the battle script to prevent them from reverting to white after flickering.
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

        Debug.Log($"PlayerSpawner — useSplitScreen: {GameData.useSplitScreen}");
    }

    private void SpawnExistingPlayers()
    {
        for (int i = 0; i < GameData.players.Count; i++)
        {
            var data = GameData.players[i];

            if (data.playerGameObject == null) continue;

            Vector3 spawnPos = GetRandomSpawnPosition(i);

            data.playerGameObject.transform.position = spawnPos;
            data.playerGameObject.SetActive(true);
        }
    }
}