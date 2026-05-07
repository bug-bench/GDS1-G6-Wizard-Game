using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;

public class PlayerVoteController : MonoBehaviour
{
    private int playerIndex;
    private int currentIndex;
    private bool isLocked;

    private VotingManager manager;
    private InputAction moveAction;
    private InputAction submitAction;
    private PlayerInput playerInput;

    private float cooldown = 0.2f;
    private float lastMove;

    public GameObject readyIndicator;

    public void Init(int index, int colorIndex, VotingManager mgr)
    {
        playerIndex  = index;
        manager      = mgr;
        currentIndex = 0;
        isLocked     = false;

        playerInput = null;
        foreach (var pi in PlayerInput.all)
        {
            if (pi.devices.Contains(GameData.players[index].device))
            {
                playerInput = pi;
                break;
            }
        }

        if (playerInput == null)
        {
            Debug.LogError($"No PlayerInput found for player {index}");
            return;
        }

        // Switch to UI map — Submit and Navigate are bound here
        playerInput.SwitchCurrentActionMap("UI");
        Debug.Log($"Player {index} switched to action map: {playerInput.currentActionMap?.name}");

        moveAction   = playerInput.actions.FindAction("Navigate", throwIfNotFound: false);
        submitAction = playerInput.actions.FindAction("Submit",   throwIfNotFound: false);

        if (moveAction   == null) Debug.LogError($"Player {index}: Navigate action not found");
        if (submitAction == null) Debug.LogError($"Player {index}: Submit action not found");

        if (moveAction != null)
        {
            moveAction.performed += OnMove;
            moveAction.started   += OnMove;
        }
        if (submitAction != null) submitAction.performed += OnSubmit;

        var img = GetComponent<Image>();
        if (img != null) img.color = GetColor(colorIndex);

        if (readyIndicator != null) readyIndicator.SetActive(false);

        MoveTo(0);
    }

    void MoveTo(int newIndex)
    {
        if (isLocked) return;
        if (manager.options.Count == 0) return;

        newIndex     = Mathf.Clamp(newIndex, 0, manager.options.Count - 1);
        currentIndex = newIndex;

        transform.SetParent(manager.options[currentIndex].markerContainer, false);
        transform.localPosition = Vector3.zero;

        Debug.Log($"Player {playerIndex} hovering option {currentIndex}");
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        if (isLocked) return;
        if (Time.time - lastMove < cooldown) return;

        Vector2 move = ctx.ReadValue<Vector2>();
        if      (move.x >  0.5f) MoveTo(currentIndex + 1);
        else if (move.x < -0.5f) MoveTo(currentIndex - 1);

        lastMove = Time.time;
    }

    void OnSubmit(InputAction.CallbackContext ctx)
    {
        Debug.Log($"Player {playerIndex} OnSubmit fired, locked: {isLocked}");
        if (isLocked) return;
        isLocked = true;

        Debug.Log($"Player {playerIndex} locked in option {currentIndex}");
        manager.RegisterVote(playerIndex, currentIndex);

        var img = GetComponent<Image>();
        if (img != null) img.color = img.color * new Color(1, 1, 1, 0.4f);

        if (readyIndicator != null) readyIndicator.SetActive(true);
    }

    void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMove;
            moveAction.started   -= OnMove;
        }
        if (submitAction != null) submitAction.performed -= OnSubmit;

        // Switch back to Player map when voting scene ends
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");
    }

    Color GetColor(int i)
    {
        Color[] colors = {
            new Color(0.76f, 0.27f, 0.27f),
            new Color(0.27f, 0.47f, 0.76f),
            new Color(0.31f, 0.65f, 0.42f),
            new Color(0.85f, 0.70f, 0.30f),
        };
        return (i >= 0 && i < colors.Length) ? colors[i] : Color.white;
    }
}