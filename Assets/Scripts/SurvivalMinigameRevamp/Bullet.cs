using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 moveDirection;

    private float speed;

    private float lifetime;

    private float timer;
    private static SurvivalScript survivalManager;
    private static bool searchedForManager = false;

    private void Awake()
    {
        CacheManager();
    }

    private void CacheManager()
    {
        // Only search for once
        if (searchedForManager) return;

        survivalManager = FindFirstObjectByType<SurvivalScript>();

        searchedForManager = true;

        if (survivalManager != null)
        {
            Debug.Log("Survival manager cached.");
        }
    }

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

        PlayerStats ps =
            other.GetComponent<PlayerStats>();

        if (ps != null)
        {
            ps.IsAliveArena = false;

            if (survivalManager != null)
            {
                survivalManager.PlayerEliminated(other.gameObject);
            }

            other.gameObject.SetActive(false);
        }

        ReturnToPool();
    }
}