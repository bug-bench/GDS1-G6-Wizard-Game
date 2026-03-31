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
    [Header("弧形盾 / Arc Shield")]
    [Tooltip("弧的半径（世界单位大致观感） — Arc radius in world units (visual scale).")]
    public float arcRadius = 0.55f;

    [Tooltip("弧展开总角度（度），例如 110 像身前一面月牙 — Total arc span in degrees (e.g. 110 for a front crescent).")]
    public float arcAngleDegrees = 110f;

    [Tooltip("弧线上的顶点数，越多越圆滑 — Vertex count along the arc; higher is smoother.")]
    public int arcSegments = 24;

    [Tooltip("沿 FirePoint 本地 Y 前移弧心，让弧鼓在身前 — Offset arc center along FirePoint local Y so the bulge sits in front.")]
    public float localForwardOffset = 0.35f;

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
        lineRenderer.loop = false;
        lineRenderer.positionCount = arcSegments + 1;
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

        transform.SetParent(firePoint, worldPositionStays: false);
        transform.localPosition = Vector3.up * localForwardOffset;
        transform.localRotation = Quaternion.identity;

        float halfRad = arcAngleDegrees * 0.5f * Mathf.Deg2Rad;
        var edgePoints = new Vector2[arcSegments + 1];

        for (int i = 0; i <= arcSegments; i++)
        {
            float t = arcSegments > 0 ? i / (float)arcSegments : 0f;
            float a = Mathf.Lerp(-halfRad, halfRad, t);
            float x = Mathf.Sin(a) * arcRadius;
            float y = Mathf.Cos(a) * arcRadius;
            edgePoints[i] = new Vector2(x, y);
            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }

        edgeCollider.points = edgePoints;
    }

    public override void StopExecute()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        SpellProjectile incomingProjectile = hitInfo.GetComponent<SpellProjectile>()
            ?? hitInfo.GetComponentInParent<SpellProjectile>();
        if (incomingProjectile == null) return;

        incomingProjectile.transform.Rotate(0, 0, 180f);

        Transform firePointTr = transform.parent;
        Transform playerRoot = firePointTr != null ? firePointTr.parent : null;
        if (playerRoot == null) return;

        incomingProjectile.caster = playerRoot.gameObject;
        SpellProjectile.RegisterWithCaster(incomingProjectile.gameObject, playerRoot.gameObject);
    }
}
