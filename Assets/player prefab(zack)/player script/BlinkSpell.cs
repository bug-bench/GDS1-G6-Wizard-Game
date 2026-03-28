using UnityEngine;

public class BlinkSpell : SpellBehavior
{
    [Header("闪现数值接口")]
    public float blinkDistance = 4f;
    public LayerMask obstacleLayer;

    [Tooltip("射线起点沿朝向微移，减少从碰撞体内部发射导致误判。")]
    public float castInset = 0.05f;

    public override void Execute(GameObject caster, Transform firePoint)
    {
        Vector2 dir = firePoint.up.normalized;
        Vector2 casterPos = caster.transform.position;
        Vector2 rayStart = casterPos + dir * castInset;

        int mask = obstacleLayer.value == 0 ? Physics2D.DefaultRaycastLayers : obstacleLayer.value;
        float rayLen = Mathf.Max(0.01f, blinkDistance - castInset);
        RaycastHit2D hit = Physics2D.Raycast(rayStart, dir, rayLen, mask);

        if (hit.collider != null)
        {
            caster.transform.position = hit.point - dir * 0.3f;
        }
        else
        {
            caster.transform.position = casterPos + dir * blinkDistance;
        }

        Destroy(gameObject);
    }
}
