using UnityEngine;

/// <summary>
/// 挂在法术 spellPrefab 上：SpellData 与 Library 都未配置时，用这里的音效。
/// On spell prefab when SpellData and Library have no clips.
/// </summary>
public class SpellCastAudio : MonoBehaviour
{
    public SpellCastAudioSettings castAudio;
    public SpellCastAudioSettings releaseAudio;
}
