using UnityEngine;

public class BlinkSpell : UtilitySpellCore
{
    [Header("Blink")]
    public float blinkDistance = 4f;

    public LayerMask obstacleLayer;

    public float blinkCastRadius = 0.4f;

    public float wallBuffer = 0.35f;

    public float selfStunDuration = 0.06f;

    [Header("After Image")]
    public GameObject afterImagePrefab;
    public int afterImageCount = 5;
    public float afterImageFadeTime = 0.25f;

    public override void Execute()
    {
        float finalDistance =
            blinkDistance *
            SizeScale;

        Vector2 direction =
            firePoint.up.normalized;

        // Store the player's original position BEFORE teleporting
        Vector2 start =
            caster.transform.position;

        Vector2 target =
            GetBlinkPosition(
                start,
                direction,
                finalDistance);

        // Spawn after images BEFORE teleporting
        SpawnAfterImages(start, target);

        Teleport(target);

        Destroy(gameObject);
    }

    void Teleport(Vector2 target)
    {
        if (RB != null)
        {
            RB.linearVelocity = Vector2.zero;
            RB.position = target;
        }
        else
        {
            caster.transform.position = target;
        }

        if (Stats != null &&
            selfStunDuration > 0f)
        {
            Stats.ApplyStun(selfStunDuration);
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

    void SpawnAfterImages(Vector2 start, Vector2 end)
    {
        if (afterImagePrefab == null)
            return;

        SpriteRenderer casterSR =
            caster.GetComponentInChildren<SpriteRenderer>();

        if (casterSR == null)
            return;

        for (int i = 1; i <= afterImageCount; i++)
        {
            float t = i / (float)(afterImageCount + 1);

            Vector2 pos =
                Vector2.Lerp(start, end, t);

            GameObject img =
                Instantiate(
                    afterImagePrefab,
                    pos,
                    caster.transform.rotation);

            // Copy the player's sprite
            SpriteRenderer imgSR =
                img.GetComponent<SpriteRenderer>();

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

            // Existing fade behaviour
            AfterImage fade =
                img.GetComponent<AfterImage>();

            if (fade != null)
                fade.fadeTime = afterImageFadeTime;

            // NEW: Initialize damaging flame behaviour (if present)
            FlameAfterImage flame =
                img.GetComponent<FlameAfterImage>();

            if (flame != null)
            {
                flame.Initialize(caster);
            }
        }
    }
}