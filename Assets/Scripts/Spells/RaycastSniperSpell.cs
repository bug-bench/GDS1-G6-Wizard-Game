using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 狙击「激光」：瞬间 Raycast2D 判伤 + LineRenderer 画一道线。
/// 预制体需挂 LineRenderer（至少 2 个 position），材质可用 Default-Line 或自定义激光材质。
/// Sniper laser: instant Raycast2D damage plus LineRenderer beam. Prefab needs LineRenderer (at least 2 positions); use Default-Line or custom material.
/// </summary>
public class RaycastSniperSpell : SpellBehavior
{
    [Header("激光 / 狙击数值 — Laser / Sniper")]
    public int damage = 50;
    public float range = 100f;
    public LayerMask layerToHit;

    [Tooltip("从发射点沿朝向前移一点再射线，避免火点在碰撞体内部时先打到自己。 — Inset ray start along aim so fire point inside colliders does not hit self first.")]
    public float castStartInset = 0.12f;

    [Header("视觉效果 — Visuals")]
    public float lineVisualDuration = 0.1f;

    private LineRenderer lineRenderer;
    private GameObject myCaster;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    static bool IsColliderOnCaster(GameObject caster, Collider2D col)
    {
        if (caster == null || col == null) return false;
        Transform t = col.transform;
        return t == caster.transform || t.IsChildOf(caster.transform);
    }

    public override void Execute(GameObject caster, Transform firePoint)
    {
        myCaster = caster;

        Vector2 dir = firePoint.up.normalized;
        Vector2 rawStart = firePoint.position;
        Vector2 curPos = rawStart + dir * castStartInset;
        GameObject damageSource = caster;

        if (lineRenderer != null)
            lineRenderer.enabled = true;

        // 面板上 Layer 为 Nothing(0) 时，用默认可碰撞层，否则射线永远打不到东西 — If layer mask is Nothing (0), use default raycast layers or hits never register.
        int mask = layerToHit.value == 0 ? Physics2D.DefaultRaycastLayers : layerToHit.value;

        float budget = Mathf.Max(0.01f, range - castStartInset);
        const float reflectSkin = 0.02f;
        const int maxBounces = 12;

        var linePts = new List<Vector3> { rawStart };

        for (int bounce = 0; bounce < maxBounces && budget > 0.001f; bounce++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(curPos, dir, budget, mask);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            RaycastHit2D hit = default;
            bool hitValid = false;
            foreach (RaycastHit2D h in hits)
            {
                if (h.collider == null) continue;
                if (IsColliderOnCaster(damageSource, h.collider)) continue;
                hit = h;
                hitValid = true;
                break;
            }

            if (!hitValid)
            {
                linePts.Add(curPos + dir * budget);
                break;
            }

            linePts.Add(hit.point);

            ReflectShieldSpell shield = hit.collider.GetComponentInParent<ReflectShieldSpell>();
            if (shield != null)
            {
                Vector2 n = hit.normal.sqrMagnitude > 1e-8f ? hit.normal.normalized : (-dir).normalized;
                dir = Vector2.Reflect(dir, n).normalized;
                budget -= hit.distance;
                curPos = hit.point + dir * reflectSkin;

                // 盾牌现在直接挂在玩家根节点上
                Transform playerRoot = shield.transform.parent;
                if (playerRoot != null)
                    damageSource = playerRoot.gameObject;
                continue;
            }

            if (hit.collider.gameObject.tag == "Wall")
                break;

            if (hit.collider.gameObject.tag == "Player")
            {
                PlayerCombat target = hit.collider.GetComponent<PlayerCombat>()
                    ?? hit.collider.GetComponentInParent<PlayerCombat>();
                if (target != null && target.gameObject != damageSource)
                {
                    if (ReflectShieldSpell.HasActiveShieldOn(target))
                        break;

                    int total = damage;
                    PlayerStats srcStats = damageSource.GetComponent<PlayerStats>();
                    if (srcStats != null)
                        total += Mathf.RoundToInt(srcStats.strength);

                    var srcInput = damageSource.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                    int attackerIndex = srcInput != null ? srcInput.playerIndex : -1;

                    target.TakeDamage(total, attackerIndex, dir);
                }
                break;
            }

            break;
        }

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = linePts.Count;
            for (int i = 0; i < linePts.Count; i++)
                lineRenderer.SetPosition(i, linePts[i]);
        }

        StartCoroutine(HideLine());
    }

    IEnumerator HideLine()
    {
        yield return new WaitForSeconds(lineVisualDuration);
        if (lineRenderer != null)
            lineRenderer.enabled = false;
        Destroy(gameObject);
    }
}
