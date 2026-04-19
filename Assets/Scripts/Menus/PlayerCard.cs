using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerCard : MonoBehaviour
{
    [Header("UI References")]
    public Image characterImage;
    public GameObject readyText;

    private PlayerInput player;

    private InputAction moveAction;
    private InputAction submitAction;
    private float joinTime;

    private int colorIndex = 0;
    [SerializeField] Color[] colors = {
    new Color(0.76f, 0.27f, 0.27f), // muted red
    new Color(0.27f, 0.47f, 0.76f), // muted blue
    new Color(0.31f, 0.65f, 0.42f), // muted green
    new Color(0.85f, 0.70f, 0.30f), // muted yellow
    };

    public bool isReady = false;
    private float inputCooldown = 0.2f;
    private float lastInputTime = 0f;

    /// <summary>
    /// 大厅用的 InputAsset 可能是 PlayerControls（地图名 Player）、TestInput（地图名 GamePlay）等，不能写死一个名字。
    /// Lobby may use PlayerControls (map "Player"), TestInput (map "GamePlay"), etc.; do not hard-code one map name.
    /// </summary>
    static InputActionMap ResolveLobbyActionMap(PlayerInput input)
    {
        if (input == null || input.actions == null) return null;

        InputActionAsset asset = input.actions;

        if (!string.IsNullOrEmpty(input.defaultActionMap))
        {
            InputActionMap dm = asset.FindActionMap(input.defaultActionMap, throwIfNotFound: false);
            if (dm != null && dm.FindAction("Move", throwIfNotFound: false) != null)
                return dm;
        }

        foreach (string name in new[] { "Player", "Gameplay", "GamePlay" })
        {
            InputActionMap m = asset.FindActionMap(name, throwIfNotFound: false);
            if (m != null && m.FindAction("Move", throwIfNotFound: false) != null)
                return m;
        }

        foreach (InputActionMap m in asset.actionMaps)
        {
            if (m.FindAction("Move", throwIfNotFound: false) != null)
                return m;
        }

        return null;
    }

    static InputAction ResolveReadyAction(InputActionMap map)
    {
        if (map == null) return null;
        return map.FindAction("Join", throwIfNotFound: false)
               ?? map.FindAction("Submit", throwIfNotFound: false);
    }

    public void SetPlayer(PlayerInput input)
    {
        player = input;
        joinTime = Time.time;

        InputActionMap actionMap = ResolveLobbyActionMap(player);
        if (actionMap == null)
        {
            Debug.LogError($"PlayerCard: 在「{player.actions?.name}」中找不到含 Move 的 Action Map，请检查 Input Actions 资源。 | No action map with Move in '{player.actions?.name}'. Check Input Actions asset.");
            return;
        }

        player.SwitchCurrentActionMap(actionMap.name);

        moveAction = actionMap.FindAction("Move", throwIfNotFound: false);
        submitAction = ResolveReadyAction(actionMap);

        if (moveAction == null)
            Debug.LogError($"PlayerCard: 地图「{actionMap.name}」缺少 Move。 | Map '{actionMap.name}' has no Move action.");
        if (submitAction == null)
            Debug.LogWarning($"PlayerCard: 地图「{actionMap.name}」缺少 Join 与 Submit，无法切换 Ready。 | Map '{actionMap.name}' has no Join/Submit; Ready toggle unavailable.");

        Debug.Log($"Player {player.playerIndex} — map: {actionMap.name}, move: {moveAction != null}, ready: {submitAction != null}");

        readyText.SetActive(false);
        UpdateColor();
    }

    public PlayerInput GetPlayer() => player;

    private void Update()
    {
        if (moveAction == null || submitAction == null)
            return;

        HandleColorChange();
        HandleReady();
    }

    private void HandleColorChange()
    {
        if (isReady) return;
        if (PauseMenu.IsPaused) return;
        if (Time.time - lastInputTime < inputCooldown) return;

        var move = moveAction.ReadValue<Vector2>();

        if (move.x > 0.5f)
        {
            colorIndex = (colorIndex + 1) % colors.Length;
            UpdateColor();
            lastInputTime = Time.time;
        }
        else if (move.x < -0.5f)
        {
            colorIndex = (colorIndex - 1 + colors.Length) % colors.Length;
            UpdateColor();
            lastInputTime = Time.time;
        }
    }

    private void HandleReady()
    {
        // Ignore input for 0.5s after joining to prevent players instantly readying up
        if (Time.time - joinTime < 0.5f) return;
        if (PauseMenu.IsPaused) return;
        if (submitAction.triggered)
        {
            isReady = !isReady;
            readyText.SetActive(isReady);
        }
    }
    private void UpdateColor()
    {
        characterImage.color = colors[colorIndex];
    }

    public int GetColorIndex() => colorIndex;
}
