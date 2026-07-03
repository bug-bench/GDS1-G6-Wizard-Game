using UnityEngine;

// <summary>
// Base class for every spell in Wisest Wizardry.

// Provides the shared functionality used by all spell types including
// initialization, caster references and hold-based spell support.
// </summary>
public abstract class SpellBehavior : MonoBehaviour
{
    #region Inspector

    [Header("Hold Settings")]

    [Tooltip("Maximum time a hold spell can remain active. Set to 0 for unlimited.")]
    public float maxHoldDuration = 0f;

    #endregion

    #region Runtime

    // The player who cast this spell.
    protected GameObject caster;

    // The fire point used when spawning or aiming the spell.
    protected Transform firePoint;

    // Cached PlayerStats component of the caster.
    protected PlayerStats casterStats;

    // Time the spell began being held.
    private float holdStartTime;

    #endregion

    #region Properties

    // Cached strength value used by combat spells.
    protected float Strength =>
        casterStats != null
            ? casterStats.strength
            : 0f;

    // Current spell size multiplier based on player stats.
    protected float SizeScale =>
        SpellStatScaling.GetSizeScale(caster);

    #endregion

    #region Initialization

    // Initializes the spell with its caster and fire point.
    public virtual void Initialize(
        GameObject caster,
        Transform firePoint)
    {
        this.caster = caster;
        this.firePoint = firePoint;
        this.casterStats = caster.GetComponent<PlayerStats>();
    }

    #endregion

    #region Execution

    // Executes the spell.
    public abstract void Execute();

    // Stops a hold-based spell.
    public virtual void StopExecute()
    {
    }

    #endregion

    #region Hold Duration

    // Starts tracking how long the spell has been held.
    public void BeginHoldDurationTracking()
    {
        holdStartTime = Time.time;
    }

    // Returns true if the maximum hold duration has been reached.
    public bool IsHoldDurationExceeded()
    {
        return maxHoldDuration > 0f &&
               Time.time - holdStartTime >= maxHoldDuration;
    }

    #endregion
}