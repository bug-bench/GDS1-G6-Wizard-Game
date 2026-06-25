using UnityEngine;

public class SurvivalBullet : MonoBehaviour
{
    private Vector2 moveDirection;

    private float speed;

    private float lifetime;

    private float timer;

    public void Initialize(
        Vector2 direction,
        float bulletSpeed,
        float bulletLifetime
    )
    {
        moveDirection = direction.normalized;

        speed = bulletSpeed;

        lifetime = bulletLifetime;

        timer = 0f;

        // Face movement direction
        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0, 0, angle - 90f);
    }

    private void Update()
    {
        transform.position +=
            (Vector3)(moveDirection * speed * Time.deltaTime);

        timer += Time.deltaTime;

        if (timer >= lifetime)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        BulletPool.Instance.ReturnBullet(gameObject);
    }

    private void OnDisable()
    {
        timer = 0f;
    }

    // ====================
    // PLAYER COLLISION
    // ====================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerCombat pc = other.GetComponent<PlayerCombat>();

        if (pc != null)
        {
            pc.TakeDamage(30);
        }
        ReturnToPool();
    }
}