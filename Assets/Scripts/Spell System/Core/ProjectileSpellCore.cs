using UnityEngine;

public abstract class ProjectileSpellCore : SpellBehavior
{

    [Header("Projectile")]

    [Tooltip("Speed in units per second.")]
    public float speed = 10f;

    [Tooltip("Seconds before projectile expires.")]
    public float lifetime = 3f;

    [Tooltip("Destroy immediately after hitting something.")]
    public bool destroyOnHit = true;


    [Header("Combat")]

    public float damage = 10f;

    public float knockbackForce = 15f;

    [Header("Status Effects")]

    public int burnDamage;

    public float burnDuration;

    [Range(0f,1f)]
    public float slowPercentage;

    public float slowDuration;


    //Runtime
    protected bool initialized;

    protected float destroyTime;

    protected bool isDestroyed;

    protected GameObject Caster;

    protected PlayerStats CasterStats;

    protected Rigidbody2D CasterRB;

    protected float SizeScale = 1f;

    protected int AttackerIndex = -1;

    protected float IgnoreCasterUntil;

    
    // Execute
    public override void Execute()
    {
        // Cache runtime information
        CacheRuntime();

        // Allow children to customise launch
        OnProjectileSpawned();

        // Start lifetime timer
        destroyTime = Time.time + lifetime;

        initialized = true;
    }

    // Runtime Cache

    protected virtual void CacheRuntime()
    {
        Caster = caster;

        if (Caster == null)
            return;

        CasterStats =
            Caster.GetComponent<PlayerStats>();

        CasterRB =
            Caster.GetComponent<Rigidbody2D>();

        SizeScale =
            SpellStatScaling.GetSizeScale(Caster);

        var input =
            Caster.GetComponent<UnityEngine.InputSystem.PlayerInput>();

        if (input != null)
            AttackerIndex = input.playerIndex;
    }

    // Update
    protected virtual void Update()
    {
        if (!initialized)
            return;

        if (isDestroyed)
            return;

        UpdateLifetime();

        MoveProjectile();

        OnProjectileUpdated();
    }

    // Movement

    // Default straight-line movement.
    // Override for homing missiles,

    protected virtual void MoveProjectile()
    {
        transform.position +=
            transform.up *
            speed *
            Time.deltaTime;
    }

    // Lifetime

    protected virtual void UpdateLifetime()
    {
        if (Time.time >= destroyTime)
        {
            DestroyProjectile();
        }
    }

    // Destroy
    protected virtual void DestroyProjectile()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        OnProjectileDestroyed();

        Destroy(gameObject);
    }

    //Hooks
    protected virtual void OnProjectileSpawned()
    {

    }


    protected virtual void OnProjectileUpdated()
    {

    }

    protected virtual void OnProjectileDestroyed()
    {

    }
}