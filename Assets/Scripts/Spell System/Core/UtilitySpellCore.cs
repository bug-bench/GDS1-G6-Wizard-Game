using UnityEngine;

public abstract class UtilitySpellCore : SpellBehavior
{
    protected Rigidbody2D RB { get; private set; }

    protected virtual void Awake()
    {
        // intentionally empty
    }

    public override void Initialize(
        GameObject caster,
        Transform firePoint)
    {
        base.Initialize(
            caster,
            firePoint);

        RB = caster.GetComponent<Rigidbody2D>();
    }

    protected PlayerCombat Combat =>
        caster != null
            ? caster.GetComponent<PlayerCombat>()
            : null;

    protected PlayerStats Stats =>
        casterStats;
}