using UnityEngine;

/// <summary>
/// 按住副键：在 FirePoint 前方生成一道弧形蓝色线条（LineRenderer），并用 EdgeCollider2D 作触发反弹区。
/// 预制体可为空物体，脚本会自动补 LineRenderer / EdgeCollider2D / Kinematic Rigidbody2D。
/// Hold sub button: arc LineRenderer in front of FirePoint plus EdgeCollider2D trigger to reflect projectiles.
/// Prefab can be empty; script adds LineRenderer, EdgeCollider2D, and kinematic Rigidbody2D if missing.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ReflectShieldSpell : SpellBehavior
{
    /// <summary>
    /// 持盾期间用于弹道/激光：身体 Trigger 可能与盾重叠，统一按「有盾则挡投射物」处理。
    /// While shield exists, treat as blocking projectiles/laser when body trigger would also fire.
    /// </summary>
    public static bool HasActiveShieldOn(PlayerCombat target)
    {
        if (target == null) return false;
        var shields = target.GetComponentsInChildren<ReflectShieldSpell>(true);
        for (int i = 0; i < shields.Length; i++)
        {
            if (shields[i] != null && shields[i].isActiveAndEnabled && shields[i].gameObject.activeInHierarchy)
                return true;
        }
        return false;
    }

    [Header("环形盾 / Circle Shield")]
    [Tooltip("圆环的半径（世界单位大致观感） — Circle radius in world units (visual scale).")]
    public float radius = 0.65f;

    [Tooltip("圆环上的顶点数，越多越圆滑 — Vertex count along the circle; higher is smoother.")]
    public int segments = 36;

    [Tooltip("沿施法者中心 Y 轴的偏移（通常填 0 即可包裹全身） — Offset along caster's local Y (0 to center on player).")]
    public float localCenterOffsetY = 0f;

    [Header("线条外观 / Line Look")]
    public float lineWidth = 0.08f;
    public Color arcColor = new Color(0.35f, 0.75f, 1f, 0.95f);
    public int sortingOrder = 12;

    LineRenderer lineRenderer;
    EdgeCollider2D edgeCollider;

    void EnsureComponents()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true; // 闭合圆环
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.numCapVertices = 4;
        Shader sh = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
        if (sh != null)
            lineRenderer.material = new Material(sh);
        lineRenderer.startColor = arcColor;
        lineRenderer.endColor = arcColor;
        lineRenderer.sortingOrder = sortingOrder;

        edgeCollider = GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
            edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
        edgeCollider.isTrigger = true;
    }

    public override void Execute(GameObject caster, Transform firePoint)
    {
        EnsureComponents();

        // 挂载到玩家根节点，而不是枪口，这样圆环能完美包裹玩家
        transform.SetParent(caster.transform, worldPositionStays: false);
        transform.localPosition = Vector3.up * localCenterOffsetY;
        transform.localRotation = Quaternion.identity;

        float step = 360f / segments * Mathf.Deg2Rad;
        var edgePoints = new Vector2[segments + 1];

        for (int i = 0; i < segments; i++)
        {
            float a = i * step;
            float x = Mathf.Sin(a) * radius;
            float y = Mathf.Cos(a) * radius;
            edgePoints[i] = new Vector2(x, y);
            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
        
        // EdgeCollider2D 需要首尾相连来闭合碰撞
        edgePoints[segments] = edgePoints[0];
        edgeCollider.points = edgePoints;
    }

    public override void StopExecute()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 由 SpellProjectile 统一触发：避免与玩家身体 Trigger 的回调顺序不确定导致「先扣血再反弹」。
    /// Invoked from SpellProjectile only so shield always wins ordering over the player body trigger.
    /// </summary>
    public void ApplyReflectToProjectile(SpellProjectile incomingProjectile)
    {
        if (incomingProjectile == null) return;

        incomingProjectile.transform.Rotate(0, 0, 180f);

        // 现在盾牌直接挂在 caster 下，所以 parent 就是 playerRoot
        Transform playerRoot = transform.parent;
        if (playerRoot == null) return;

        incomingProjectile.caster = playerRoot.gameObject;
        SpellProjectile.RegisterWithCaster(incomingProjectile.gameObject, playerRoot.gameObject);
    }
}
