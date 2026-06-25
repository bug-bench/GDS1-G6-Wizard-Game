using UnityEngine;
using UnityEngine.InputSystem;

public class SpellProjectile : MonoBehaviour
{
    [Header("Runtime Data")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float knockbackForce = 15f;

    [Header("Status Effects")]
    [SerializeField] private int burnDamage = 0;
    [SerializeField] private float burnDuration = 3f;

    [SerializeField, Range(0f, 1f)] private float slowPercentage = 0f;
    [SerializeField] private float slowDuration = 2f;

    [Header("Hit VFX")]
    public GameObject hitVFXPrefab;
    public float hitVFXLifetime = 0.5f;
    public bool rotateHitVFXToProjectile = true;

    [HideInInspector] public GameObject caster;
    [HideInInspector] public float ignoreCasterUntilTime;

    bool IsUnderSpellPickup => GetComponentInParent<SpellPickup>() != null;

    bool initialized;

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

        initialized = true;
    }

    void Awake()
    {
        if (IsUnderSpellPickup)
            enabled = false;
    }

    void Start()
    {
        if (IsUnderSpellPickup) return;

        float finalLifetime = lifeTime > 0f ? lifeTime : 3f;
        Destroy(gameObject, finalLifetime);
    }

    void Update()
    {
        if (IsUnderSpellPickup) return;

        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (IsUnderSpellPickup) return;

        if (!initialized && caster == null)
        {
            Destroy(gameObject);
            return;
        }

        if (caster == null)
        {
            if (HasTag(hitInfo, "Player"))
                Destroy(gameObject);
            return;
        }

        if (Time.time < ignoreCasterUntilTime && IsColliderOnCaster(caster, hitInfo))
            return;

        if (IsColliderOnCaster(caster, hitInfo))
            return;

        ReflectShieldSpell shield = hitInfo.GetComponent<ReflectShieldSpell>()
            ?? hitInfo.GetComponentInParent<ReflectShieldSpell>();

        if (shield != null)
        {
            shield.ApplyReflectToProjectile(this);
            return;
        }

        destroyableObject destroyable = hitInfo.GetComponent<destroyableObject>()
            ?? hitInfo.GetComponentInParent<destroyableObject>();

        if (destroyable != null)
        {
            destroyable.takeDamage(GetTotalDamage());
            SpawnHitVFX(transform.position);
            Destroy(gameObject);
            return;
        }

        if (HasTag(hitInfo, "Player"))
        {
            HandlePlayerHit(hitInfo);
            return;
        }

        if (hitInfo.gameObject.layer == LayerMask.NameToLayer("wall") || HasTag(hitInfo, "Wall"))
        {
            SpawnHitVFX(transform.position);
            Destroy(gameObject);
        }
    }


    void HandlePlayerHit(Collider2D hitInfo)
    {
        PlayerCombat target = hitInfo.GetComponent<PlayerCombat>()
            ?? hitInfo.GetComponentInParent<PlayerCombat>();

        if (target == null) return;
        if (target.gameObject == caster) return;
        if (target.IsInvincible) return;

        if (ReflectShieldSpell.HasActiveShieldOn(target))
            return;

        int attackerIndex = -1;
        var casterInput = caster.GetComponent<PlayerInput>();
        if (casterInput != null)
            attackerIndex = casterInput.playerIndex;

        Vector2 knockDirection = (Vector2)transform.up;
        target.TakeDamage(
            Mathf.RoundToInt(GetTotalDamage()),
            attackerIndex,
            knockDirection.normalized * knockbackForce);

        PlayerStats targetStats = target.GetComponent<PlayerStats>();
        if (targetStats != null)
        {
            if (burnDamage > 0)
                targetStats.ApplyBurn(burnDamage, burnDuration, 0.5f, attackerIndex);

            if (slowPercentage > 0f)
                targetStats.ApplySpeedMultiplier(1f - slowPercentage, slowDuration);
        }

        SpawnHitVFX(transform.position);
        Destroy(gameObject);
    }

    float GetTotalDamage()
    {
        float totalDamage = damage;

        if (caster != null)
        {
            PlayerStats casterStats = caster.GetComponent<PlayerStats>();
            if (casterStats != null)
                totalDamage += casterStats.strength;
        }

        return totalDamage;
    }

    void SpawnHitVFX(Vector2 hitPosition)
    {
        if (hitVFXPrefab == null) return;

        Quaternion rotation = rotateHitVFXToProjectile
            ? transform.rotation
            : Quaternion.identity;

        GameObject vfx = Instantiate(hitVFXPrefab, hitPosition, rotation);
        Destroy(vfx, hitVFXLifetime);
    }

    public void ReflectToNewCaster(GameObject newCaster, float casterIgnoreSeconds = 0.15f)
    {
        if (newCaster == null) return;

        caster = newCaster;
        ignoreCasterUntilTime = Time.time + casterIgnoreSeconds;

        RegisterWithCaster(gameObject, newCaster, casterIgnoreSeconds);
    }

    public static void RegisterWithCaster(GameObject projectileRoot, GameObject casterRoot, float casterIgnoreSeconds = 0.15f)
    {
        if (projectileRoot == null || casterRoot == null) return;

        float until = Time.time + casterIgnoreSeconds;

        Collider2D[] casterCols = casterRoot.GetComponentsInChildren<Collider2D>(true);
        Collider2D[] projCols = projectileRoot.GetComponentsInChildren<Collider2D>(true);

        foreach (SpellProjectile sp in projectileRoot.GetComponentsInChildren<SpellProjectile>(true))
        {
            sp.caster = casterRoot;
            sp.ignoreCasterUntilTime = until;
        }

        foreach (Collider2D pc in projCols)
        {
            if (pc == null) continue;

            foreach (Collider2D cc in casterCols)
            {
                if (cc == null) continue;
                Physics2D.IgnoreCollision(pc, cc, true);
            }
        }
    }

    static bool IsColliderOnCaster(GameObject casterRoot, Collider2D col)
    {
        if (casterRoot == null || col == null) return false;

        Transform t = col.transform;
        return t == casterRoot.transform || t.IsChildOf(casterRoot.transform);
    }

    static bool HasTag(Collider2D col, string tagName)
    {
        return col != null && col.gameObject.tag == tagName;
    }
}