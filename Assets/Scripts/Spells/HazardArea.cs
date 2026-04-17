using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 通用地面陷阱/区域脚本。
/// 只要挂载在包含 Trigger Collider2D 的物体上，就能对进入的玩家造成伤害、挂异常状态。
/// Generic hazard area script. Deals damage or applies status effects to players inside the trigger.
/// </summary>
public class HazardArea : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("每秒造成的持续伤害 — Damage per second while inside the area.")]
    public int damagePerSecond = 0;
    
    [Tooltip("进入区域时瞬间造成的单次伤害 — Instant damage applied once upon entering.")]
    public int instantDamageOnEnter = 0;

    [Header("Status Effects")]
    [Tooltip("进入区域时附加的燃烧总伤害（离开区域后仍会持续掉血） — Total burn damage applied on enter.")]
    public int applyBurnDamage = 0;
    [Tooltip("燃烧持续时间 — Burn duration.")]
    public float burnDuration = 3f;

    [Tooltip("进入区域时附加的眩晕时间（无法移动、无法施法） — Stun duration applied on enter.")]
    public float applyStunDuration = 0f;

    [Tooltip("进入区域时附加的定身时间（无法移动，但能施法） — Root duration applied on enter.")]
    public float applyRootDuration = 0f;

    [Tooltip("在区域内时临时降低的速度百分比（0~1，例如0.4表示减速40%） — Speed reduction percentage while inside.")]
    [Range(0f, 1f)]
    public float speedReductionPercentage = 0f;

    [Header("Lifetime")]
    [Tooltip("区域存在多长时间后自动销毁（0表示永久存在） — Destroy area after X seconds (0 = infinite).")]
    public float lifetime = 0f;

    // 记录在区域内的玩家，用于持续伤害
    private Dictionary<GameObject, float> playersInside = new Dictionary<GameObject, float>();
    // 记录每个玩家在区域内被扣除的具体速度值，以便离开时精准恢复
    private Dictionary<GameObject, float> speedDrops = new Dictionary<GameObject, float>();

    void Start()
    {
        if (lifetime > 0f)
        {
            Destroy(gameObject, lifetime);
        }
    }

    void Update()
    {
        if (damagePerSecond <= 0) return;

        // 处理持续伤害 (DoT)
        var keys = new List<GameObject>(playersInside.Keys);
        foreach (var player in keys)
        {
            if (player == null || !player.activeInHierarchy)
            {
                playersInside.Remove(player);
                continue;
            }

            if (Time.time >= playersInside[player])
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null && stats.IsAliveArena)
                {
                    // 直接扣血，不触发受击无敌帧
                    stats.health -= damagePerSecond;
                    stats.health = Mathf.Clamp(stats.health, 0f, float.MaxValue);
                    
                    if (stats.health <= 0)
                    {
                        // 陷阱击杀算作环境击杀 (attackerIndex = -1)
                        stats.TakeDamage(0); // 借用 TakeDamage 触发死亡逻辑
                    }
                }
                // 更新下一次受伤害的时间（1秒后）
                playersInside[player] = Time.time + 1f;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject player = other.gameObject;
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats == null) return;

            // 记录进入时间，用于每秒持续伤害
            if (!playersInside.ContainsKey(player))
            {
                playersInside.Add(player, Time.time + 1f);
            }

            // 瞬间伤害
            if (instantDamageOnEnter > 0)
            {
                PlayerCombat combat = player.GetComponent<PlayerCombat>();
                if (combat != null)
                {
                    combat.TakeDamage(instantDamageOnEnter);
                }
            }

            // 附加异常状态
            if (applyBurnDamage > 0)
            {
                stats.ApplyBurn(applyBurnDamage, burnDuration);
            }
            if (applyStunDuration > 0f)
            {
                stats.ApplyStun(applyStunDuration);
            }
            if (applyRootDuration > 0f)
            {
                stats.ApplyRoot(applyRootDuration);
            }
            if (speedReductionPercentage > 0f && !speedDrops.ContainsKey(player))
            {
                float drop = stats.speed * speedReductionPercentage;
                stats.speed -= drop;
                speedDrops[player] = drop;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject player = other.gameObject;
            if (playersInside.ContainsKey(player))
            {
                playersInside.Remove(player);
            }

            // 离开区域时恢复速度
            if (speedDrops.TryGetValue(player, out float drop))
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.speed += drop;
                }
                speedDrops.Remove(player);
            }
        }
    }
}