using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SceneMusic
{
    public string sceneName;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume      = 1f;

    [Header("Loop Points (seconds) — 0 = disabled)")]
    public float loopStart   = 0f;  // where to jump back to
    public float loopEnd     = 0f;  // where to jump from (0 = use full clip)

    [Header("Outro")]
    public float outroStart  = 0f;  // when timer hits 0, jump here and play to end
    public bool  hasOutro    = false;
}

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource sourceA;
    public AudioSource sourceB;

    [Header("Scene Music")]
    public SceneMusic[] sceneTracks;

    [Header("Crossfade")]
    public float crossfadeDuration = 1.5f;

    [Header("Loop Crossfade")]
    public float loopCrossfadeDuration = 0.5f; // 50ms — barely audible    

    [Header("Intensity")]
    public float intensifyStartTime = 30f;
    public float maxPitch           = 1.1f;

    private AudioSource activeSource;
    private AudioSource inactiveSource;
    private float timeRemaining     = -1f;
    private SceneMusic currentTrack;
    private bool outroPlaying       = false;
    private Coroutine loopCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        activeSource   = sourceA;
        inactiveSource = sourceB;

        sourceA.loop   = false; // we handle looping manually
        sourceB.loop   = false;
        sourceB.volume = 0f;
    }

    void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        outroPlaying  = false;
        timeRemaining = -1f;
        ResetPitch();
        PlayForScene(scene.name);
    }

    void Update()
    {
        if (timeRemaining < 0f || outroPlaying) return;

        // Pitch up in last 15 seconds
        if (timeRemaining <= intensifyStartTime)
        {
            float t     = 1f - (timeRemaining / intensifyStartTime);
            float pitch = Mathf.Lerp(1f, maxPitch, t);
            activeSource.pitch   = pitch;
            inactiveSource.pitch = pitch;
        }

        // Timer hit 0 — play outro
        if (timeRemaining <= 3f && currentTrack != null && currentTrack.hasOutro)
            TriggerOutro();
    }

    public void SetTimeRemaining(float seconds)
    {
        timeRemaining = seconds;
    }

    public void ResetPitch()
    {
        activeSource.pitch   = 1f;
        inactiveSource.pitch = 1f;
    }

    public void TriggerOutro()
    {
        if (outroPlaying || currentTrack == null || !currentTrack.hasOutro) return;
        outroPlaying = true;

        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }

        // Jump to outro position and play to end
        activeSource.time = currentTrack.outroStart;
        activeSource.loop = false;

        // After outro ends, crossfade to menu/voting music
        StartCoroutine(WaitForOutroThenCrossfade());
    }

    IEnumerator WaitForOutroThenCrossfade()
    {
        float outroLength = activeSource.clip.length - currentTrack.outroStart;
        // Account for pitch — higher pitch = faster playback
        float waitTime = outroLength / activeSource.pitch;
        yield return new WaitForSeconds(waitTime);

        // Crossfade to menu music
        SceneMusic menuTrack = System.Array.Find(
            sceneTracks, t => t.sceneName == "MainMenu"
        );
        if (menuTrack != null)
            StartCoroutine(Crossfade(menuTrack.clip, menuTrack.volume, menuTrack));
    }

    public void PlayForScene(string sceneName)
    {
        SceneMusic track = System.Array.Find(
            sceneTracks, t => t.sceneName == sceneName
        );
        if (track == null) return;
        if (activeSource.clip == track.clip) return;

        StartCoroutine(Crossfade(track.clip, track.volume, track));
    }

    IEnumerator Crossfade(AudioClip newClip, float newVolume, SceneMusic track)
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }

        inactiveSource.clip   = newClip;
        inactiveSource.volume = 0f;
        inactiveSource.pitch  = 1f;
        inactiveSource.time   = 0f;
        inactiveSource.loop   = false;
        inactiveSource.Play();

        float elapsed     = 0f;
        float startVolume = activeSource.volume;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / crossfadeDuration;

            activeSource.volume   = Mathf.Lerp(startVolume, 0f,        t);
            inactiveSource.volume = Mathf.Lerp(0f,          newVolume, t);

            yield return null;
        }

        activeSource.Stop();
        activeSource.clip   = null;
        activeSource.volume = 0f;

        (activeSource, inactiveSource) = (inactiveSource, activeSource);

        currentTrack = track;

        // Start manual loop if track has loop points
        if (track != null && track.loopEnd > track.loopStart)
            loopCoroutine = StartCoroutine(LoopSection(track));
    }

    IEnumerator LoopSection(SceneMusic track)
    {
        while (true)
        {
            if (!outroPlaying && activeSource.time >= track.loopEnd - loopCrossfadeDuration)
            {
                // Start crossfade to loop start
                float elapsed = 0f;
                float startVol = activeSource.volume;

                // Set up inactive source at loop start
                inactiveSource.clip   = activeSource.clip;
                inactiveSource.volume = 0f;
                inactiveSource.pitch  = activeSource.pitch;
                inactiveSource.time   = track.loopStart;
                inactiveSource.Play();

                while (elapsed < loopCrossfadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t  = elapsed / loopCrossfadeDuration;

                    activeSource.volume   = Mathf.Lerp(startVol, 0f,       t);
                    inactiveSource.volume = Mathf.Lerp(0f,       startVol, t);

                    yield return null;
                }

                activeSource.Stop();
                activeSource.volume = 0f;
                (activeSource, inactiveSource) = (inactiveSource, activeSource);
            }

            yield return null;
        }
    }

    IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed     = 0f;

        while (elapsed < duration)
        {
            elapsed       += Time.deltaTime;
            source.volume  = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.Stop();
        source.volume = 0f;
    }
}