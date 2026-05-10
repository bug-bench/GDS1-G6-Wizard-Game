using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SelfExplosionSpell : SpellBehavior
{
    [Header("Combat Settings")]
    public int damage = 30;
    public float explosionRadius = 3f;  // 爆炸判定半径
    [Tooltip("击退力。如果觉得不够猛，可以继续改大（比如 60 甚至 100）")]
    public float knockbackForce = 50f;  // 默认击退力加强
    public LayerMask targetLayer;       // 目标层级（确保勾选 Enemy/Player 的 Layer）

    [Header("Visual Settings")]
    public float expandDuration = 0.3f; // 圆环扩散消失的时间
    public int segments = 36;           // 圆环平滑度
    public float lineWidth = 0.15f;     // 圆环粗细
    [Tooltip("圆环颜色（最后会渐变为全透明）")]
    public Color ringColor = new Color(1f, 0.4f, 0.1f, 0.9f); // 默认橘红色
    public int sortingOrder = 12;

    private LineRenderer lineRenderer;
    private float timer;

    void EnsureLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true; // 闭合圆环
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.numCapVertices = 4;
        
        // 尝试获取护盾那样的材质
        Shader sh = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
        if (sh != null)
            lineRenderer.material = new Material(sh);
            
        lineRenderer.sortingOrder = sortingOrder;
        lineRenderer.startColor = ringColor;
        lineRenderer.endColor = ringColor;
    }

    public override void Execute(GameObject caster, Transform firePoint)
    {
        // 1. 强制特效位置在玩家正中心
        transform.position = caster.transform.position;
        
        // 2. 第一帧瞬间进行物理范围检测，结算伤害和击退
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(caster.transform.position, explosionRadius, targetLayer);

        foreach (var col in hitColliders)
        {
            if (col.gameObject == caster) continue; // 排除自己

            PlayerCombat targetCombat = col.GetComponentInParent<PlayerCombat>();
            if (targetCombat != null)
            {
                // 击退方向：从玩家向外辐射
                Vector2 knockbackDir = (targetCombat.transform.position - caster.transform.position).normalized;
                Vector2 knockbackVector = knockbackDir * knockbackForce;

                targetCombat.TakeDamage(damage, -1, knockbackVector);
            }
        }

        EnsureLineRenderer();
        DrawCircle(0.1f); // 初始半径给个很小的值

        // 持续时间结束后销毁
        Destroy(gameObject, expandDuration);
    }

    private void Update()
    {
        if (lineRenderer == null) return;

        timer += Time.deltaTime;
        float t = timer / expandDuration;
        
        // 随着时间扩大圆环的半径
        float currentRadius = Mathf.Lerp(0.1f, explosionRadius, t);
        DrawCircle(currentRadius);

        // 透明度渐隐 (从原设定的 Alpha 降到 0)
        Color c = ringColor;
        c.a = Mathf.Lerp(ringColor.a, 0f, t);
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
    }

    private void DrawCircle(float radius)
    {
        float step = 360f / segments * Mathf.Deg2Rad;
        for (int i = 0; i < segments; i++)
        {
            float a = i * step;
            float x = Mathf.Sin(a) * radius;
            float y = Mathf.Cos(a) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 方便在编辑器查看实际伤害判定范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
