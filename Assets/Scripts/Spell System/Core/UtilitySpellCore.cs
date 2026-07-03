using UnityEngine;

// <summary>
// Base class for all utility spells.

// Provides shared functionality for spells that directly affect the caster,
// such as movement abilities, shields and other player-focused effects.
// </summary>
public abstract class UtilitySpellCore : SpellBehavior
{
    #region Runtime

    // Cached Rigidbody2D of the caster.
    protected Rigidbody2D RB { get; private set; }

    // Cached PlayerCombat component of the caster.
    /// </summary>
    protected PlayerCombat Combat =>
        caster != null
            ? caster.GetComponent<PlayerCombat>()
            : null;

    // Cached PlayerStats component of the caster.
    protected PlayerStats Stats =>
        casterStats;

    #endregion

    #region Unity Events

    protected virtual void Awake()
    {
        // Intentionally left empty.
        // Child utility spells can override if required.
    }

    #endregion

    #region Initialization

    // Caches commonly used components from the caster.
    public override void Initialize(
        GameObject caster,
        Transform firePoint)
    {
        base.Initialize(
            caster,
            firePoint);

        RB = caster.GetComponent<Rigidbody2D>();
    }

    #endregion
}