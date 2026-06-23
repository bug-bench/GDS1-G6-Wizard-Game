using UnityEngine;

public abstract class ProjectileSpellCore : SpellBehavior
{
    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Combat")]
    public float damage = 10f;
    public float speed = 10f;
    public float lifeTime = 3f;
    public float knockbackForce = 15f;

    [Header("Status Effects")]
    public int burnDamage;
    public float burnDuration;

    [Range(0f, 1f)]
    public float slowPercentage;

    public float slowDuration;

    [Header("Spread")]
    public int projectileCount = 1;
    public float spreadAngle;

    public override void Execute()
    {
        SpawnProjectiles();
        Destroy(gameObject);
    }

    protected virtual void SpawnProjectiles()
    {
        float angleStep =
            projectileCount > 1
                ? spreadAngle / (projectileCount - 1)
                : 0f;

        float startAngle =
            projectileCount > 1
                ? -spreadAngle * 0.5f
                : 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + angleStep * i;

            Quaternion rotation =
                firePoint.rotation *
                Quaternion.Euler(0f, 0f, angle);

            GameObject projectileObject =
                Instantiate(
                    projectilePrefab,
                    firePoint.position,
                    rotation);

            SpellProjectile projectile =
                projectileObject.GetComponent<SpellProjectile>();

            if (projectile != null)
            {
                projectile.Initialize(
                    caster,
                    damage,
                    speed,
                    lifeTime,
                    knockbackForce,
                    burnDamage,
                    burnDuration,
                    slowPercentage,
                    slowDuration);
            }

            SpellProjectile.RegisterWithCaster(
                projectileObject,
                caster);

            SpellStatScaling.ApplyProjectileSize(
                projectileObject,
                caster);
        }
    }
}