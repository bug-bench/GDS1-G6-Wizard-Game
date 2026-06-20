using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerEffectsSystem : MonoBehaviour
{
    [Header("References")]
    public Volume postProcessVolume;

    [System.Serializable]
    public class EffectProfile
    {
        public string effectName;
        public bool   active;

        [Header("Post Processing")]
        public bool  useLensDistortion;
        public float lensDistortionStrength;

        public bool  useChromaticAberration;
        public float chromaticAberrationStrength;

        public bool  useVignette;
        public float vignetteIntensity;
        public Color vignetteColor = Color.black;

        public bool  useColorAdjustments;
        public float saturation;
        public float contrast;
    }

    [Header("Effect Profiles")]
    public EffectProfile drunkEffect;
    public EffectProfile knockedEffect;

    private DrunkSystem    drunkSystem;
    private PlayerCombat   combat;
    private PlayerStats    stats;

    private LensDistortion      lensDistortion;
    private ChromaticAberration chromaticAberration;
    private Vignette            vignette;
    private ColorAdjustments    colorAdjustments;

    private Camera  playerCamera;
    private Vector3 cameraOriginalPos;

    // One-shot shake state
    private float shakeDuration      = 0f;
    private float shakeElapsed       = 0f;
    private float shakePeakIntensity = 0f;

    void Awake()
    {
        drunkSystem = GetComponent<DrunkSystem>();
        combat      = GetComponent<PlayerCombat>();
        stats       = GetComponent<PlayerStats>();
    }

    void Start()
    {
        SetupPostProcessing();
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera != null)
            cameraOriginalPos = playerCamera.transform.localPosition;
    }

    void SetupPostProcessing()
    {
        if (postProcessVolume == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) postProcessVolume = cam.GetComponent<Volume>();
        }
        if (postProcessVolume == null) return;

        postProcessVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

        lensDistortion      = postProcessVolume.profile.Add<LensDistortion>(true);
        chromaticAberration = postProcessVolume.profile.Add<ChromaticAberration>(true);
        vignette            = postProcessVolume.profile.Add<Vignette>(true);
        colorAdjustments    = postProcessVolume.profile.Add<ColorAdjustments>(true);

        postProcessVolume.weight = 0f;
    }

    public void ActivateForCamera()
    {
        if (postProcessVolume != null)
            postProcessVolume.weight = 1f;
    }

    public void DeactivateForCamera()
    {
        if (postProcessVolume != null)
            postProcessVolume.weight = 0f;
    }

    // Call this to trigger a one-shot shake burst
    public void TriggerShake(float intensity, float duration)
    {
        shakePeakIntensity = intensity;
        shakeDuration      = duration;
        shakeElapsed       = 0f;
    }

    void Update()
    {
        bool isKnocked = combat != null && combat.isKnockedDown;
        float drunkT   = drunkSystem != null ? drunkSystem.drunkness / 100f : 0f;

        ResetAllEffects();

        if (isKnocked)
            ApplyEffect(knockedEffect, 1f);
        else if (drunkT > 0f)
            ApplyEffect(drunkEffect, drunkT);

        UpdateShake();
    }

    void UpdateShake()
    {
        if (playerCamera == null) return;

        if (shakeElapsed >= shakeDuration)
        {
            playerCamera.transform.localPosition = cameraOriginalPos;
            return;
        }

        shakeElapsed += Time.deltaTime;

        float t         = 1f - (shakeElapsed / shakeDuration); // 1 → 0 fade
        float intensity = shakePeakIntensity * t;

        float x = Mathf.Sin(shakeElapsed * 80f) * intensity;
        float y = Mathf.Cos(shakeElapsed * 75f) * intensity;

        playerCamera.transform.localPosition = cameraOriginalPos + new Vector3(x, y, 0f);
    }

    void ApplyEffect(EffectProfile profile, float strength)
    {
        if (profile == null) return;

        if (lensDistortion != null && profile.useLensDistortion)
        {
            lensDistortion.active = true;
            lensDistortion.intensity.Override(profile.lensDistortionStrength * strength);
        }
        if (chromaticAberration != null && profile.useChromaticAberration)
        {
            chromaticAberration.active = true;
            chromaticAberration.intensity.Override(profile.chromaticAberrationStrength * strength);
        }
        if (vignette != null && profile.useVignette)
        {
            vignette.active = true;
            vignette.intensity.Override(profile.vignetteIntensity * strength);
            vignette.color.Override(profile.vignetteColor);
        }
        if (colorAdjustments != null && profile.useColorAdjustments)
        {
            colorAdjustments.active = true;
            colorAdjustments.saturation.Override(profile.saturation * strength);
            colorAdjustments.contrast.Override(profile.contrast * strength);
        }
    }

    void ResetAllEffects()
    {
        if (lensDistortion      != null) { lensDistortion.active = false;      lensDistortion.intensity.Override(0f); }
        if (chromaticAberration != null) { chromaticAberration.active = false; chromaticAberration.intensity.Override(0f); }
        if (vignette            != null) { vignette.active = false;            vignette.intensity.Override(0f); }
        if (colorAdjustments    != null) { colorAdjustments.active = false;    colorAdjustments.saturation.Override(0f); colorAdjustments.contrast.Override(0f); }
    }

    void OnDestroy()
    {
        ResetAllEffects();
        if (playerCamera != null)
            playerCamera.transform.localPosition = cameraOriginalPos;
    }

    public bool IsEffectActive(string effectName)
    {
        if (effectName == "Drunk")   return drunkSystem != null && drunkSystem.drunkness > 0f;
        if (effectName == "Knocked") return combat != null && combat.isKnockedDown;
        return false;
    }

    public float GetEffectStrength(string effectName)
    {
        if (effectName == "Drunk")   return drunkSystem != null ? drunkSystem.drunkness / 100f : 0f;
        if (effectName == "Knocked") return combat != null && combat.isKnockedDown ? 1f : 0f;
        return 0f;
    }
}