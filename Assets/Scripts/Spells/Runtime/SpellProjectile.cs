using UnityEngine;
using UnityEngine.InputSystem;

public class SpellProjectile : MonoBehaviour
{
    [Header("Runtime Data")]
    private float damage;
    private float speed;
    private float lifeTime;
    private float knockbackForce;

    private int burnDamage;
    private float burnDuration;

    private float slowPercentage;
    private float slowDuration;

    [HideInInspector]
    public GameObject caster;

    [HideInInspector]
    public float ignoreCasterUntilTime;

    [Header("Hit VFX")]
    public GameObject hitVFXPrefab;
    public float hitVFXLifetime = 0.5f;

    bool IsUnderSpellPickup =>
        GetComponentInParent<SpellPickup>() != null;

    public void Initialize(
        GameObject caster,
        float damage,
        float speed,
        float lifeTime,
        float knockbackForce,
        int burnDamage,
        float burnDuration,
        float slowPercentage,
        float slowDuration)
    {
        this.caster = caster;

        this.damage = damage;
        this.speed = speed;
        this.lifeTime = lifeTime;
        this.knockbackForce = knockbackForce;

        this.burnDamage = burnDamage;
        this.burnDuration = burnDuration;

        this.slowPercentage = slowPercentage;
        this.slowDuration = slowDuration;
    }

    void Awake()
    {
        if (IsUnderSpellPickup)
            enabled = false;
    }

    void Start()
    {
        if (IsUnderSpellPickup)
            return;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(
            Vector3.up *
            speed *
            Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        if (caster == null)
            return;

        if (Time.time < ignoreCasterUntilTime &&
            IsColliderOnCaster(caster, hit))
            return;

        if (IsColliderOnCaster(caster, hit))
            return;

        destroyableObject destroyable =
            hit.GetComponent<destroyableObject>()
            ?? hit.GetComponentInParent<destroyableObject>();

        if (destroyable != null)
        {
            destroyable.takeDamage(
                damage + StrengthBonus());

            SpawnHitVFX();
            Destroy(gameObject);
            return;
        }

        if (hit.CompareTag("Player"))
        {
            HandlePlayerHit(hit);
            return;
        }

        if (hit.gameObject.layer ==
            LayerMask.NameToLayer("wall"))
        {
            SpawnHitVFX();
            Destroy(gameObject);
        }
    }

    void HandlePlayerHit(Collider2D hit)
    {
        PlayerCombat target =
            hit.GetComponent<PlayerCombat>()
            ?? hit.GetComponentInParent<PlayerCombat>();

        if (target == null)
            return;

        if (target.gameObject == caster)
            return;

        if (target.IsInvincible)
            return;

        float finalDamage =
            damage + StrengthBonus();

        int attackerIndex = -1;

        PlayerInput input =
            caster.GetComponent<PlayerInput>();

        if (input != null)
            attackerIndex = input.playerIndex;

        Vector2 knockback =
            (Vector2)transform.up *
            knockbackForce;

        target.TakeDamage(
            Mathf.RoundToInt(finalDamage),
            attackerIndex,
            knockback);

        PlayerStats stats =
            target.GetComponent<PlayerStats>();

        if (stats != null)
        {
            if (burnDamage > 0)
            {
                stats.ApplyBurn(
                    burnDamage,
                    burnDuration,
                    0.5f,
                    attackerIndex);
            }

            if (slowPercentage > 0f)
            {
                stats.ApplySpeedMultiplier(
                    1f - slowPercentage,
                    slowDuration);
            }
        }

        SpawnHitVFX();

        Destroy(gameObject);
    }

    float StrengthBonus()
    {
        if (caster == null)
            return 0f;

        PlayerStats stats =
            caster.GetComponent<PlayerStats>();

        return stats != null
            ? stats.strength
            : 0f;
    }

    void SpawnHitVFX()
    {
        if (hitVFXPrefab == null)
            return;

        GameObject vfx =
            Instantiate(
                hitVFXPrefab,
                transform.position,
                transform.rotation);

        Destroy(vfx, hitVFXLifetime);
    }

    static bool IsColliderOnCaster(
        GameObject casterRoot,
        Collider2D col)
    {
        if (casterRoot == null || col == null)
            return false;

        Transform t = col.transform;

        return t == casterRoot.transform ||
               t.IsChildOf(casterRoot.transform);
    }

    public static void RegisterWithCaster(
        GameObject projectileRoot,
        GameObject casterRoot,
        float casterIgnoreSeconds = 0.15f)
    {
        if (projectileRoot == null ||
            casterRoot == null)
            return;

        float until =
            Time.time + casterIgnoreSeconds;

        Collider2D[] casterCols =
            casterRoot.GetComponentsInChildren<Collider2D>(true);

        Collider2D[] projCols =
            projectileRoot.GetComponentsInChildren<Collider2D>(true);

        foreach (SpellProjectile sp in projectileRoot.GetComponentsInChildren<SpellProjectile>())
        {
            sp.caster = casterRoot;
            sp.ignoreCasterUntilTime = until;
        }

        foreach (Collider2D pc in projCols)
        {
            foreach (Collider2D cc in casterCols)
            {
                Physics2D.IgnoreCollision(
                    pc,
                    cc,
                    true);
            }
        }
    }
}