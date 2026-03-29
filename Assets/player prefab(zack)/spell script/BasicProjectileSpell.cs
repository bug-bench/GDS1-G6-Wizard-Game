using UnityEngine;

/// <summary>
/// 单发弹道（火球 / 自动枪 / 狙击等），数值在 SpellProjectile 预制体与 SpellData 冷却上配置。
/// </summary>
public class BasicProjectileSpell : SpellBehavior
{
    [Header("弹道")]
    public GameObject projectilePrefab;

    [Tooltip("沿发射朝向前移，减少出生在玩家碰撞体内部的概率。")]
    public float spawnForwardInset = 0.22f;

    public override void Execute(GameObject caster, Transform firePoint)
    {
        Vector3 pos = firePoint.position + firePoint.up * spawnForwardInset;
        GameObject proj = Instantiate(projectilePrefab, pos, firePoint.rotation);
        SpellProjectile.RegisterWithCaster(proj, caster);
        Destroy(gameObject);
    }
}
