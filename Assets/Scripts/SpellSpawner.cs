using System.Collections;
using UnityEngine;

public class SpellSpawner : MonoBehaviour
{
    public GameObject[] spellPrefab;


    public Vector2 Spawncenter;
    public Vector2 SpawnSize = new Vector2(10F, 10F);


    public int numberToSpawn = 30;

    public float respawnDelay = 10f;



    public float DistancebetweenStats = 1f;
    public LayerMask statLayer;
    public int maxattempts = 20;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnSpells();
    }

   void SpawnSpells()
    {
        for(int i = 0; i <numberToSpawn; i++)
        {
            SpawnSingleSpell();
        }
    }
    void SpawnSingleSpell()
    {
        Vector2 randomPosition = Vector2.zero;
        bool foundValidPosition = false;

        for (int i = 0; i < maxattempts; i++)
        {


            randomPosition = new Vector2(
                   Random.Range(Spawncenter.x - SpawnSize.x / 2f, Spawncenter.x + SpawnSize.x / 2f),
                   Random.Range(Spawncenter.y - SpawnSize.y / 2f, Spawncenter.y + SpawnSize.y / 2f)
               );

            Collider2D hit = Physics2D.OverlapCircle(randomPosition, DistancebetweenStats, statLayer);

            if (hit == null)
            {
                foundValidPosition = true;
                break;
            }



        }

        int randomIndex = Random.Range(0, spellPrefab.Length);
        GameObject prefabToSpawn = spellPrefab[randomIndex];

        GameObject newSpell = Instantiate(prefabToSpawn, randomPosition, Quaternion.identity);

        SpellPickup pickup = newSpell.GetComponent<SpellPickup>();

        if(pickup != null)
        {
            pickup.SetSpawner(this);
        }
    }

 


    public void RespawnSpell()
    {
        StartCoroutine(RespawnCoroutine());
    }
    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnSingleSpell();
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Spawncenter, SpawnSize);
    }
}
