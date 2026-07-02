using UnityEngine;
using UnityEngine.InputSystem;

// <summary>
// Base class for all projectile-based spells.

// Contains the shared functionality used by every projectile including:
// movement, collision, damage, status effects, reflection and lifetime.

// Individual projectile spells should inherit from this class and only
// override behaviour that is unique to that spell.
// </summary>
public abstract class ProjectileSpellCore : SpellBehavior
{
    #region Projectile

    [Header("Projectile")]

    [Tooltip("Units travelled per second.")]
    public float speed = 10f;

    [Tooltip("How long the projectile exists.")]
    public float lifetime = 3f;

    [Tooltip("Destroy immediately after hitting something.")]
    public bool destroyOnHit = true;
    private GameObject ignoredCaster;

    #endregion

    #region Combat

    [Header("Combat")]

    public float damage = 10f;

    public float knockbackForce = 15f;

    #endregion

    #region Status Effects

    [Header("Burn")]

    public int burnDamage;

    public float burnDuration;

    [Header("Slow")]

    [Range(0f,1f)]
    public float slowPercentage;

    public float slowDuration;

    #endregion

    #region Impact

    [Header("Impact")]

    public GameObject hitVFXPrefab;

    public float hitVFXLifetime = 0.5f;

    public bool rotateHitVFX = true;

    #endregion

    #region Runtime

    protected bool initialized;

    protected bool destroyed;

    protected float destroyTime;

    protected PlayerStats Stats;

    protected Rigidbody2D casterRB;

    protected int attackerIndex = -1;

    protected float sizeScale = 1f;

    public float IgnoreCasterUntil { get; protected set; }

    #endregion

    #region Properties

    protected Vector2 Direction
        => transform.up;

    protected float TotalDamage
        => GetTotalDamage();

    protected new float SizeScale
        => sizeScale;

    #endregion

    #region Execute

    protected virtual void SpawnProjectile()
    {
        ExecuteProjectile();
    }

    protected virtual void ExecuteProjectile()
    {
        CacheRuntime();

        destroyTime = Time.time + lifetime;

        initialized = true;

        RegisterWithCaster();

        OnProjectileSpawned();
    }

    public override void Execute()
    {
        SpawnProjectile();
    }

    #endregion

    #region Runtime

    protected virtual void CacheRuntime()
    {
        if (caster == null)
            return;

        casterStats =
            caster.GetComponent<PlayerStats>();

        casterRB =
            caster.GetComponent<Rigidbody2D>();

        sizeScale =
            SpellStatScaling.GetSizeScale(caster);

        PlayerInput input =
            caster.GetComponent<PlayerInput>();

        if (input != null)
            attackerIndex =
                input.playerIndex;
    }

    #endregion

    #region Update

    protected virtual void Update()
    {
        if (!initialized)
            return;

        if (destroyed)
            return;

        MoveProjectile();

        UpdateLifetime();

        OnProjectileUpdated();
    }

    #endregion

    #region Movement

    // Default straight-line movement.
    // Override for homing, boomerangs, orbiting projectiles, etc.
    protected virtual void MoveProjectile()
    {
        transform.position +=
            (Vector3)(Direction * speed * Time.deltaTime);
    }

    #endregion

    #region Lifetime

    protected virtual void UpdateLifetime()
    {
        if (Time.time >= destroyTime)
        {
            DestroyProjectile();
        }
    }

    #endregion

    #region Destroy

    protected virtual void DestroyProjectile()
    {
        if (destroyed)
            return;

        destroyed = true;

        OnProjectileDestroyed();

        Destroy(gameObject);
    }

    protected void DestroyIfRequired()
    {
        if (destroyOnHit)
            DestroyProjectile();
    }

    #endregion
    #region Collision

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized)
            return;

        if (destroyed)
            return;

        if (!CanCollide(other))
            return;

        HandleCollision(other);
    }

    // Determines whether this collision should be processed.
    protected virtual bool CanCollide(Collider2D other)
    {
        if (other == null)
            return false;

        if (other.isTrigger)
        {
            ReflectShieldSpell shield =
                other.GetComponent<ReflectShieldSpell>()
                ?? other.GetComponentInParent<ReflectShieldSpell>();

            if (shield == null)
                return false;
        }

        // Ignore collisions with the caster immediately after spawning.
        if (Time.time < IgnoreCasterUntil && IsCaster(other))
            return false;

        // Never hit yourself.
        if (IsCaster(other))
            return false;

        return true;
    }

    // Routes the collision to the correct handler.
    protected virtual void HandleCollision(Collider2D other)
    {
        if (TryHandleShield(other))
            return;

        if (TryHandleDestroyable(other))
            return;

        if (TryHandlePlayer(other))
            return;

        if (TryHandleWall(other))
            return;

        OnProjectileHit(other);
    }

    #endregion


    #region Collision Routing

    protected virtual bool TryHandlePlayer(Collider2D other)
    {
        PlayerCombat player =
            other.GetComponent<PlayerCombat>()
            ?? other.GetComponentInParent<PlayerCombat>();

        if (player == null)
            return false;

        OnPlayerHit(player);

        return true;
    }

    protected virtual bool TryHandleDestroyable(Collider2D other)
    {
        destroyableObject destroyable =
            other.GetComponent<destroyableObject>()
            ?? other.GetComponentInParent<destroyableObject>();

        if (destroyable == null)
            return false;

        OnDestroyableHit(destroyable);

        return true;
    }

    protected virtual bool TryHandleShield(Collider2D other)
    {
        ReflectShieldSpell shield =
            other.GetComponent<ReflectShieldSpell>()
            ?? other.GetComponentInParent<ReflectShieldSpell>();

        if (shield == null)
        {
            Debug.Log("No ReflectShieldSpell found.");
            return false;
        }

        Debug.Log("Reflect shield found!");

        OnShieldHit(shield);

        return true;
    }

    protected virtual bool TryHandleWall(Collider2D other)
    {
        bool isWall =
            other.gameObject.layer == LayerMask.NameToLayer("wall") ||
            other.CompareTag("wall");

        if (!isWall)
            return false;

        OnWallHit(other);

        return true;
    }

    #endregion


    #region Utility

    // Returns true if the collider belongs to the caster.
    protected bool IsCaster(Collider2D col)
    {
        if (caster == null || col == null)
            return false;

        Transform t = col.transform;

        return t == caster.transform ||
               t.IsChildOf(caster.transform);
    }

    // Registers this projectile with its caster so they
    // cannot immediately collide with each other.
    public virtual void RegisterWithCaster(float ignoreTime = 0.15f)
    {
        if (caster == null)
            return;

        // Restore collisions with the previous owner.
        if (ignoredCaster != null)
        {
            SetCollisionWithCaster(
                ignoredCaster,
                false);
        }

        IgnoreCasterUntil = Time.time + ignoreTime;

        // Ignore collisions with the new owner.
        SetCollisionWithCaster(
            caster,
            true);

        ignoredCaster = caster;
    }

    private void SetCollisionWithCaster(
        GameObject targetCaster,
        bool ignore)
    {
        if (targetCaster == null)
            return;

        Collider2D[] projectileColliders =
            GetComponentsInChildren<Collider2D>(true);

        Collider2D[] casterColliders =
            targetCaster.GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D projectileCollider in projectileColliders)
        {
            if (projectileCollider == null)
                continue;

            foreach (Collider2D casterCollider in casterColliders)
            {
                if (casterCollider == null)
                    continue;

                Physics2D.IgnoreCollision(
                    projectileCollider,
                    casterCollider,
                    ignore);
            }
        }
    }

    #endregion
    #region Combat

    // Handles hitting a player.
    // Override this if a projectile has unique behaviour.
    protected virtual void OnPlayerHit(PlayerCombat player)
    {
        if (player == null)
            return;

        if (player.gameObject == caster)
            return;

        if (player.IsInvincible)
            return;

        if (ReflectShieldSpell.HasActiveShieldOn(player))
            return;

        ApplyDamage(player);

        ApplyStatusEffects(player);

        SpawnHitVFX(transform.position);

        DestroyIfRequired();
    }

    // Handles hitting a destroyable object.
    protected virtual void OnDestroyableHit(destroyableObject destroyable)
    {
        if (destroyable == null)
            return;

        destroyable.takeDamage(GetTotalDamage());

        SpawnHitVFX(transform.position);

        DestroyIfRequired();
    }

    // Handles hitting a wall.
    protected virtual void OnWallHit(Collider2D wall)
    {
        SpawnHitVFX(transform.position);

        DestroyIfRequired();
    }

    // Called whenever the projectile collides with a shield.
    protected virtual void OnShieldHit(ReflectShieldSpell shield)
    {
        Debug.Log("Projectile hit shield!");
        if (shield == null)
            return;

        shield.ApplyReflectToProjectile(this);
    }

    // Generic collision callback.
    protected virtual void OnProjectileHit(Collider2D other)
    {

    }

    #endregion


    #region Damage

    protected virtual void ApplyDamage(PlayerCombat player)
    {
        Vector2 knockback =
            Direction.normalized *
            knockbackForce;

        player.TakeDamage(
            Mathf.RoundToInt(GetTotalDamage()),
            attackerIndex,
            knockback);
    }

    protected virtual float GetTotalDamage()
    {
        float total = damage;

        if (casterStats != null)
            total += casterStats.strength;

        return total;
    }

    #endregion


    #region Status Effects

    protected virtual void ApplyStatusEffects(PlayerCombat player)
    {
        PlayerStats targetStats =
            player.GetComponent<PlayerStats>();

        if (targetStats == null)
            return;

        if (burnDamage > 0)
        {
            targetStats.ApplyBurn(
                burnDamage,
                burnDuration,
                0.5f,
                attackerIndex);
        }

        if (slowPercentage > 0f)
        {
            targetStats.ApplySpeedMultiplier(
                1f - slowPercentage,
                slowDuration);
        }
    }

    #endregion


    #region Reflection

    // Called by ReflectShieldSpell when the projectile is reflected.
    public virtual void Reflect(GameObject newCaster)
    {

        if (newCaster == null)
            return;

        caster = newCaster;

        CacheRuntime();

        RegisterWithCaster();

        transform.up = -transform.up;

        OnReflected();
    }

    protected virtual void OnReflected()
    {

    }

    #endregion


    #region Hit VFX

    protected virtual void SpawnHitVFX(Vector3 position)
    {
        if (hitVFXPrefab == null)
            return;

        Quaternion rotation =
            rotateHitVFX
            ? transform.rotation
            : Quaternion.identity;

        GameObject vfx =
            Instantiate(
                hitVFXPrefab,
                position,
                rotation);

        Destroy(
            vfx,
            hitVFXLifetime);
    }

    #endregion
    #region Utilities

    // Returns the world position a given distance in front of the projectile.
    // Useful for explosions, trails and spawning effects.
    protected Vector2 ForwardPosition(float distance)
    {
        return (Vector2)transform.position +
               Direction * distance;
    }

    // Instantiates an object using the projectile's position and rotation.
    protected GameObject Spawn(GameObject prefab)
    {
        if (prefab == null)
            return null;

        return Instantiate(
            prefab,
            transform.position,
            transform.rotation);
    }

    // Instantiates an object at a custom position.
    protected GameObject Spawn(GameObject prefab, Vector3 position)
    {
        if (prefab == null)
            return null;

        return Instantiate(
            prefab,
            position,
            transform.rotation);
    }

    // Instantiates an object at a custom position and rotation.
    protected GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        return Instantiate(
            prefab,
            position,
            rotation);
    }

    #endregion


    #region Virtual Hooks

    // Called once immediately after Execute().
    protected virtual void OnProjectileSpawned()
    {

    }

    // Called every frame after movement.
    protected virtual void OnProjectileUpdated()
    {

    }

    // Called immediately before Destroy(gameObject).
    protected virtual void OnProjectileDestroyed()
    {

    }

    #endregion


#if UNITY_EDITOR

    #region Gizmos

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            0.15f);

        Gizmos.color = Color.red;

        Gizmos.DrawLine(
            transform.position,
            transform.position + transform.up);
    }

    #endregion

#endif

}