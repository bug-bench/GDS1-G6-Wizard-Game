using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 按 spellName 集中配置施法/松键音效；SpellData 未指定 clip 时作为回退。
/// Central cast/release clips by spellName when SpellData has no clip assigned.
/// </summary>
[CreateAssetMenu(fileName = "SpellAudioLibrary", menuName = "Game/Spell Audio Library", order = 1)]
public class SpellAudioLibrary : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string spellName;
        public SpellCastAudioSettings castAudio;
        public SpellCastAudioSettings releaseAudio;
    }

    public Entry[] entries = Array.Empty<Entry>();

    Dictionary<string, Entry> lookup;

    void OnEnable() => RebuildLookup();

    public void RebuildLookup()
    {
        lookup = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        if (entries == null) return;
        foreach (Entry e in entries)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.spellName)) continue;
            lookup[e.spellName.Trim()] = e;
        }
    }

    public bool TryGetCast(string spellName, out SpellCastAudioSettings settings)
    {
        settings = default;
        if (string.IsNullOrWhiteSpace(spellName)) return false;
        EnsureLookup();
        if (!lookup.TryGetValue(spellName.Trim(), out Entry e) || e == null) return false;
        if (!e.castAudio.IsValid) return false;
        settings = e.castAudio;
        return true;
    }

    public bool TryGetRelease(string spellName, out SpellCastAudioSettings settings)
    {
        settings = default;
        if (string.IsNullOrWhiteSpace(spellName)) return false;
        EnsureLookup();
        if (!lookup.TryGetValue(spellName.Trim(), out Entry e) || e == null) return false;
        if (!e.releaseAudio.IsValid) return false;
        settings = e.releaseAudio;
        return true;
    }

    void EnsureLookup()
    {
        if (lookup == null) RebuildLookup();
    }
}
