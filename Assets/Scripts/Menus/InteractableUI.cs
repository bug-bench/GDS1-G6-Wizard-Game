using UnityEngine;

public class InteractableUI : MonoBehaviour
{
    public GameObject interactPrompt;
    public float interactRadius = 2f;

    private SpellSwapIcon swapIcon;
    private SpellPickupHUDReceiver currentReceiver;

    void Awake()
    {
        swapIcon = GetComponentInChildren<SpellSwapIcon>();
        if (swapIcon != null)
            swapIcon.onSwap += OnIconSwapped;
    }

    void OnDestroy()
    {
        if (swapIcon != null)
            swapIcon.onSwap -= OnIconSwapped;
    }

    void Update()
    {
        float closestDist  = float.MaxValue;
        SpellPickupHUDReceiver closestReceiver = null;

        foreach (var player in GameData.players)
        {
            if (player.playerGameObject == null) continue;
            float dist = Vector2.Distance(
                transform.position,
                player.playerGameObject.transform.position
            );
            if (dist < closestDist)
            {
                closestDist    = dist;
                closestReceiver = player.playerGameObject
                    .GetComponentInChildren<SpellPickupHUDReceiver>();
            }
        }

        bool anyNearby = closestDist <= interactRadius;

        if (anyNearby)
        {
            if (interactPrompt != null) interactPrompt.SetActive(true);

            // Switch receiver if player changed
            if (closestReceiver != currentReceiver)
            {
                currentReceiver?.HidePrompt();
                currentReceiver = closestReceiver;
            }

            // Sync highlight with current icon state
            currentReceiver?.ShowSlotHighlight(swapIcon != null && swapIcon.IsShowingA());
        }
        else
        {
            if (interactPrompt != null) interactPrompt.SetActive(false);
            currentReceiver?.HidePrompt();
            currentReceiver = null;
        }
    }

    void OnIconSwapped(bool showingA)
    {
        currentReceiver?.ShowSlotHighlight(showingA);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}