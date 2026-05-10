using System.Collections.Generic;
using UnityEngine;

public class MeleeSwingSpell : SpellBehavior
{
    [Header("Combat Settings")]
    public int damage = 20;
    public float swingDuration = 0.25f; // 挥棒持续时间
    [Tooltip("击退力。如果觉得不够猛，可以继续改大（比如 50 甚至 80）")]
    public float knockbackForce = 40f;  // 默认击退力加强
    
    [Tooltip("哪些层级的物体可以被打到？（务必勾选包含敌人/玩家的层）")]
    public LayerMask hitLayer = ~0; // 默认打所有层
    [Tooltip("武器的打击判定半径（红色圆圈）")]
    public float hitRadius = 0.8f;  
    [Tooltip("打击点距离手有多远？（决定红色圆圈的位置）")]
    public float hitOffset = 1.0f;

    [Header("Visual Settings")]
    [Tooltip("起手角度。如果挥反了，把正负号换一下，比如 100")]
    public float startAngle = 100f; 
    [Tooltip("结束角度。如果挥反了，把正负号换一下，比如 -90")]
    public float endAngle = -90f;

    private GameObject casterRef;
    private float timer;
    
    // 记录已经打到过的人，防止一棒子造成多次伤害
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    public override void Execute(GameObject caster, Transform firePoint)
    {
        casterRef = caster;
        
        // 1. 挂载到 firePoint 下，跟随玩家移动和朝向
        transform.SetParent(firePoint);
        transform.localPosition = Vector3.zero;

        // 自动修正贴图偏移（推出去）
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.gameObject != this.gameObject)
        {
            sr.transform.localPosition = new Vector3(0, hitOffset, 0);
        }
        else if (sr != null && sr.gameObject == this.gameObject)
        {
            Debug.LogWarning("你的棒球棍贴图和 MeleeSwingSpell 脚本挂在了同一个物体上！这会导致它原地打转。");
        }
        
        transform.localRotation = Quaternion.Euler(0, 0, startAngle);
        Destroy(gameObject, swingDuration);
    }

    private void Update()
    {
        if (casterRef == null) return;
        
        timer += Time.deltaTime;
        float t = timer / swingDuration;
        
        // 挥击的平滑动画
        float easeT = t * t * (3f - 2f * t);
        float currentAngle = Mathf.Lerp(startAngle, endAngle, easeT);
        transform.localRotation = Quaternion.Euler(0, 0, currentAngle);

        // 每帧主动进行球形伤害检测（最靠谱的近战检测方式，不需要挂Collider2D）
        CheckMeleeHit();
    }

    private void CheckMeleeHit()
    {
        // 算出棒球棍的打击中心点（沿着自身的上/Y方向推出去）
        Vector3 hitCenter = transform.position + transform.up * hitOffset;

        // 获取圆圈内的所有碰撞体
        Collider2D[] colliders = Physics2D.OverlapCircleAll(hitCenter, hitRadius, hitLayer);
        foreach (var col in colliders)
        {
            GameObject targetObj = col.gameObject;
            if (targetObj == casterRef) continue; // 不打自己

            // 找对方最根部的对象，确保只记录一次
            PlayerCombat targetCombat = col.GetComponentInParent<PlayerCombat>();
            if (targetCombat != null)
            {
                GameObject rootObj = targetCombat.gameObject;
                if (!hitTargets.Contains(rootObj))
                {
                    hitTargets.Add(rootObj); // 记录已命中，防止单次挥击重复扣血

                    // 击退方向
                    Vector2 knockbackDir = (rootObj.transform.position - casterRef.transform.position).normalized;
                    Vector2 knockbackVector = knockbackDir * knockbackForce;

                    targetCombat.TakeDamage(damage, -1, knockbackVector);
                }
            }
        }
    }

    private void Start()
    {
        if (casterRef != null)
        {
            PlayerCombat combat = casterRef.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                combat.HideWeaponVisualFor(swingDuration);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 在编辑器里画出攻击判定的红色圆圈，方便你调整大小和距离
        Gizmos.color = Color.red;
        Vector3 center = transform.position + transform.up * hitOffset;
        Gizmos.DrawWireSphere(center, hitRadius);
    }
}
