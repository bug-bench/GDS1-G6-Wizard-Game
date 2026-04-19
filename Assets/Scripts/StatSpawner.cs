using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StatSpawner : MonoBehaviour
{
    public GameObject AttackSprite;
    public GameObject HealthSprite;
    public GameObject MovementSprite;
    //public GameObject Spell;
    public Tilemap groundTilemap;


    public Vector2 Spawncenter;
    public Vector2 SpawnSize = new Vector2(10F, 10F);

    public int numberToSpawn = 30;

    public float respawnDelay = 3f;

    //spawn distance so no overlay
    public float DistancebetweenStats = 1f;
    public LayerMask statLayer;
    public int maxattempts = 20;
    void Start()
    {
        SpawnStats();
    }


    void SpawnStats()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            SpawnSingleStats();
        }
    }
    void SpawnSingleStats()
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

        int randomStat = Random.Range(0, 4);

        GameObject prefabToSpawn = null;
        

        switch (randomStat)
        {
            case 0:
                prefabToSpawn = AttackSprite;
                break;
            case 1:
                prefabToSpawn = HealthSprite;
                break;
            case 2:
                prefabToSpawn = MovementSprite;
                break;
            //case 3:                
               // prefabToSpawn = Spell;
               // break;
        }

        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"StatSpawner: The prefab for randomStat {randomStat} is missing! Please assign it in the Inspector.");
            return;
        }

        GameObject newStats = Instantiate(prefabToSpawn, randomPosition, Quaternion.identity);

        StatPickUp pickup = newStats.GetComponent<StatPickUp>();
        if (pickup != null)
        {
            pickup.SetSpawner(this);
        }
    }

    public void RespawnStats()
    {
        StartCoroutine(RespawnCoroutine());
    }
    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnSingleStats();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Spawncenter, SpawnSize);
    }

}
