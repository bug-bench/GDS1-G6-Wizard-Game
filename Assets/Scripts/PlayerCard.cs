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

    private int colorIndex = 0;
    [SerializeField] Color[] colors = {
    new Color(0.76f, 0.27f, 0.27f), // muted red
    new Color(0.27f, 0.47f, 0.76f), // muted blue
    new Color(0.31f, 0.65f, 0.42f), // muted green
    new Color(0.85f, 0.70f, 0.30f), // muted gold/yellow
    };

    public bool isReady = false;
    private float inputCooldown = 0.2f;
    private float lastInputTime = 0f;

    public void SetPlayer(PlayerInput input)
    {
        player = input;
        player.SwitchCurrentActionMap("Gameplay");
        
        var actionMap = player.actions.FindActionMap("Gameplay", throwIfNotFound: true);
        moveAction = actionMap.FindAction("Move", throwIfNotFound: true);
        submitAction = actionMap.FindAction("Submit", throwIfNotFound: true);
        
        Debug.Log($"Player {player.playerIndex} — moveAction: {moveAction?.name}, enabled: {moveAction?.enabled}");

        readyText.SetActive(false);
        UpdateColor();
    }

    public PlayerInput GetPlayer() => player;

    private void Update()
    {
        if (moveAction == null || submitAction == null)
        {
            Debug.Log($"Actions null on card");
            return;
        }
        
        // log only when input detected
        var move = moveAction.ReadValue<Vector2>();
        if (move.magnitude > 0.1f)
            Debug.Log($"Move: {move}");
        if (submitAction.triggered)
            Debug.Log($"Submit triggered");
            
        HandleColorChange();
        HandleReady();
    }

    private void HandleColorChange()
    {
        if (isReady) return;
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