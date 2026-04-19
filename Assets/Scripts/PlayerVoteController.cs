using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVoteController : MonoBehaviour
{
    private int playerIndex;
    private int currentIndex;
    private bool isLocked;

    private VotingManager manager;
    private InputAction moveAction;
    private InputAction submitAction;

    private float cooldown = 0.2f;
    private float lastMove;

    public void Init(int index, int colorIndex, VotingManager mgr)
    {
        playerIndex = index;
        manager = mgr;
        currentIndex = 0;
        isLocked = false;

        PlayerInput input = PlayerInput.all[index];

        moveAction = input.actions.FindAction("Move", throwIfNotFound: false);
        submitAction = input.actions.FindAction("Submit", throwIfNotFound: false);

        // Use callbacks instead of polling
        if (moveAction != null) moveAction.performed += OnMove;
        if (submitAction != null) submitAction.performed += OnSubmit;

        GetComponent<UnityEngine.UI.Image>().color = GetColor(colorIndex);
        MoveTo(0);
    }

    void MoveTo(int newIndex)
    {
        if (manager.options.Count == 0) return;
        newIndex = Mathf.Clamp(newIndex, 0, manager.options.Count - 1);
        currentIndex = newIndex;
        transform.SetParent(manager.options[currentIndex].markerContainer, false);
        transform.localPosition = Vector3.zero;
        // removed auto-register from here
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        if (Time.time - lastMove < cooldown) return;
        Vector2 move = ctx.ReadValue<Vector2>();
        if (move.x > 0.5f) MoveTo(currentIndex + 1);
        else if (move.x < -0.5f) MoveTo(currentIndex - 1);
        lastMove = Time.time;
        // register after moving
        manager.RegisterVote(playerIndex, currentIndex);
    }

    void OnSubmit(InputAction.CallbackContext ctx)
    {
        isLocked = true;
        manager.RegisterVote(playerIndex, currentIndex);
        Debug.Log($"Player {playerIndex} locked in vote for option {currentIndex}");
    }

    void OnDestroy()
    {
        if (moveAction != null) moveAction.performed -= OnMove;
        if (submitAction != null) submitAction.performed -= OnSubmit;
    }

    Color GetColor(int i)
    {
        Color[] colors = {
            new Color(0.76f, 0.27f, 0.27f),
            new Color(0.27f, 0.47f, 0.76f),
            new Color(0.31f, 0.65f, 0.42f),
            new Color(0.85f, 0.70f, 0.30f),
        };
        if (i >= 0 && i < colors.Length) return colors[i];
        return Color.white;
    }
}