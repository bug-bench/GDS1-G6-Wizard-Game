using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class HitScanSpellCore : SpellBehavior
{
    [Header("Damage")]
    public float damage = 25f;

    [Header("Range")]
    public float range = 100f;

    [Header("Knockback")]
    public float knockbackForce = 20f;

    [Header("Raycast")]
    public LayerMask layerMask;

    public float castStartInset = 0.1f;

    protected readonly List<Vector3> hitPoints =
        new List<Vector3>();

    public override void Execute()
    {
        hitPoints.Clear();

        PerformHitScan();

        OnHitScanFinished(hitPoints);

        Destroy(gameObject);
    }

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

    protected virtual void ProcessHit(
        RaycastHit2D hit,
        Vector2 direction)
    {
        HandleDestroyable(hit);

        HandlePlayer(hit, direction);
    }

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

    protected abstract void OnHitScanFinished(
        List<Vector3> hitPoints);
}