using System.Collections;
using UnityEngine;

public class ostacleSpawner : MonoBehaviour
{

    public GameObject[] sprites;

    public Vector2 spawnplace;
    public Vector2 size = new Vector2(10f, 10f);


    public int numberofspawn = 10;
    public int Delay = 1;


    void Start()
    {
        SpawnAll();
    }

   void SpawnAll()
    {
        for(int i = 0; i<numberofspawn; i++)
        {
            spawnOne();
        }
    }

   public void respawn()
    {
        StartCoroutine(RespawnCouroutine());

    }

    public void spawnOne()
    {
       if(sprites.Length == 0)
        {
            return;
        }

       Vector2 pos = new Vector2(
           Random.Range(spawnplace.x - size.x /2f,spawnplace.x + size.x /2), 
            Random.Range(spawnplace.y - size.y / 2f, spawnplace.y + size.y / 2));

        GameObject prefab = sprites[Random.Range(0, sprites.Length)];

        Instantiate(prefab, pos, Quaternion.identity);

            
    }

    IEnumerator RespawnCouroutine()
    {
        yield return new WaitForSeconds(Delay);
        spawnOne();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(spawnplace, size);
    }

}
