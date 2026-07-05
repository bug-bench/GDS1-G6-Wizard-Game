using UnityEngine;

/// <summary>
/// 玩家施法音效管理：施法瞬间 / 按住类松键；支持 SpellData、Library、预制体三层配置。
/// Manages cast and hold-release sounds via SpellData, SpellAudioLibrary, or prefab SpellCastAudio.
/// </summary>
[DisallowMultipleComponent]
public class SpellAudioSystem : MonoBehaviour
{
    [Header("Library (optional)")]
    [Tooltip("全局按 spellName 回退 — Optional fallback by spell name.")]
    public SpellAudioLibrary library;

    [Header("Output")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Tooltip("同时播放的最大 OneShot 数 — Max overlapping one-shots.")]
    [Min(1)]
    public int castSourcePoolSize = 3;

    AudioSource[] castSources;
    int castSourceIndex;
    AudioSource releaseSource;

    void Awake()
    {
        EnsureSources();
    }

    void EnsureSources()
    {
        if (castSources != null && castSources.Length > 0 && releaseSource != null)
            return;

        var existing = GetComponents<AudioSource>();
        int needed = castSourcePoolSize + 1;
        if (existing.Length < needed)
        {
            for (int i = existing.Length; i < needed; i++)
                gameObject.AddComponent<AudioSource>();
            existing = GetComponents<AudioSource>();
        }

        castSources = new AudioSource[castSourcePoolSize];
        for (int i = 0; i < castSourcePoolSize; i++)
            castSources[i] = ConfigureSource(existing[i]);

        releaseSource = ConfigureSource(existing[castSourcePoolSize]);
    }

    static AudioSource ConfigureSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        return source;
    }

    /// <summary>施法成功时调用（按下攻击/移动键且通过 CD 检测后）。</summary>
    public void PlayCast(SpellData data, Transform firePoint, GameObject spawnedSpellInstance = null)
    {
        if (!TryResolveCast(data, spawnedSpellInstance, out SpellCastAudioSettings settings))
            return;

        Vector3? worldPos = firePoint != null ? firePoint.position : (Vector3?)null;
        Play(settings, GetNextCastSource(), worldPos);
    }

    /// <summary>按住类技能结束（松键或达到最长按住时间）时调用。</summary>
    public void PlayRelease(SpellData data, Transform firePoint, GameObject spawnedSpellInstance = null)
    {
        if (!TryResolveRelease(data, spawnedSpellInstance, out SpellCastAudioSettings settings))
            return;

        Vector3? worldPos = firePoint != null ? firePoint.position : (Vector3?)null;
        Play(settings, releaseSource, worldPos);
    }

    public bool TryResolveCast(SpellData data, GameObject spawnedSpellInstance, out SpellCastAudioSettings settings)
    {
        settings = default;
        if (data == null) return false;

        if (data.castAudio.IsValid)
        {
            settings = data.castAudio;
            return true;
        }

        if (library != null && library.TryGetCast(data.spellName, out settings))
            return true;

        if (TryGetPrefabAudio(data.spellPrefab, spawnedSpellInstance, true, out settings))
            return true;

        return false;
    }

    public bool TryResolveRelease(SpellData data, GameObject spawnedSpellInstance, out SpellCastAudioSettings settings)
    {
        settings = default;
        if (data == null) return false;

        if (data.releaseAudio.IsValid)
        {
            settings = data.releaseAudio;
            return true;
        }

        if (library != null && library.TryGetRelease(data.spellName, out settings))
            return true;

        if (TryGetPrefabAudio(data.spellPrefab, spawnedSpellInstance, false, out settings))
            return true;

        return false;
    }

    static bool TryGetPrefabAudio(GameObject spellPrefab, GameObject instance, bool cast, out SpellCastAudioSettings settings)
    {
        settings = default;
        SpellCastAudio audio = null;

        if (instance != null)
            audio = instance.GetComponentInChildren<SpellCastAudio>(true);

        if (audio == null && spellPrefab != null)
            audio = spellPrefab.GetComponentInChildren<SpellCastAudio>(true);

        if (audio == null) return false;

        SpellCastAudioSettings candidate = cast ? audio.castAudio : audio.releaseAudio;
        if (!candidate.IsValid) return false;
        settings = candidate;
        return true;
    }

    AudioSource GetNextCastSource()
    {
        EnsureSources();
        AudioSource source = castSources[castSourceIndex];
        castSourceIndex = (castSourceIndex + 1) % castSources.Length;
        return source;
    }

    void Play(SpellCastAudioSettings settings, AudioSource source, Vector3? firePointWorldPos)
    {
        if (!settings.IsValid || source == null) return;

        float pitch = settings.pitchMin >= settings.pitchMax
            ? settings.pitchMin
            : Random.Range(settings.pitchMin, settings.pitchMax);
        float volume = Mathf.Clamp01(settings.volume * masterVolume);

        if (settings.playAtWorldPosition && firePointWorldPos.HasValue)
        {
            PlayClipAtWorldPoint(settings.clip, firePointWorldPos.Value, volume, pitch, settings.spatialBlend);
            return;
        }

        source.pitch = pitch;
        source.spatialBlend = settings.spatialBlend;
        source.PlayOneShot(settings.clip, volume);
    }

    static void PlayClipAtWorldPoint(AudioClip clip, Vector3 position, float volume, float pitch, float spatialBlend)
    {
        if (clip == null) return;

        var go = new GameObject("SpellCastAudio_World");
        go.transform.position = position;
        AudioSource temp = go.AddComponent<AudioSource>();
        temp.clip = clip;
        temp.volume = volume;
        temp.pitch = pitch;
        temp.spatialBlend = spatialBlend;
        temp.playOnAwake = false;
        temp.Play();
        Object.Destroy(go, clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch)) + 0.05f);
    }
}
