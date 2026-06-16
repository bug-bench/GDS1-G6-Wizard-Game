using System.Collections;
using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("The tag used to identify players.")]
    public string playerTag = "Player";

    [Tooltip("Prevents repeated teleporting too quickly.")]
    public float teleportCooldown = 0.5f;

    private Transform assignedSpawnPoint;

    private bool canTeleport = true;

    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        assignedSpawnPoint = newSpawnPoint;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering the zone is a player
        if (other.CompareTag(playerTag) && canTeleport)
        {
            TeleportPlayer(other.transform);
        }
    }

    private void TeleportPlayer(Transform player)
    {
        // Make sure this teleport zone has been given a spawn point
        if (assignedSpawnPoint == null)
        {
            Debug.LogWarning(gameObject.name + " has no assigned spawn point.");
            return;
        }

        // Move the player to the assigned spawn point
        player.position = assignedSpawnPoint.position;

        // Start cooldown so this teleport zone cannot instantly teleport again
        StartCoroutine(TeleportCooldown());
    }

    private IEnumerator TeleportCooldown()
    {
        canTeleport = false;

        yield return new WaitForSeconds(teleportCooldown);

        canTeleport = true;
    }
}