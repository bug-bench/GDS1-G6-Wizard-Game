using UnityEngine;

public class SurvivalHazard : MonoBehaviour
{
    public enum Direction
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    public Direction moveDirection;

    [Header("Stats")]
    public float speed = 1f;
    public float health = 30f;

    private float maxHealth;

    private Vector3 startPosition;
    private bool moving = true;

    private SurvivalScript manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        maxHealth = health;
    }

    // Update is called once per frame
    void Update()
    {
        if (!moving) return;

        Move();

        if (ReachedEnd())
        {
            moving = false;
            manager.HazardFinished();
        }
    }

    public void SetManager(SurvivalScript m)
    {
        manager = m;
    }

    void Move()
    {
        Vector3 dir = Vector3.zero;

        switch (moveDirection)
        {
            case Direction.LeftToRight: dir = Vector3.right; break;
            case Direction.RightToLeft: dir = Vector3.left; break;
            case Direction.TopToBottom: dir = Vector3.down; break;
            case Direction.BottomToTop: dir = Vector3.up; break;
        }

        transform.position += dir * speed * Time.deltaTime;
    }

    bool ReachedEnd()
    {
        float limit = 20f;

        return Mathf.Abs(transform.position.x) > limit || 
        Mathf.Abs(transform.position.y) > limit;
    }

    // ====================
    // DAMAGE SYSTEM
    // ====================

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        moving = false;
        ResetToStart();
    }

    // ====================
    // RESET + LOOP
    // ====================

    public void ResetToStart()
    {
        transform.position = startPosition;
        health = maxHealth;
        moving = true;
    }

    public void IncreaseDifficulty(float multiplier)
    {
        maxHealth = 30f * multiplier;
        speed = 1f * multiplier;
    }

    // ====================
    // PLAYER COLLISION
    // ====================

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();

            if (ps != null)
            {
                ps.IsAliveArena = false;

                SurvivalScript ss = FindFirstObjectByType<SurvivalScript>();
                if (ss != null)
                {
                    ss.PlayerEliminated(other.gameObject);
                }

                other.gameObject.SetActive(false);
            }
        }
    }
}
