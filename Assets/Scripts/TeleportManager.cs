using System.Collections.Generic;
using UnityEngine;

public class TeleportManager : MonoBehaviour
{
    [Header("Spawn Points")]
    [Tooltip("All possible teleport destination points.")]
    public Transform[] spawnPoints;

    [Header("Teleport Zones")]
    [Tooltip("All teleport zones in the scene.")]
    public TeleportZone[] teleportZones;

    private void Start()
    {
        AssignUniqueSpawnPoints();
    }

    private void AssignUniqueSpawnPoints()
    {
        // Make sure there are teleport zones assigned
        if (teleportZones == null || teleportZones.Length == 0)
        {
            Debug.LogWarning("No teleport zones have been assigned.");
            return;
        }

        // Make sure there are spawn points assigned
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points have been assigned.");
            return;
        }

        // Make sure there are enough spawn points for all teleport zones
        if (spawnPoints.Length < teleportZones.Length)
        {
            Debug.LogWarning("There are not enough spawn points for every teleport zone to have a unique spawn point.");
            return;
        }

        // Create a list so we can remove spawn points after they are used
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        // Give each teleport zone a unique random spawn point
        for (int i = 0; i < teleportZones.Length; i++)
        {
            int randomIndex = Random.Range(0, availableSpawnPoints.Count);

            Transform chosenSpawnPoint = availableSpawnPoints[randomIndex];

            teleportZones[i].SetSpawnPoint(chosenSpawnPoint);

            // Remove this spawn point so another teleport zone cannot use it
            availableSpawnPoints.RemoveAt(randomIndex);
        }
    }
}