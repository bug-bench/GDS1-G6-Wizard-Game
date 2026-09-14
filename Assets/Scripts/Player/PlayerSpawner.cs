using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;
    public GameObject backgroundPrefab;

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

            Debug.Log($"Player {data.playerIndex} — character: {data.character?.characterId}, skin: {data.skin?.skinName}");

            playerInput.transform.position = spawnPos;

            var characterLayerSync = playerInput.GetComponentInChildren<CharacterLayerSync>();
            Debug.Log($"CharacterLayerSync found: {characterLayerSync != null}");

            if (characterLayerSync != null && data.skin != null)
            {
                characterLayerSync.ApplySkin(data.skin);
                foreach (var anim in characterLayerSync.GetComponentsInChildren<Animator>())
                    anim.Rebind();
                Debug.Log($"body: {data.skin.bodyController?.name}, head: {data.skin.headwearController?.name}, body wear: {data.skin.bodywearController?.name}");
            }

            // Color all SpriteRenderers in children
            var renderers = playerInput.GetComponentsInChildren<SpriteRenderer>();
            Color playerColor = colors[data.colorIndex];
            SpriteRenderer bodyRenderer = null;

            foreach (var sr in renderers)
            {
                bool isClothing = sr.CompareTag("Clothing");
                if (isClothing && data.skin != null && data.skin.usesColorTint)
                    sr.color = playerColor;
                else if (!isClothing)
                    sr.color = Color.white;

                // Save the body renderer specifically (not clothing)
                if (!isClothing && bodyRenderer == null)
                    bodyRenderer = sr;
            }

            if (bodyRenderer != null)
            {
                data.playerSprite = bodyRenderer.sprite;
                data.playerSpriteColor = bodyRenderer.color;
            }

            var combat = playerInput.GetComponent<PlayerCombat>();
            if (combat != null)
                combat.UpdateOriginalBlinkColors();

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
        if (GameData.useSplitScreen)
        {
            ApplyCameraLayout();
            SpawnBackgroundForGap();
        }
    }

    void ApplyCameraLayout()
    {
        int count = GameData.players.Count;
        for (int i = 0; i < count; i++)
        {
            var go  = GameData.players[i].playerGameObject;
            if (go == null) continue;
            var cam = go.GetComponentInChildren<Camera>();
            if (cam == null) continue;
            cam.rect = GetCameraRect(i, count);
        }
    }

    void SpawnBackgroundForGap()
    {
        // Only needed for 3 players — 4 players fills the screen, 1/2 have no gap
        if (GameData.players.Count != 3) return;
        if (backgroundPrefab == null) return;

        // Background camera — renders full screen behind all player cameras
        GameObject bgCamObj   = new GameObject("BackgroundCamera");
        Camera bgCam          = bgCamObj.AddComponent<Camera>();
        bgCam.clearFlags      = CameraClearFlags.SolidColor;
        bgCam.backgroundColor = Color.black;
        bgCam.cullingMask     = 0;      // renders nothing from game world
        bgCam.depth           = -10;    // behind all player cameras
        bgCam.rect            = new Rect(0, 0, 1, 1);

        // Spawn background prefab and assign it to the background camera
        GameObject bg    = Instantiate(backgroundPrefab);
        Canvas bgCanvas  = bg.GetComponent<Canvas>();
        if (bgCanvas != null)
        {
            bgCanvas.renderMode   = RenderMode.ScreenSpaceCamera;
            bgCanvas.worldCamera  = bgCam;
            bgCanvas.sortingOrder = -100;
        }
    }

    Rect GetCameraRect(int index, int total)
    {
        switch (total)
        {
            case 1: return new Rect(0, 0, 1, 1);
            case 2: return new Rect(index * 0.5f, 0, 0.5f, 1);
            case 3:
                switch (index)
                {
                    case 0: return new Rect(0,     0.5f, 0.5f, 0.5f);
                    case 1: return new Rect(0.5f,  0.5f, 0.5f, 0.5f);
                    case 2: return new Rect(0.25f, 0f,   0.5f, 0.5f);
                    default: return new Rect(0, 0, 1, 1);
                }
            case 4:
                return new Rect(
                    (index % 2) * 0.5f,
                    index < 2 ? 0.5f : 0f,
                    0.5f, 0.5f
                );
            default: return new Rect(0, 0, 1, 1);
        }
    }

    void SpawnExistingPlayers()
    {
        for (int i = 0; i < GameData.players.Count; i++)
        {
            var data = GameData.players[i];

            if (data.playerGameObject == null) continue;

            Vector3 spawnPos = spawnPoints != null && i < spawnPoints.Length
                ? spawnPoints[i].position
                : new Vector3(i * 2, 0, 0);

            data.playerGameObject.transform.position = spawnPos;
            data.playerGameObject.SetActive(true);
        }
    }
}