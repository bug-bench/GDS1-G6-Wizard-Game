using UnityEngine;

public class BlinkSpell : UtilitySpellCore
{
    [Header("Blink")]
    public float blinkDistance = 4f;

    public LayerMask obstacleLayer;

    public float blinkCastRadius = 0.4f;

    public float wallBuffer = 0.35f;

    public float selfStunDuration = 0.06f;

    public override void Execute()
    {
        float finalDistance =
            blinkDistance *
            SizeScale;

        Vector2 direction =
            firePoint.up.normalized;

        Vector2 start =
            caster.transform.position;

        Vector2 target =
            GetBlinkPosition(
                start,
                direction,
                finalDistance);

        Teleport(target);

        Destroy(gameObject);
    }

    void Teleport(Vector2 target)
    {
        if (RB != null)
        {
            RB.linearVelocity =
                Vector2.zero;

            RB.position =
                target;
        }
        else
        {
            caster.transform.position =
                target;
        }

        if (Stats != null &&
            selfStunDuration > 0f)
        {
            Stats.ApplyStun(
                selfStunDuration);
        }
    }

    Vector2 GetBlinkPosition(
        Vector2 start,
        Vector2 direction,
        float distance)
    {
        RaycastHit2D hit =
            Physics2D.CircleCast(
                start,
                blinkCastRadius,
                direction,
                distance,
                obstacleLayer);

        if (hit.collider != null)
        {
            return start +
                   direction *
                   Mathf.Max(
                       0,
                       hit.distance -
                       wallBuffer);
        }

        return start +
               direction *
               distance;
    }
}