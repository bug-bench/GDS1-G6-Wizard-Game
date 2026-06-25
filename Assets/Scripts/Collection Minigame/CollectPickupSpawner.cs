using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CollectPickupSpawner : MonoBehaviour
{
    public GameObject pickup = null;
    public Tilemap groundTilemap;


    public Vector2 Spawncenter;
    public Vector2 SpawnSize = new Vector2(10F, 10F);

    public int startingPickups = 10;

    public float respawnDelay = 3f;
    private bool spawning = true;

    //spawn distance so no overlay
    public float DistancebetweenStats = 1f;
    public LayerMask statLayer;
    public int maxattempts = 20;
    void Start()
    {
        for (int i = 0; i < startingPickups; i++)
        {
            SpawnSinglePickup();
        }
        StartCoroutine(SpawnOverTime());
    }

    IEnumerator SpawnOverTime()
    {
        while (spawning)
        {
            yield return new WaitForSeconds(respawnDelay);
            SpawnSinglePickup();
        }
    }

    public void StopSpawning()
    {
        spawning = false;
    }
    
    void SpawnSinglePickup()
    {
        Vector2 randomPosition = Vector2.zero;
        bool foundValidPosition = false;
        for (int i = 0; i < maxattempts; i++)
        {
             randomPosition = new Vector2(
                    Random.Range(Spawncenter.x - SpawnSize.x / 2f, Spawncenter.x + SpawnSize.x / 2f),
                    Random.Range(Spawncenter.y - SpawnSize.y / 2f, Spawncenter.y + SpawnSize.y / 2f)
                );

            // convert to tile position
            Vector3Int cellPos = groundTilemap.WorldToCell(randomPosition);

            // check if tile exists
            bool hasTile = groundTilemap.HasTile(cellPos);

            Collider2D hit = Physics2D.OverlapCircle(randomPosition, DistancebetweenStats, statLayer);

            if(hit == null && hasTile)
            {
                foundValidPosition = true;
                break;
            }

        }
        
        if (!foundValidPosition)
        {
            return;
        }

        if (pickup == null)
        {
            Debug.LogWarning($"StatSpawner: The prefab for Pickup is missing! Please assign it in the Inspector.");
            return;
        }

        GameObject newPickup = Instantiate(pickup, randomPosition, Quaternion.identity);

        CollectPickup collectPickup = newPickup.GetComponent<CollectPickup>();
        if (collectPickup != null)
        {
            collectPickup.SetSpawner(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Spawncenter, SpawnSize);
    }
}
