using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class FlameAfterImage : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 8f;
    public float knockbackForce = 0f;

    [Header("Burn")]
    public int burnDamage = 3;
    public float burnDuration = 2f;

    [Header("Timing")]
    public float lifetime = 0.6f;

    [Tooltip("How often each player can be damaged.")]
    public float damageInterval = 0.25f;

    private GameObject caster;

    private readonly Dictionary<PlayerCombat, float> nextDamageTime =
        new Dictionary<PlayerCombat, float>();

    public void Initialize(GameObject owner)
    {
        caster = owner;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (IsCaster(other))
            return;

        PlayerCombat combat =
            other.GetComponent<PlayerCombat>() ??
            other.GetComponentInParent<PlayerCombat>();

        if (combat != null)
        {
            DamagePlayer(combat);
            return;
        }

        destroyableObject crate =
            other.GetComponent<destroyableObject>() ??
            other.GetComponentInParent<destroyableObject>();

        if (crate != null)
        {
            crate.takeDamage(GetTotalDamage());
        }
    }

    bool IsCaster(Collider2D other)
    {
        if (caster == null)
            return false;

        Transform t = other.transform;

        return t == caster.transform ||
               t.IsChildOf(caster.transform);
    }

    void DamagePlayer(PlayerCombat target)
    {
        if (target == null)
            return;

        if (target.IsInvincible)
            return;

        if (ReflectShieldSpell.HasActiveShieldOn(target))
            return;

        if (nextDamageTime.TryGetValue(target, out float nextTime))
        {
            if (Time.time < nextTime)
                return;
        }

        nextDamageTime[target] =
            Time.time + damageInterval;

        int attackerIndex = -1;

        if (caster != null)
        {
            PlayerInput input =
                caster.GetComponent<PlayerInput>();

            if (input != null)
                attackerIndex = input.playerIndex;
        }

        Vector2 knockDirection =
            (target.transform.position -
             transform.position).normalized;

        target.TakeDamage(
            Mathf.RoundToInt(GetTotalDamage()),
            attackerIndex,
            knockDirection * knockbackForce);

        PlayerStats stats =
            target.GetComponent<PlayerStats>();

        if (stats != null && burnDamage > 0)
        {
            stats.ApplyBurn(
                burnDamage,
                burnDuration,
                0.5f,
                attackerIndex);
        }
    }

    float GetTotalDamage()
    {
        float total = damage;

        if (caster != null)
        {
            PlayerStats stats =
                caster.GetComponent<PlayerStats>();

            if (stats != null)
                total += stats.strength;
        }

        return total;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Collider2D col =
            GetComponent<Collider2D>();

        if (col is CircleCollider2D circle)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(
                transform.position +
                (Vector3)circle.offset,
                circle.radius *
                transform.lossyScale.x);
        }
    }
#endif
}