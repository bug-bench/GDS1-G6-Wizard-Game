using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 1f;
    public float lifeTime = 3f;

    // 用来记录发射这个火球的玩家是谁
    [HideInInspector] public GameObject caster;

    /// <summary>
    /// 生成后极短时间内忽略与施法者碰撞（Overlap 时 Trigger 可能同一帧多次触发）。
    /// </summary>
    [HideInInspector] public float ignoreCasterUntilTime;

    /// <summary>
    /// 地上的 SpellPickup 预制体若误挂了本脚本，Start 里的 Destroy 会把整个拾取物删掉；这里直接当作装饰品关掉弹道逻辑。
    /// </summary>
    bool IsUnderSpellPickup => GetComponentInParent<SpellPickup>() != null;

    void Awake()
    {
        if (IsUnderSpellPickup)
            enabled = false;
    }

    void Start()
    {
        if (IsUnderSpellPickup) return;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (IsUnderSpellPickup) return;
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    static bool IsColliderOnCaster(GameObject casterRoot, Collider2D col)
    {
        if (casterRoot == null || col == null) return false;
        Transform t = col.transform;
        return t == casterRoot.transform || t.IsChildOf(casterRoot.transform);
    }

    /// <summary>
    /// 不用 CompareTag：项目里若未在 Tag 列表添加 "Wall"/"Player"，CompareTag 会报错刷屏甚至卡死。
    /// </summary>
    static bool HasTag(Collider2D col, string tagName)
    {
        return col != null && col.gameObject.tag == tagName;
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (IsUnderSpellPickup) return;

        // caster 未设置时，以前会误判成「可以打 Player」，导致立刻自伤
        if (caster == null)
        {
            if (HasTag(hitInfo, "Player"))
                Destroy(gameObject);
            return;
        }

        if (Time.time < ignoreCasterUntilTime && IsColliderOnCaster(caster, hitInfo))
            return;

        // 碰撞体在玩家子物体上时 gameObject != caster 根节点，必须用层级判断否则会打到自己
        if (IsColliderOnCaster(caster, hitInfo)) return;

        if (HasTag(hitInfo, "Player"))
        {
            PlayerCombat target = hitInfo.GetComponent<PlayerCombat>()
                ?? hitInfo.GetComponentInParent<PlayerCombat>();
            // 双保险：即使 IgnoreCollision 漏了某个碰撞体，也不打施法者本人
            if (target != null && target.gameObject != caster)
            {
                float totalDamage = damage;

                PlayerStats casterStats = caster.GetComponent<PlayerStats>();
                if (casterStats != null)
                {
                    totalDamage += casterStats.strength;
                }

                target.TakeDamage(Mathf.RoundToInt(totalDamage));
                Destroy(gameObject);
            }
        }
        else if (HasTag(hitInfo, "Wall"))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 子物体上火球也要认主；并与施法者所有 Collider2D 做 IgnoreCollision，防止出生点重叠瞬伤。
    /// </summary>
    public static void RegisterWithCaster(GameObject projectileRoot, GameObject casterRoot, float casterIgnoreSeconds = 0.15f)
    {
        if (projectileRoot == null || casterRoot == null) return;

        float until = Time.time + casterIgnoreSeconds;
        Collider2D[] casterCols = casterRoot.GetComponentsInChildren<Collider2D>(true);
        // 必须从弹道根物体收集：Collider 常在父节点、SpellProjectile 在子节点，用 sp.GetComponentsInChildren 会漏掉根上的碰撞体
        Collider2D[] projCols = projectileRoot.GetComponentsInChildren<Collider2D>(true);

        foreach (SpellProjectile sp in projectileRoot.GetComponentsInChildren<SpellProjectile>(true))
        {
            sp.caster = casterRoot;
            sp.ignoreCasterUntilTime = until;
        }

        foreach (Collider2D pc in projCols)
        {
            if (pc == null) continue;
            foreach (Collider2D cc in casterCols)
            {
                if (cc == null) continue;
                Physics2D.IgnoreCollision(pc, cc, true);
            }
        }
    }
}
