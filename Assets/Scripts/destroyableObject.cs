using Unity.VisualScripting;
using System.Collections;
using UnityEngine;

public class destroyableObject : MonoBehaviour
{
    public int hitstoBreak = 3;
    private int currentHits = 0;
  

    public GameObject[] StatPrefabs;
    public int mindrops = 2;
    public int maxdrops = 5;



    public GameObject[] SpellPrefabs;
    [Range(0f,1f)]
    public float SpelldropChance = 0.2f;

    public float dropforce = 3f;

    

    public float delay = 0.5f;
    private Animator anim;
    private bool isBroken = false;


    private AudioSource BarrelSound;
    void Start()
    {
       
        anim = GetComponent<Animator>();

        BarrelSound = GetComponent<AudioSource>();
    }

    public void takeDamage(float damage)
    {

        if (isBroken) return;

        currentHits++;
        
        if(currentHits<hitstoBreak)
        {
            if(anim != null)
            {
                anim.SetTrigger("Hit");

            }
        }
        else
        {
            BreakObject();
        }
    }
   
    void BreakObject()
    {
        isBroken = true;
       if(anim !=null)
        {
            anim.SetTrigger("Break");
        }

        StartCoroutine(BreakRoutine());

        BarrelSound.Play();

    }

    IEnumerator BreakRoutine()
    {
        yield return new WaitForSeconds(delay);
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
