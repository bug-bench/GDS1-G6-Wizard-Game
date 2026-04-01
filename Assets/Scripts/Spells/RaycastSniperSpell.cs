using System.Collections;
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
        Vector2 startPos = rawStart + dir * castStartInset;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, rawStart);
        }

        // 面板上 Layer 为 Nothing(0) 时，用默认可碰撞层，否则射线永远打不到东西 — If layer mask is Nothing (0), use default raycast layers or hits never register.
        int mask = layerToHit.value == 0 ? Physics2D.DefaultRaycastLayers : layerToHit.value;

        float castLen = Mathf.Max(0.01f, range - castStartInset);
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, dir, castLen, mask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        RaycastHit2D hit = default;
        bool hitValid = false;
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider == null) continue;
            if (IsColliderOnCaster(caster, h.collider)) continue;
            hit = h;
            hitValid = true;
            break;
        }

        Vector2 endPoint;
        if (hitValid)
        {
            endPoint = hit.point;

            if (hit.collider.gameObject.tag == "Player")
            {
                PlayerCombat target = hit.collider.GetComponent<PlayerCombat>()
                    ?? hit.collider.GetComponentInParent<PlayerCombat>();
                if (target != null && target.gameObject != caster)
                {
                    int total = damage;
                    PlayerStats casterStats = caster.GetComponent<PlayerStats>();
                    if (casterStats != null)
                        total += Mathf.RoundToInt(casterStats.strength);

                    var casterInput = caster.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                    int attackerIndex = casterInput != null ? casterInput.playerIndex : -1; 

                    target.TakeDamage(total, attackerIndex);
                }
            }
        }
        else
        {
            endPoint = startPos + dir * castLen;
        }

        if (lineRenderer != null)
            lineRenderer.SetPosition(1, endPoint);

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
