using UnityEngine;

public class ShotgunSpell : SpellBehavior
{
    [Header("霰弹枪数值接口 — Shotgun Tuning")]
    public GameObject pelletPrefab;
    public int pelletCount = 5;
    public float spreadAngle = 45f;
    public float spawnForwardInset = 0.12f;

    public override void Execute(GameObject caster, Transform firePoint)
    {
        if (pelletCount <= 0) { Destroy(gameObject); return; }

        float angleStep = pelletCount > 1 ? spreadAngle / (pelletCount - 1) : 0f;
        float startAngle = pelletCount > 1 ? -spreadAngle / 2f : 0f;

        for (int i = 0; i < pelletCount; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, 0, currentAngle);

            Vector3 pos = firePoint.position + rotation * Vector3.up * spawnForwardInset;
            GameObject pellet = Instantiate(pelletPrefab, pos, rotation);
            SpellProjectile.RegisterWithCaster(pellet, caster);
        }

        Destroy(gameObject);
    }
}
