using System;
using UnityEngine;

/// <summary>
/// 单次施法/松键音效的参数（可挂在 SpellData 或法术预制体上）。
/// Parameters for one cast or release sound (on SpellData or spell prefab).
/// </summary>
[Serializable]
public class SpellCastAudioSettings
{
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("随机音高范围 — Random pitch range.")]
    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;

    [Tooltip("在 firePoint 世界坐标播放（否则为 2D 玩家音源）。 — Play at firePoint world position; otherwise 2D on player source.")]
    public bool playAtWorldPosition;

    [Range(0f, 1f)]
    public float spatialBlend = 0.35f;

    public bool IsValid => clip != null;
}
