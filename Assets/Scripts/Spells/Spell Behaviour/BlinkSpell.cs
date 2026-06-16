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

    [Tooltip("会阻挡闪现的物理 Layer — Physics layers that block blink.")]
    public LayerMask obstacleLayer;

    [Tooltip("闪现路径检测半径，防止从墙角/Tilemap 边缘穿过去 — Blink path check radius to prevent slipping through wall/tilemap corners.")]
    public float blinkCastRadius = 0.4f;

    [Tooltip("撞墙时终点离墙皮留的空隙 — Gap from wall surface when stopping on hit.")]
    public float wallBuffer = 0.35f;

    [Tooltip("射线起点沿移动方向微移，避免从碰撞体内部出发 — Nudge ray origin along move dir to avoid casting from inside colliders.")]
    public float castInset = 0.02f;

    [Tooltip("闪现落地后的硬直时间（眩晕，无法移动施法） — Stun duration applied to self after blinking.")]
    public float selfStunDuration = 0.06f;

    [Header("Landing Safety")]
    [Tooltip("落点不安全时往回拉的次数 — How many times to pull back if landing is unsafe.")]
    public int safetyPullbackSteps = 12;

    [Tooltip("每次往回拉的距离 — Distance pulled back each safety step.")]
    public float safetyPullbackAmount = 0.1f;

    [Header("After Image")]
    public GameObject afterImagePrefab;
    public int afterImageCount = 5;
    public float afterImageFadeTime = 0.25f;

    public override void Execute(GameObject caster, Transform firePoint)
    {
        float blinkRange = blinkDistance * SpellStatScaling.GetSizeScale(caster);

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
                travelDist = blinkRange;
            }
            else
            {
                dir = toTarget / dist;
                travelDist = Mathf.Min(blinkRange, dist);
            }
        }
        else
        {
            // 手柄：沿右摇杆瞄准方向，最大 blinkDistance — Gamepad: along right-stick aim, up to blinkDistance.
            dir = firePoint.up.normalized;
            travelDist = blinkRange;
        }

        Vector2 finalPos = GetBlinkPosition(caster, casterPos, dir, travelDist);

        SpawnAfterImages(caster, casterPos, finalPos);

        Rigidbody2D rb = caster.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 停止闪现前的高速移动，防止闪现后继续冲进墙里
            // Stop fast movement before blink so the player does not keep sliding into walls after blinking.
            rb.linearVelocity = Vector2.zero;

            // 移动到最终安全位置
            // Move to the final safe position.
            rb.position = finalPos;

            // 再清一次，避免同一帧保留旧速度
            // Clear again to avoid keeping old velocity during the same frame.
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            Vector3 p = caster.transform.position;
            p.x = finalPos.x;
            p.y = finalPos.y;
            caster.transform.position = p;
        }

        // 轻微硬直用于防止移动脚本在闪现后立刻重新推动玩家穿墙
        // Tiny stun prevents the movement script from instantly pushing the player through a wall after blink.
        if (selfStunDuration > 0f)
        {
            PlayerStats stats = caster.GetComponent<PlayerStats>();
            if (stats != null)
                stats.ApplyStun(selfStunDuration);
        }

        Destroy(gameObject);
    }

    Vector2 GetBlinkPosition(GameObject caster, Vector2 start, Vector2 dir, float blinkRange)
    {
        if (blinkRange <= 0.001f)
            return start;

        // 默认检测 wall 层，如果没有设置则使用 DefaultRaycastLayers
        int mask = obstacleLayer.value == 0 ? LayerMask.GetMask("wall") : obstacleLayer.value;
        if (mask == 0) mask = Physics2D.DefaultRaycastLayers;

        Vector2 rayOrigin = start + dir * castInset;
        float rayDistance = Mathf.Max(0.01f, blinkRange - castInset);

        // 使用 CircleCast 而不是 Raycast，因为 Raycast 是一条很细的线，可能从 Tilemap 墙角穿过去
        // Use CircleCast instead of Raycast because a thin ray can slip past tilemap corners.
        RaycastHit2D hit = Physics2D.CircleCast(
            rayOrigin,
            blinkCastRadius,
            dir,
            rayDistance,
            mask
        );

        bool wallInBlinkPath = hit.collider != null
            && !hit.collider.isTrigger
            && !IsColliderOnCaster(caster, hit.collider);

        Vector2 targetPos;

        if (wallInBlinkPath)
        {
            // 如果墙在闪现距离内，就停在墙前面
            // If the wall is inside the blink distance, stop before the wall.
            float safeDistance = Mathf.Max(0f, hit.distance + castInset - wallBuffer);
            targetPos = start + dir * safeDistance;
        }
        else
        {
            // 如果没有墙，就正常闪现完整距离
            // If there is no wall, blink the full distance.
            targetPos = start + dir * blinkRange;
        }

        // 最后再检查一次落点是否卡进墙角/Tilemap 角落
        // Final safety check to stop landing inside corners / tilemap edges.
        return PullBackUntilSafe(caster, start, targetPos, mask);
    }

    Vector2 PullBackUntilSafe(GameObject caster, Vector2 start, Vector2 target, int mask)
    {
        Vector2 directionFromStart = target - start;
        float distance = directionFromStart.magnitude;

        if (distance <= 0.001f)
            return start;

        Vector2 dir = directionFromStart.normalized;

        // Step backwards in small chunks until the landing position is no longer inside a wall.
        // This helps stop corner slipping on TilemapCollider2D corners.
        for (int i = 0; i < safetyPullbackSteps; i++)
        {
            Collider2D hit = Physics2D.OverlapCircle(
                target,
                blinkCastRadius,
                mask
            );

            bool landingInsideWall = hit != null
                && !hit.isTrigger
                && !IsColliderOnCaster(caster, hit);

            if (!landingInsideWall)
                return target;

            distance -= safetyPullbackAmount;

            if (distance <= 0f)
                return start;

            target = start + dir * distance;
        }

        return start;
    }

    static bool IsColliderOnCaster(GameObject casterRoot, Collider2D col)
    {
        if (casterRoot == null || col == null) return false;

        Transform t = col.transform;
        return t == casterRoot.transform || t.IsChildOf(casterRoot.transform);
    }

    void SpawnAfterImages(GameObject caster, Vector2 start, Vector2 end)
    {
        if (afterImagePrefab == null) return;

        SpriteRenderer casterSR = caster.GetComponentInChildren<SpriteRenderer>();
        if (casterSR == null) return;

        for (int i = 1; i <= afterImageCount; i++)
        {
            float t = i / (float)(afterImageCount + 1);
            Vector2 pos = Vector2.Lerp(start, end, t);

            GameObject img = Instantiate(afterImagePrefab, pos, caster.transform.rotation);

            SpriteRenderer imgSR = img.GetComponent<SpriteRenderer>();
            if (imgSR != null)
            {
                imgSR.sprite = casterSR.sprite;
                imgSR.flipX = casterSR.flipX;
                imgSR.flipY = casterSR.flipY;
                imgSR.sortingLayerID = casterSR.sortingLayerID;
                imgSR.sortingOrder = casterSR.sortingOrder - 1;

                Color c = imgSR.color;
                c.a = Mathf.Lerp(0.15f, 0.6f, 1f - t);
                imgSR.color = c;
            }

            AfterImage fade = img.GetComponent<AfterImage>();
            if (fade != null)
                fade.fadeTime = afterImageFadeTime;
        }
    }
}