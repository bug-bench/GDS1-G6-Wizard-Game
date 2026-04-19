using Unity.VisualScripting;
using UnityEngine;

public class destroyableObject : MonoBehaviour
{
    public float health = 20f;

    public GameObject[] StatPrefabs;
    public int mindrops = 2;
    public int maxdrops = 5;



    public GameObject[] SpellPrefabs;
    [Range(0f,1f)]
    public float SpelldropChance = 0.2f;

    public float dropforce = 3f;

    private Vector3 startPosition;
    void Start()
    {
        startPosition = transform.position;
    }

    public void takeDamage(float damage)
    {
        health -= damage;

        if(health <=0)
        {
            BreakObject();
        }
    }
   
    void BreakObject()
    {
        DropStats();
        DropSpells();

        Destroy(gameObject);

    }

    void DropStats()
    {
        if (StatPrefabs.Length == 0) return;

        int dropCount = Random.Range(mindrops, maxdrops + 1);

        for (int i = 0; i<dropCount; i++)
        {
            GameObject prefab = StatPrefabs[Random.Range(0, StatPrefabs.Length)];

            Vector2 offset = Random.insideUnitCircle * 1f;
            GameObject drops = Instantiate(prefab, (Vector2)transform.position + offset, Quaternion.identity);

            Rigidbody2D rb = drops.GetComponent<Rigidbody2D>();

            if(rb !=null)
            {
                Vector2 force = Random.insideUnitCircle * dropforce;
                rb.AddForce(force, ForceMode2D.Impulse);
            }
        }

    }
    void DropSpells()
    {
        if (SpellPrefabs.Length == 0) return;

        float roll = Random.value;

        if(roll<= SpelldropChance)
        {
            GameObject prefab = StatPrefabs[Random.Range(0, SpellPrefabs.Length)];

            GameObject drops = Instantiate(prefab, transform.position, Quaternion.identity);

            Rigidbody2D rb = drops.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 force = Random.insideUnitCircle * dropforce;
                rb.AddForce(force, ForceMode2D.Impulse);
            }
        }
    }

}
