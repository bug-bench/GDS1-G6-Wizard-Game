using UnityEngine;

public class BlinkSpell : SpellBehavior
{
    [Header("闪现数值接口")]
    public float blinkDistance = 4f;
    public LayerMask obstacleLayer;

    public override void Execute(GameObject caster, Transform firePoint)
    {
        Vector2 dir = firePoint.up;
        Vector2 startPos = caster.transform.position;

        RaycastHit2D hit = Physics2D.Raycast(startPos, dir, blinkDistance, obstacleLayer);

        if (hit.collider != null)
        {
            caster.transform.position = hit.point - dir * 0.3f;
        }
        else
        {
            caster.transform.position = startPos + dir * blinkDistance;
        }

        Destroy(gameObject);
    }
}
