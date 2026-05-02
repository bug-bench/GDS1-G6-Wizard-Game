using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class InteractableChest : MonoBehaviour
{
    public GameObject[] StatPrefabs;
    public int mindrops = 2;
    public int maxdrops = 5;



    public GameObject[] SpellPrefabs;
    [Range(0f, 1f)]
    public float SpelldropChance = 0.2f;

    public float dropforce = 3f;

    private PlayerInput currentPlayer;

    private Animator anim;
    private bool Opened = false;


    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Opened || currentPlayer == null) return;

        var gamepad = currentPlayer.GetDevice<Gamepad>();
        var keyboard = currentPlayer.GetDevice<Keyboard>();

        if(keyboard != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            
            Openchest();
        }

        if(gamepad !=null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            
            Openchest();
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

        for (int i = 0; i < dropCount; i++)
        {
            GameObject prefab = StatPrefabs[Random.Range(0, StatPrefabs.Length)];

            Vector2 offset = Random.insideUnitCircle * 0.5f;
            GameObject drops = Instantiate(prefab,(Vector2) transform.position+offset, Quaternion.identity);

            Rigidbody2D rb = drops.GetComponent<Rigidbody2D>();

            if (rb != null)
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

        if (roll <= SpelldropChance)
        {
            GameObject prefab = SpellPrefabs[Random.Range(0, SpellPrefabs.Length)];

            GameObject drops = Instantiate(prefab, transform.position, Quaternion.identity);

            Rigidbody2D rb = drops.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 force = Random.insideUnitCircle * dropforce;
                rb.AddForce(force, ForceMode2D.Impulse);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInput pi = other.GetComponent<PlayerInput>();
        if (pi != null)
        {
            currentPlayer = pi;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInput pi = other.GetComponent<PlayerInput>();
        if (pi != null && pi == currentPlayer)
        {
            currentPlayer = pi;
        }
    }

    void Openchest()
    {

        Opened = true;

        if(anim != null)
        {

            anim.SetTrigger("Open");


        }

        StartCoroutine(OpenRoutine());

       


    }

    IEnumerator OpenRoutine()
    {
        yield return new WaitForSeconds(1.6f);

        BreakObject();

    }

}
