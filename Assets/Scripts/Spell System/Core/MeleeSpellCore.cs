using System.Collections.Generic;
using UnityEngine;

// <summary>
// Base class for all melee spells.

// Handles the shared functionality for melee attacks including
// hit detection, damage, knockback and hit tracking.
// Override virtual methods to customise individual melee spells.
// </summary>
public abstract class MeleeSpellCore : SpellBehavior
{
    #region Inspector

    [Header("Combat")]
    public float damage = 20f;

    [Header("Hitbox")]
    [Tooltip("Radius of the melee hitbox.")]
    public float hitRadius = 0.8f;

    [Tooltip("Distance the hitbox is placed in front of the caster.")]
    public float hitOffset = 1f;

    [Header("Knockback")]
    public float knockbackForce = 40f;

    [Header("Layers")]
    [Tooltip("Layers that can be hit by this melee attack.")]
    public LayerMask hitLayer = ~0;

    #endregion

    #region Runtime

    // Tracks everything already hit during this attack so each target can only be damaged once.
    protected readonly HashSet<GameObject> hitTargets = new();

    #endregion

    #region Scaling

    // Applies player size scaling to the melee hitbox.
    protected virtual void ApplyScaling()
    {
        SpellStatScaling.ApplyMeleeHitboxScale(
            this,
            SizeScale);
    }

    #endregion

    #region Hit Detection

    // Checks for valid targets inside the melee hitbox.
    protected virtual void CheckHits()
    {
        Vector3 hitCenter =
            transform.position +
            transform.up * hitOffset;

        Collider2D[] colliders =
            Physics2D.OverlapCircleAll(
                hitCenter,
                hitRadius,
                hitLayer);

        foreach (Collider2D col in colliders)
        {
            ProcessHit(col);
        }
    }

    // Routes the collision to the correct handler.
    protected virtual void ProcessHit(
        Collider2D col)
    {
        HandleDestroyable(col);

        HandlePlayer(col);
    }

    #endregion

    #region Hit Handling

    // Applies damage to destroyable world objects.
    protected virtual void HandleDestroyable(
        Collider2D col)
    {
        destroyableObject destroyable =
            col.GetComponent<destroyableObject>()
            ?? col.GetComponentInParent<destroyableObject>();

        if (destroyable == null)
            return;

        GameObject root =
            destroyable.gameObject;

        if (hitTargets.Contains(root))
            return;

        hitTargets.Add(root);

        destroyable.takeDamage(
            damage + Strength);
    }

    // Applies damage and knockback to players.
    protected virtual void HandlePlayer(
        Collider2D col)
    {
        PlayerCombat target =
            col.GetComponent<PlayerCombat>()
            ?? col.GetComponentInParent<PlayerCombat>();

        if (target == null)
            return;

        if (target.gameObject == caster)
            return;

        GameObject root =
            target.gameObject;

        if (hitTargets.Contains(root))
            return;

        hitTargets.Add(root);

        Vector2 knockbackDirection =
            (root.transform.position -
             caster.transform.position)
            .normalized;

        if (knockbackDirection == Vector2.zero)
        {
            knockbackDirection =
                Random.insideUnitCircle.normalized;
        }

        target.TakeDamage(
            Mathf.RoundToInt(damage + Strength),
            -1,
            knockbackDirection *
            knockbackForce);
    }

    #endregion

    #region Gizmos

    // Draws the melee hitbox in the Scene view.
    protected virtual void DrawHitboxGizmo()
    {
        Gizmos.color = Color.red;

        Vector3 center =
            transform.position +
            transform.up * hitOffset;

        Gizmos.DrawWireSphere(
            center,
            hitRadius);
    }

    #endregion
}