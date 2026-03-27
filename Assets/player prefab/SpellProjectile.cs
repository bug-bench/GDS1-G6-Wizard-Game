using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    public float lifeTime = 3f;

    // 用来记录发射这个火球的玩家是谁
    [HideInInspector] public GameObject caster;

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
        // 如果碰到的东西就是发射者自己，直接无视，穿过去
        if (hitInfo.gameObject == caster) return;

        if (hitInfo.CompareTag("Player"))
        {
            PlayerCombat target = hitInfo.GetComponent<PlayerCombat>();
            if (target != null)
            {
                target.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else if (hitInfo.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
