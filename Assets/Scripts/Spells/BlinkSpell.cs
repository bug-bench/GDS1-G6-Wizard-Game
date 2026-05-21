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

    [Tooltip("会阻挡闪现的 Sorting Layer 名称 — Sorting Layer names that block blink.")]
    public string[] blockingSortingLayers;

    [Tooltip("闪现路径检测半径，应该接近玩家碰撞体大小 — Blink path check radius, should roughly match player collider size.")]
    public float blinkCastRadius = 0.25f;

    [Tooltip("撞墙时终点离墙皮留的空隙 — Gap from wall surface when stopping on hit.")]
    public float wallBuffer = 0.2f;

    [Tooltip("射线起点沿移动方向微移，避免从碰撞体内部出发 — Nudge ray origin along move dir to avoid casting from inside colliders.")]
    public float castInset = 0.08f;

    [Tooltip("闪现落地后的硬直时间（眩晕，无法移动施法） — Stun duration applied to self after blinking.")]
    public float selfStunDuration = 0f;

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

        Destroy(gameObject);
    }

    static bool IsColliderOnCaster(GameObject casterRoot, Collider2D col)
    {
        if (casterRoot == null || col == null) return false;
        Transform t = col.transform;
        return t == casterRoot.transform || t.IsChildOf(casterRoot.transform);
    }

    bool IsSortingLayerBlocked(Collider2D col)
    {
        if (blockingSortingLayers == null || blockingSortingLayers.Length == 0)
            return false;

        Renderer renderer = col.GetComponentInParent<Renderer>();

        if (renderer == null)
            return false;

        for (int i = 0; i < blockingSortingLayers.Length; i++)
        {
            if (renderer.sortingLayerName == blockingSortingLayers[i])
                return true;
        }

        return false;
    }

    bool IsPhysicsLayerBlocked(Collider2D col, int mask)
    {
        int objectLayerMask = 1 << col.gameObject.layer;
        return (mask & objectLayerMask) != 0;
    }

    bool IsBlockingCollider(Collider2D col, int mask)
    {
        if (col == null) return false;
        if (col.isTrigger) return false; // 忽略触发器 — Ignore triggers

        bool blockedByPhysicsLayer = IsPhysicsLayerBlocked(col, mask);
        bool blockedBySortingLayer = IsSortingLayerBlocked(col);

        return blockedByPhysicsLayer || blockedBySortingLayer;
    }

    Vector2 ResolvePathEnd(GameObject caster, Vector2 start, Vector2 dir, Vector2 desiredEnd)
    {
        float pathLen = Vector2.Distance(start, desiredEnd);
        if (pathLen < 0.001f)
            return start;

        // 默认检测 wall 层，如果没有设置则使用 DefaultRaycastLayers
        int mask = obstacleLayer.value == 0 ? LayerMask.GetMask("wall") : obstacleLayer.value;
        if (mask == 0) mask = Physics2D.DefaultRaycastLayers;

        // 此时退回到射线击中墙壁的前面，防止卡在墙里
        Vector2 rayOrigin = start + dir * castInset;
        float rayLen = Mathf.Max(0.01f, pathLen - castInset);

        // 使用 CircleCast 而不是 Raycast，因为玩家有体积，单条射线可能会从墙边缝隙穿过去
        // Use CircleCast instead of Raycast because the player has size, and a thin ray can miss wall edges/gaps.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            rayOrigin,
            blinkCastRadius,
            dir,
            rayLen,
            Physics2D.DefaultRaycastLayers
        );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.isTrigger) continue; // 忽略触发器
            if (IsColliderOnCaster(caster, hit.collider)) continue;

            // 如果物体在阻挡 Physics Layer 或 Sorting Layer 上，就停在它前面
            // If object is on a blocking Physics Layer or Sorting Layer, stop before it.
            if (IsBlockingCollider(hit.collider, mask))
            {
                // CircleCast 使用 centroid 会比 hit.point 更适合作为玩家的新位置
                // CircleCast centroid is better than hit.point for placing the player body safely.
                return hit.centroid - dir * wallBuffer;
            }
        }

        // 最后检查落点是否直接卡进墙里，防止目标点在障碍物内部
        // Final check: make sure the destination itself is not inside a wall/object.
        Collider2D[] destinationHits = Physics2D.OverlapCircleAll(
            desiredEnd,
            blinkCastRadius,
            Physics2D.DefaultRaycastLayers
        );

        foreach (Collider2D col in destinationHits)
        {
            if (col == null) continue;
            if (col.isTrigger) continue;
            if (IsColliderOnCaster(caster, col)) continue;

            if (IsBlockingCollider(col, mask))
            {
                return start;
            }
        }

        return desiredEnd;
    }
}