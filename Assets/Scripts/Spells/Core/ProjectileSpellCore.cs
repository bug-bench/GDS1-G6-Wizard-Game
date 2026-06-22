using UnityEngine;

public abstract class ProjectileSpellCore : SpellBehavior
{
    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Projectile Stats")]
    public float damage = 10f;
    public float speed = 10f;
    public float lifetime = 3f;
    public float knockbackForce = 10f;

    [Header("Spread")]
    public int projectileCount = 1;
    public float spreadAngle = 0f;

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

            GameObject projectile =
                Instantiate(
                    projectilePrefab,
                    firePoint.position,
                    rotation);

            ConfigureProjectile(projectile);
        }
    }

    protected virtual void ConfigureProjectile(
        GameObject projectile)
    {
        SpellProjectile sp =
            projectile.GetComponent<SpellProjectile>();

        if (sp == null)
            return;

        sp.damage = damage;
        sp.speed = speed;
        sp.lifeTime = lifetime;
        sp.knockbackForce = knockbackForce;

        SpellProjectile.RegisterWithCaster(
            projectile,
            caster);

        SpellStatScaling.ApplyProjectileSize(
            projectile,
            caster);
    }
}