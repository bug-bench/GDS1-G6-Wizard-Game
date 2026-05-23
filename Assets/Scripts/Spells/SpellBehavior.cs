using UnityEngine;

public abstract class SpellBehavior : MonoBehaviour
{
    [Header("按住持续 / Hold Duration")]
    [Tooltip("单次按住激活的最长秒数，0 表示不限时 — Max seconds per hold activation; 0 = unlimited.")]
    public float maxHoldDuration = 0f;

    float holdStartTime;

    public abstract void Execute(GameObject caster, Transform firePoint);

    public virtual void StopExecute() { }

    /// <summary>由 PlayerCombat 在 Execute 之后调用，开始计时。 — Called after Execute to start the hold timer.</summary>
    public void BeginHoldDurationTracking()
    {
        holdStartTime = Time.time;
    }

    public bool IsHoldDurationExceeded()
    {
        return maxHoldDuration > 0f && Time.time - holdStartTime >= maxHoldDuration;
    }
}
