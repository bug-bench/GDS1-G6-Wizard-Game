using UnityEngine;

/// <summary>
/// 放在场景任意物体上（如 GameManager），指定全局 SpellScalingConfig。
/// Place in scene (e.g. on GameManager) and assign the config asset.
/// </summary>
public class SpellScalingConfigProvider : MonoBehaviour
{
    public static SpellScalingConfig Active { get; private set; }

    public SpellScalingConfig config;

    [Tooltip("无 Provider 时从 Resources/SpellScalingConfig 加载。\nLoad from Resources when no provider is present.")]
    public bool loadFromResourcesIfMissing = true;

    void Awake()
    {
        Apply();
    }

    void OnDestroy()
    {
        if (Active == config)
            Active = null;
    }

    public void Apply()
    {
        if (config != null)
        {
            Active = config;
            return;
        }

        if (loadFromResourcesIfMissing)
        {
            var res = Resources.Load<SpellScalingConfig>("SpellScalingConfig");
            if (res != null)
                Active = res;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (config != null && Application.isPlaying)
            Active = config;
    }
#endif
}
