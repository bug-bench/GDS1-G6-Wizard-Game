using UnityEngine;

public class StatSpawner : MonoBehaviour
{
    public GameObject AttackSprite;
    public GameObject HealthSprite;
    public GameObject MovementSprite;


    public Vector2 Spawncenter;
    public Vector2 SpawnSize = new Vector2(10F, 10F);

    public int numberToSpawn = 10;
    void Start()
    {
        SpawnStats();
    }


    void SpawnStats()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            Vector2 randomPosition = new Vector2(
                Random.Range(Spawncenter.x - SpawnSize.x / 2f, Spawncenter.x + SpawnSize.x / 2f),
                Random.Range(Spawncenter.y - SpawnSize.y / 2f, Spawncenter.y + SpawnSize.y / 2f)
            );

            int randomStat = Random.Range(0, 3);

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
            }

            Instantiate(prefabToSpawn, randomPosition, Quaternion.identity);
        }
    }




    
}
