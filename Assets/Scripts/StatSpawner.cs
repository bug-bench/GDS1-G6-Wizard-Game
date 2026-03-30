using System.Collections;
using UnityEngine;

public class StatSpawner : MonoBehaviour
{
    public GameObject AttackSprite;
    public GameObject HealthSprite;
    public GameObject MovementSprite;
    public GameObject Laser;


    public Vector2 Spawncenter;
    public Vector2 SpawnSize = new Vector2(10F, 10F);

    public int numberToSpawn = 30;

    public float respawnDelay = 3f;
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
        Vector2 randomPosition = new Vector2(
                Random.Range(Spawncenter.x - SpawnSize.x / 2f, Spawncenter.x + SpawnSize.x / 2f),
                Random.Range(Spawncenter.y - SpawnSize.y / 2f, Spawncenter.y + SpawnSize.y / 2f)
            );

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
            case 3:                
                prefabToSpawn = Laser;
                break;
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
    
}
