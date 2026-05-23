using UnityEngine;

/// <summary>
/// 在 Project 里创建资源后手动调 Size / Focus 等缩放；拖进场景的 SpellScalingConfigProvider。
/// Create via Assets → Create → Game → Spell Scaling Config, assign on SpellScalingConfigProvider in scene.
/// </summary>
[CreateAssetMenu(fileName = "SpellScalingConfig", menuName = "Game/Spell Scaling Config", order = 2)]
public class SpellScalingConfig : ScriptableObject
{
    [Header("Size 豆 → 技能体积")]
    [Range(0.05f, 1f)]
    [Tooltip("技能放大转化率。0.5 + 每豆0.1 → 约 20 颗到 2× 上限。\n公式：技能倍率 = 1 + (sizeMultiplier-1) × 本值，再受上限截断。")]
    public float sizeBonusStrength = 0.5f;

    [Range(1.05f, 3f)]
    [Tooltip("技能体积倍率上限（2 = 最多两倍大）。\nMax spell size scale (2 = double size).")]
    public float maxSizeScale = 2f;

    [Header("参考（与 SizeStat 预制体 amount 一致，仅预览用）")]
    [Tooltip("每颗 Size 豆给 sizeMultiplier +多少（改预制体 StatPickUp.amount）。\nPer-pickup add to sizeMultiplier; match SizeStat prefab.")]
    public float sizePickupAmountOnPrefab = 0.1f;

    /// <summary>调参预览：吃 n 颗 Size 豆后的约略技能倍率。</summary>
    public float PreviewSpellScaleAfterPickups(int pickupCount)
    {
        float mult = 1f + pickupCount * sizePickupAmountOnPrefab;
        float bonus = Mathf.Max(0f, mult - 1f);
        return Mathf.Min(maxSizeScale, 1f + bonus * sizeBonusStrength);
    }

    public float ComputeSpellScale(float sizeMultiplier)
    {
        float bonus = Mathf.Max(0f, sizeMultiplier - 1f);
        return Mathf.Min(maxSizeScale, 1f + bonus * sizeBonusStrength);
    }
}
