using UnityEngine;

public abstract class SpellBehavior : MonoBehaviour
{
    [Header("Hold Settings")]
    public float maxHoldDuration = 0f;

    protected GameObject caster;
    protected Transform firePoint;
    protected PlayerStats casterStats;

    private float holdStartTime;

    public virtual void Initialize(
        GameObject caster,
        Transform firePoint)
    {
        this.caster = caster;
        this.firePoint = firePoint;
        this.casterStats = caster.GetComponent<PlayerStats>();
    }

    public abstract void Execute();

    public virtual void StopExecute()
    {
    }

    public void BeginHoldDurationTracking()
    {
        holdStartTime = Time.time;
    }

    public bool IsHoldDurationExceeded()
    {
        return maxHoldDuration > 0f &&
               Time.time - holdStartTime >= maxHoldDuration;
    }

    protected float Strength =>
        casterStats != null
            ? casterStats.strength
            : 0f;

    protected float SizeScale =>
        SpellStatScaling.GetSizeScale(caster);
}