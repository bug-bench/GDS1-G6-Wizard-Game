using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StatSpawner : MonoBehaviour
{
    [Header("Stat Prefabs")]
    [SerializeField] private GameObject AttackSprite;
    [SerializeField] private GameObject HealthSprite;
    [SerializeField] private GameObject MovementSprite;
    [SerializeField] private GameObject FocusSprite;
    [SerializeField] private GameObject SizeSprite;
    [SerializeField] private GameObject DefenseSprite;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;

    [Tooltip("Optional")]
    [SerializeField] private Tilemap spawnWeightTilemap;

    [Header("Weight Settings")]
    [SerializeField] private int normalTileWeight = 5;

    [Tooltip("Lower = rarer, higher = more common.")]
    [SerializeField] private int paintedTileWeight = 1;

    [Header("Edge Prevention")]
    [SerializeField] private int edgeBufferTiles = 1;

    [Header("Spawn Settings")]
    [SerializeField] private int numberToSpawn = 30;
    [SerializeField] private float respawnDelay = 3f;

    [Header("Spacing")]
    [SerializeField] private float distanceBetweenStats = 1f;
    [SerializeField] private LayerMask statLayer;
    [SerializeField] private int maxAttempts = 30;

    private GameObject[] statPrefabs;

    private List<WeightedSpawnPoint> validSpawnPoints = new List<WeightedSpawnPoint>();

    private class WeightedSpawnPoint
    {
        public Vector3 worldPosition;
        public int weight;

        public WeightedSpawnPoint(Vector3 worldPosition, int weight)
        {
            this.worldPosition = worldPosition;
            this.weight = weight;
        }
    }

    private void Awake()
    {
        statPrefabs = new GameObject[]
        {
            AttackSprite,
            HealthSprite,
            MovementSprite,
            SizeSprite,
            FocusSprite,
            DefenseSprite
        };
    }

    private void Start()
    {
        CacheValidSpawnTiles();
        SpawnStats();
    }

    private void CacheValidSpawnTiles()
    {
        validSpawnPoints.Clear();

        if (groundTilemap == null)
        {
            Debug.LogError("StatSpawner: Ground Tilemap is missing.");
            return;
        }

        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (Vector3Int cellPosition in bounds.allPositionsWithin)
        {
            if (!groundTilemap.HasTile(cellPosition))
                continue;

            if (IsTooCloseToEdge(cellPosition))
                continue;

            int weight = normalTileWeight;

            if (spawnWeightTilemap != null && spawnWeightTilemap.HasTile(cellPosition))
            {
                weight = paintedTileWeight;
            }

            if (weight <= 0)
                continue;

            Vector3 worldPosition = groundTilemap.GetCellCenterWorld(cellPosition);
            validSpawnPoints.Add(new WeightedSpawnPoint(worldPosition, weight));
        }

        Debug.Log($"StatSpawner: Cached {validSpawnPoints.Count} valid spawn points.");
    }

    private bool IsTooCloseToEdge(Vector3Int cellPosition)
    {
        if (edgeBufferTiles <= 0)
            return false;

        for (int x = -edgeBufferTiles; x <= edgeBufferTiles; x++)
        {
            for (int y = -edgeBufferTiles; y <= edgeBufferTiles; y++)
            {
                Vector3Int checkPosition = new Vector3Int(
                    cellPosition.x + x,
                    cellPosition.y + y,
                    cellPosition.z
                );

                if (!groundTilemap.HasTile(checkPosition))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void SpawnStats()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            SpawnSingleStat();
        }
    }

    private void SpawnSingleStat()
    {
        if (validSpawnPoints.Count == 0)
        {
            Debug.LogWarning("StatSpawner: No valid spawn points found.");
            return;
        }

        Vector3 spawnPosition = Vector3.zero;
        bool foundValidPosition = false;

        for (int i = 0; i < maxAttempts; i++)
        {
            spawnPosition = GetRandomWeightedSpawnPosition();

            Collider2D hit = Physics2D.OverlapCircle(
                spawnPosition,
                distanceBetweenStats,
                statLayer
            );

            if (hit == null)
            {
                foundValidPosition = true;
                break;
            }
        }

        if (!foundValidPosition)
        {
            Debug.LogWarning("StatSpawner: Could not find a free spawn position.");
            return;
        }

        GameObject prefabToSpawn = statPrefabs[Random.Range(0, statPrefabs.Length)];

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("StatSpawner: One of the stat prefabs is missing.");
            return;
        }

        GameObject newStat = Instantiate(
            prefabToSpawn,
            spawnPosition,
            Quaternion.identity
        );

        StatPickUp pickup = newStat.GetComponent<StatPickUp>();

        if (pickup != null)
        {
            pickup.SetSpawner(this);
        }
    }

    private Vector3 GetRandomWeightedSpawnPosition()
    {
        int totalWeight = 0;

        foreach (WeightedSpawnPoint point in validSpawnPoints)
        {
            totalWeight += point.weight;
        }

        int randomWeight = Random.Range(0, totalWeight);

        foreach (WeightedSpawnPoint point in validSpawnPoints)
        {
            randomWeight -= point.weight;

            if (randomWeight < 0)
            {
                return point.worldPosition;
            }
        }

        return validSpawnPoints[0].worldPosition;
    }

    public void RespawnStats()
    {
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnSingleStat();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundTilemap == null)
            return;

        Gizmos.color = Color.green;

        foreach (Vector3Int cellPosition in groundTilemap.cellBounds.allPositionsWithin)
        {
            if (!groundTilemap.HasTile(cellPosition))
                continue;

            if (Application.isPlaying && IsTooCloseToEdge(cellPosition))
                continue;

            Vector3 worldPosition = groundTilemap.GetCellCenterWorld(cellPosition);
            Gizmos.DrawWireSphere(worldPosition, 0.1f);
        }
    }
}