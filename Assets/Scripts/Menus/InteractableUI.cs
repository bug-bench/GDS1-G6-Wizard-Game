using UnityEngine;

public class InteractableUI : MonoBehaviour
{
    public GameObject interactPrompt;  // the world space canvas
    public float interactRadius = 2f;

    void Update()
    {
        bool playerNearby = false;

        foreach (var player in GameData.players)
        {
            if (player.playerGameObject == null) continue;
            float dist = Vector2.Distance(
                transform.position,
                player.playerGameObject.transform.position
            );
            if (dist <= interactRadius)
            {
                playerNearby = true;
                break;
            }
        }

        if (interactPrompt != null)
            interactPrompt.SetActive(playerNearby);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}