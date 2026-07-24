using UnityEngine;

public class IceSpikeSpell : ProjectileSpellCore
{
    [Header("Spread")]

    public int projectileCount = 3;

    public float spreadAngle = 25f;

    protected override void SpawnProjectile()
    {
        float angleStep =
            projectileCount > 1
            ? spreadAngle / (projectileCount - 1)
            : 0f;

        float startAngle =
            -spreadAngle * 0.5f;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle =
                startAngle +
                angleStep * i;

            GameObject spike =
                Instantiate(
                    gameObject,
                    transform.position,
                    transform.rotation *
                    Quaternion.Euler(0,0,angle));

            IceSpikeSpell spell =
                spike.GetComponent<IceSpikeSpell>();

            spell.caster = caster;
            spell.firePoint = firePoint;

            spell.ExecuteProjectile();
        }

        Destroy(gameObject);
    }
}