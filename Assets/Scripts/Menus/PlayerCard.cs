using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerCard : MonoBehaviour
{
    [Header("UI References")]
    public Image characterImage;
    public Image headwearImage;
    public Image bodywearImage;
    public TMPro.TextMeshProUGUI characterNameText;
    public CustomizationOverlay customizationOverlay;
    public GameObject readyText;

    [Header("Customization Data")]
    public CharacterDefinition[] availableCharacters;
    public Color[] clothingColors = {
        new Color(0.76f, 0.27f, 0.27f),
        new Color(0.27f, 0.47f, 0.76f),
        new Color(0.31f, 0.65f, 0.42f),
        new Color(0.85f, 0.70f, 0.30f),
    };

    // Current selections
    private int characterIndex = 0;
    private int skinIndex      = 0;
    private int colorIndex     = 0;

    // State
    public bool isReady        = false;
    private bool isCustomizing = false;

    // Input
    private PlayerInput player;
    private InputAction moveAction;
    private InputAction submitAction;
    private InputAction customizeAction; // the "Option" button

    private float joinTime      = 0f;
    private float inputCooldown = 0.2f;
    private float lastInputTime = 0f;

    // ----------------------------------------------------------------
    // Existing action map resolver — unchanged
    // ----------------------------------------------------------------
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
            if (m.FindAction("Move", throwIfNotFound: false) != null)
                return m;

        return null;
    }

    static InputAction ResolveReadyAction(InputActionMap map)
    {
        if (map == null) return null;
        return map.FindAction("Join",   throwIfNotFound: false)
               ?? map.FindAction("Submit", throwIfNotFound: false);
    }

    // ----------------------------------------------------------------
    // Setup
    // ----------------------------------------------------------------
    public void SetPlayer(PlayerInput input)
    {
        player   = input;
        joinTime = Time.time;

        InputActionMap actionMap = ResolveLobbyActionMap(player);
        if (actionMap == null)
        {
            Debug.LogError($"PlayerCard: No action map with Move found in '{player.actions?.name}'.");
            return;
        }

        player.SwitchCurrentActionMap(actionMap.name);

        moveAction      = actionMap.FindAction("Move",     throwIfNotFound: false);
        submitAction    = ResolveReadyAction(actionMap);
        customizeAction = actionMap.FindAction("Customize", throwIfNotFound: false)
                          ?? actionMap.FindAction("Cancel",    throwIfNotFound: false)
                          ?? actionMap.FindAction("Pause",     throwIfNotFound: false);

        if (moveAction      == null) Debug.LogError($"PlayerCard: Map '{actionMap.name}' has no Move action.");
        if (submitAction    == null) Debug.LogWarning($"PlayerCard: Map '{actionMap.name}' has no Join/Submit.");
        if (customizeAction == null) Debug.LogWarning($"PlayerCard: No Customize/Cancel/Pause action found — customization toggle unavailable.");

        readyText.SetActive(false);
        RefreshCharacterImage();
    }

    public PlayerInput GetPlayer()           => player;
    public int         GetColorIndex()       => colorIndex;
    public CharacterDefinition GetCharacter() => availableCharacters[characterIndex];
    public CharacterSkin       GetSkin()      => availableCharacters[characterIndex].skins[skinIndex];

    // ----------------------------------------------------------------
    // Update
    // ----------------------------------------------------------------
    private void Update()
    {
        if (moveAction == null) return;

        HandleCustomizeToggle();

        if (isCustomizing)
            HandleCustomizationInput();
        else
            HandleLobbyInput();
    }

    // ----------------------------------------------------------------
    // Customize toggle
    // ----------------------------------------------------------------
    private void HandleCustomizeToggle()
    {
        if (customizeAction == null) return;
        if (isReady) return;
        if (PauseMenu.IsPaused) return;
        if (Time.time - joinTime < 0.5f) return;

        if (customizeAction.triggered)
        {
            isCustomizing = !isCustomizing;

            if (isCustomizing)
                customizationOverlay.Show();
            else
                customizationOverlay.Hide();
        }
    }

    // ----------------------------------------------------------------
    // Input while customization overlay is open
    // ----------------------------------------------------------------
    private void HandleCustomizationInput()
    {
        if (PauseMenu.IsPaused) return;
        if (Time.time - lastInputTime < inputCooldown) return;

        var move = moveAction.ReadValue<Vector2>();

        // Up/down — move between rows
        if (move.y > 0.5f)
        {
            customizationOverlay.MoveRow(-1);
            lastInputTime = Time.time;
        }
        else if (move.y < -0.5f)
        {
            customizationOverlay.MoveRow(1);
            lastInputTime = Time.time;
        }
        // Left/right — cycle the selected option
        else if (move.x > 0.5f)
        {
            CycleSelectedRow(1);
            lastInputTime = Time.time;
        }
        else if (move.x < -0.5f)
        {
            CycleSelectedRow(-1);
            lastInputTime = Time.time;
        }
    }

    private void CycleSelectedRow(int direction)
    {
        switch (customizationOverlay.GetSelectedRow())
        {
            case 0: CycleCharacter(direction); break;
            case 1: CycleColor(direction);     break;
            case 2: CycleSkin(direction);      break;
        }
    }

    // ----------------------------------------------------------------
    // Input while in normal lobby mode (your existing logic)
    // ----------------------------------------------------------------
    private void HandleLobbyInput()
    {
        HandleReady();
    }

    private void HandleReady()
    {
        if (submitAction    == null) return;
        if (PauseMenu.IsPaused) return;
        if (Time.time - joinTime < 0.5f) return;

        if (submitAction.triggered)
        {
            isReady = !isReady;
            readyText.SetActive(isReady);
        }
    }

    // ----------------------------------------------------------------
    // Cycling logic
    // ----------------------------------------------------------------
    private void CycleCharacter(int direction)
    {
        characterIndex = (characterIndex + direction + availableCharacters.Length) % availableCharacters.Length;
        skinIndex = 0; // reset skin when character changes
        RefreshCharacterImage();
    }

    private void CycleSkin(int direction)
    {
        var skins = availableCharacters[characterIndex].skins;
        skinIndex = (skinIndex + direction + skins.Length) % skins.Length;
        RefreshCharacterImage();
    }

    private void CycleColor(int direction)
    {
        colorIndex = GetNextAvailableColor(colorIndex, direction);
        RefreshCharacterImage();
    }

    private int GetNextAvailableColor(int current, int direction)
    {
        HashSet<int> takenColors = new HashSet<int>();
        var manager = FindFirstObjectByType<PlayerJoinManager>();
        if (manager != null)
            foreach (var card in manager.playerCards)
                if (card != this) takenColors.Add(card.GetColorIndex());

        int next = current;
        for (int i = 0; i < clothingColors.Length; i++)
        {
            next = (next + direction + clothingColors.Length) % clothingColors.Length;
            if (!takenColors.Contains(next)) return next;
        }
        return current;
    }

    // ----------------------------------------------------------------
    // Visual refresh
    // ----------------------------------------------------------------
    private void RefreshCharacterImage()
    {
        var skin = availableCharacters[characterIndex].skins[skinIndex];

        // Pull the first sprite from the body controller's default state
        if (skin.bodyController != null)
        {
            var clips = skin.bodyController.animationClips;
            if (clips.Length > 0)
            {
                // Get first frame of first clip
                var bindings = UnityEditor.AnimationUtility.GetObjectReferenceCurveBindings(clips[0]);
                if (bindings.Length > 0)
                {
                    var frames = UnityEditor.AnimationUtility.GetObjectReferenceCurve(clips[0], bindings[0]);
                    if (frames.Length > 0)
                        characterImage.sprite = frames[0].value as Sprite;
                }
            }
        }

        characterImage.color = Color.white;

        Color clothingColor = skin.usesColorTint ? clothingColors[colorIndex] : Color.white;
        if (headwearImage != null) headwearImage.color = clothingColor;
        if (bodywearImage != null) bodywearImage.color = clothingColor;
        if (characterNameText != null) characterNameText.text = availableCharacters[characterIndex].displayName;
    }
}