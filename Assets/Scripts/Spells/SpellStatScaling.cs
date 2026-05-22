using UnityEngine;

/// <summary>
/// 将 PlayerStats 的 Focus（冷却缩减）与 Size（体积倍率）接到各法术。
/// Applies Focus (CDR) and Size (scale) from PlayerStats to individual spells.
/// </summary>
public static class SpellStatScaling
{
    public const float MaxCooldownReduction = 0.9f;

    public static float GetSizeScale(GameObject caster)
    {
        if (caster == null) return 1f;
        PlayerStats stats = caster.GetComponent<PlayerStats>();
        return GetSizeScale(stats);
    }

    public static float GetSizeScale(PlayerStats stats)
    {
        if (stats == null) return 1f;
        return Mathf.Max(1f, stats.sizeMultiplier);
    }

    public static float GetEffectiveCooldown(PlayerStats stats, float baseCooldown)
    {
        if (baseCooldown <= 0f) return 0f;
        if (stats == null) return baseCooldown;
        float reduction = Mathf.Clamp(stats.cooldownReduction, 0f, MaxCooldownReduction);
        return baseCooldown * (1f - reduction);
    }

    /// <summary>火球 / 冰球等弹道：整体缩放 transform（碰撞体随比例变大）。</summary>
    public static void ApplyProjectileSize(GameObject projectileRoot, GameObject caster)
    {
        if (projectileRoot == null) return;
        float scale = GetSizeScale(caster);
        if (scale <= 1.001f) return;

        foreach (SpellProjectile sp in projectileRoot.GetComponentsInChildren<SpellProjectile>(true))
        {
            if (sp == null) continue;
            sp.transform.localScale *= scale;
        }
    }

    public static void ApplyProjectileSizeToTree(GameObject spellRoot, GameObject caster)
    {
        if (spellRoot == null) return;
        ApplyProjectileSize(spellRoot, caster);
    }

    public static void ApplyMeleeHitboxScale(MeleeSwingSpell melee, float scale)
    {
        if (melee == null || scale <= 1.001f) return;
        melee.hitRadius *= scale;
        melee.hitOffset *= scale;

        SpriteRenderer sr = melee.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.transform != melee.transform)
            sr.transform.localScale *= scale;
    }

    public static void ApplyLaserWidth(LineRenderer line, float baseStartWidth, float baseEndWidth, float scale)
    {
        if (line == null || scale <= 1.001f) return;
        line.startWidth = baseStartWidth * scale;
        line.endWidth = baseEndWidth * scale;
    }
}
