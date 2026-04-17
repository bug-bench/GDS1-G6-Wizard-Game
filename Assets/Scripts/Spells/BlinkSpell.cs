using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// LoL 风格闪现：朝「当前瞄准目标点」传送，距离不超过 blinkDistance；
/// 若鼠标在脚下则退回为沿 firePoint 方向；途中撞墙则停在墙前。
/// LoL-style blink toward current aim, clamped to blinkDistance; if mouse is on feet, blink along firePoint; stop before walls.
/// </summary>
public class BlinkSpell : SpellBehavior
{
    [Header("闪现数值接口 — Blink Tuning")]
    public float blinkDistance = 4f;
    public LayerMask obstacleLayer;

    [Tooltip("撞墙时终点离墙皮留的空隙 — Gap from wall surface when stopping on hit.")]
    public float wallBuffer = 0.2f;

    [Tooltip("射线起点沿移动方向微移，避免从碰撞体内部出发 — Nudge ray origin along move dir to avoid casting from inside colliders.")]
    public float castInset = 0.08f;

    [Tooltip("闪现落地后的硬直时间（眩晕，无法移动施法） — Stun duration applied to self after blinking.")]
    public float selfStunDuration = 0.5f;

    public override void Execute(GameObject caster, Transform firePoint)
    {
        Vector2 casterPos = caster.transform.position;
        Vector2 dir;
        float travelDist;

        Camera cam = caster.GetComponentInChildren<Camera>();
        PlayerInput pi = caster.GetComponent<PlayerInput>();
        bool useMouseAim = pi != null && pi.currentControlScheme == "KeyMouse"
            && Mouse.current != null && cam != null;

        if (useMouseAim)
        {
            Vector2 mouseWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 toTarget = mouseWorld - casterPos;
            float dist = toTarget.magnitude;
            if (dist < 0.08f)
            {
                // 鼠标几乎在角色身上：按面朝方向闪一段，避免原地抽搐 — Mouse on character: blink along facing to avoid jitter.
                dir = firePoint.up.normalized;
                travelDist = blinkDistance;
            }
            else
            {
                dir = toTarget / dist;
                travelDist = Mathf.Min(blinkDistance, dist);
            }
        }
        else
        {
            // 手柄：沿右摇杆瞄准方向，最大 blinkDistance — Gamepad: along right-stick aim, up to blinkDistance.
            dir = firePoint.up.normalized;
            travelDist = blinkDistance;
        }

        Vector2 desiredEnd = casterPos + dir * travelDist;
        Vector2 finalPos = ResolvePathEnd(caster, casterPos, dir, desiredEnd);

        Rigidbody2D rb = caster.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.position = finalPos;
        else
        {
            Vector3 p = caster.transform.position;
            p.x = finalPos.x;
            p.y = finalPos.y;
            caster.transform.position = p;
        }

        if (selfStunDuration > 0f)
        {
            PlayerStats stats = caster.GetComponent<PlayerStats>();
            if (stats != null)
                stats.ApplyStun(selfStunDuration);
        }

        Destroy(gameObject);
    }

    static bool IsColliderOnCaster(GameObject casterRoot, Collider2D col)
    {
        if (casterRoot == null || col == null) return false;
        Transform t = col.transform;
        return t == casterRoot.transform || t.IsChildOf(casterRoot.transform);
    }

    Vector2 ResolvePathEnd(GameObject caster, Vector2 start, Vector2 dir, Vector2 desiredEnd)
    {
        float pathLen = Vector2.Distance(start, desiredEnd);
        if (pathLen < 0.001f)
            return start;

        int mask = obstacleLayer.value == 0 ? Physics2D.DefaultRaycastLayers : obstacleLayer.value;
        Vector2 rayOrigin = start + dir * castInset;
        float rayLen = Mathf.Max(0.01f, pathLen - castInset);

        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, dir, rayLen, mask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;
            if (IsColliderOnCaster(caster, hit.collider)) continue;
            return hit.point - dir * wallBuffer;
        }

        return desiredEnd;
    }
}
