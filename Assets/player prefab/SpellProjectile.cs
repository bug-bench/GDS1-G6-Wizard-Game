using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    public float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            PlayerCombat target = hitInfo.GetComponent<PlayerCombat>();
            if (target != null)
            {
                target.TakeDamage(damage);
                Destroy(gameObject); 
            }
        }
    }
}