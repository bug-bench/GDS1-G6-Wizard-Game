using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// <summary>
// Base class for all hit scan spells.

// Handles the shared functionality for instant-hit spells including
// raycasting, damage, knockback and collision handling.
// Override virtual methods to customise individual hit scan spells.
// </summary>
public abstract class HitScanSpellCore : SpellBehavior
{
    #region Inspector

    [Header("Damage")]
    public float damage = 25f;

    [Header("Range")]
    public float range = 100f;

    [Header("Knockback")]
    public float knockbackForce = 20f;

    [Header("Raycast")]
    public LayerMask layerMask;

    [Tooltip("Moves the raycast slightly forward to prevent hitting the caster.")]
    public float castStartInset = 0.1f;

    #endregion

    #region Runtime

    // Stores every point the ray travels through.
    // Used for visual effects such as lasers or beam renderers.
    protected readonly List<Vector3> hitPoints =
        new List<Vector3>();

    #endregion

    #region Execution

    public override void Execute()
    {
        hitPoints.Clear();

        PerformHitScan();

        OnHitScanFinished(hitPoints);

        Destroy(gameObject);
    }

    #endregion

    #region Hit Scan

    // Performs the raycast and records every point hit.
    // Override this to support bouncing or piercing lasers.
    protected virtual void PerformHitScan()
    {
        Vector2 direction =
            firePoint.up.normalized;

        Vector2 start =
            (Vector2)firePoint.position +
            direction * castStartInset;

        float remainingRange =
            range - castStartInset;

        hitPoints.Add(start);

        RaycastHit2D hit =
            Physics2D.Raycast(
                start,
                direction,
                remainingRange,
                layerMask);

        if (!hit.collider)
        {
            hitPoints.Add(
                start + direction * remainingRange);

            return;
        }

        hitPoints.Add(hit.point);

        ProcessHit(hit, direction);
    }

    #endregion

    #region Hit Processing

    // Routes the hit to the appropriate handler.
    protected virtual void ProcessHit(
        RaycastHit2D hit,
        Vector2 direction)
    {
        HandleDestroyable(hit);

        HandlePlayer(hit, direction);
    }

    // Applies damage to destroyable world objects.
    protected virtual void HandleDestroyable(
        RaycastHit2D hit)
    {
        destroyableObject destroyable =
            hit.collider.GetComponent<destroyableObject>()
            ?? hit.collider.GetComponentInParent<destroyableObject>();

        if (destroyable == null)
            return;

        destroyable.takeDamage(
            damage + Strength);
    }

    // Applies damage and knockback to players.
    protected virtual void HandlePlayer(
        RaycastHit2D hit,
        Vector2 direction)
    {
        PlayerCombat target =
            hit.collider.GetComponent<PlayerCombat>()
            ?? hit.collider.GetComponentInParent<PlayerCombat>();

        if (target == null)
            return;

        if (target.gameObject == caster)
            return;

        if (target.IsInvincible)
            return;

        int attackerIndex = -1;

        PlayerInput input =
            caster.GetComponent<PlayerInput>();

        if (input != null)
            attackerIndex = input.playerIndex;

        target.TakeDamage(
            Mathf.RoundToInt(damage + Strength),
            attackerIndex,
            direction * knockbackForce);
    }

    #endregion

    #region Virtual Methods

    // Called after the hit scan completes.
    // Used for visuals such as lasers, trails or beam effects.
    protected abstract void OnHitScanFinished(
        List<Vector3> hitPoints);

    #endregion
}