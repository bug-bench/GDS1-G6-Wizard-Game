using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeSpellCore : SpellBehavior
{
    [Header("Combat")]
    public float damage = 20f;

    [Header("Hitbox")]
    public float hitRadius = 0.8f;
    public float hitOffset = 1f;

    [Header("Knockback")]
    public float knockbackForce = 40f;

    [Header("Layers")]
    public LayerMask hitLayer = ~0;

    protected readonly HashSet<GameObject>
        hitTargets = new();

    protected virtual void ApplyScaling()
    {
        SpellStatScaling.ApplyMeleeHitboxScale(
            this,
            SizeScale);
    }

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

    protected virtual void ProcessHit(
        Collider2D col)
    {
        HandleDestroyable(col);

        HandlePlayer(col);
    }

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
}