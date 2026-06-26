using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class LaserSpell : HitScanSpellCore
{
    [Header("Visuals")]
    public float lineDuration = 0.1f;
    public bool showLineRenderer = true;

    [Header("Beam VFX")]
    public GameObject beamVFX;
    public float beamThickness = 1f;
    public float beamLength = 1f;
    public float beamLifetime = 0.15f;
    public float beamRotationOffset = 0f;

    [Header("Reflection")]
    public int maxReflections = 8;
    public float reflectionSurfaceOffset = 0.02f;
    public float lineStartOffset = 0.6f;

    private LineRenderer lineRenderer;
    private float baseStartWidth;
    private float baseEndWidth;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            baseStartWidth = lineRenderer.startWidth;
            baseEndWidth = lineRenderer.endWidth;
        }
    }

    public override void Execute()
    {
        hitPoints.Clear();
        PerformLaserTrace();
        OnHitScanFinished(hitPoints);
    }

    private void PerformLaserTrace()
    {
        if (caster == null || firePoint == null)
            return;

        Vector2 dir = firePoint.up.normalized;
        Vector2 rawStart = firePoint.position;
        Vector2 currentPos = rawStart + dir * castStartInset;

        GameObject currentDamageSource = caster;

        int mask = layerMask.value == 0
            ? Physics2D.DefaultRaycastLayers
            : layerMask.value;

        float remainingRange = Mathf.Max(0.01f, range - castStartInset);

        Vector2 visualStart = rawStart + dir * lineStartOffset;
        hitPoints.Add(visualStart);

        for (int reflection = 0; reflection <= maxReflections && remainingRange > 0.001f; reflection++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(currentPos, dir, remainingRange, mask);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            bool foundValidHit = false;
            RaycastHit2D validHit = default;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null)
                    continue;

                if (IsColliderOnObject(currentDamageSource, hit.collider))
                    continue;

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Pickup") ||
                    hit.collider.GetComponentInParent<SpellPickup>() != null)
                    continue;

                ReflectShieldSpell possibleShield = hit.collider.GetComponent<ReflectShieldSpell>()
                    ?? hit.collider.GetComponentInParent<ReflectShieldSpell>();

                if (hit.collider.isTrigger && possibleShield == null)
                    continue;

                validHit = hit;
                foundValidHit = true;
                break;
            }

            if (!foundValidHit)
            {
                hitPoints.Add(currentPos + dir * remainingRange);
                break;
            }

            hitPoints.Add(validHit.point);

            ReflectShieldSpell shield = validHit.collider.GetComponent<ReflectShieldSpell>()
                ?? validHit.collider.GetComponentInParent<ReflectShieldSpell>();

            if (shield != null)
            {
                GameObject newCaster = shield.CurrentCaster != null
                    ? shield.CurrentCaster
                    : currentDamageSource;

                Vector2 normal = validHit.normal.sqrMagnitude > 0.0001f
                    ? validHit.normal.normalized
                    : (-dir).normalized;

                dir = Vector2.Reflect(dir, normal).normalized;
                remainingRange -= validHit.distance;
                currentPos = validHit.point + dir * reflectionSurfaceOffset;
                currentDamageSource = newCaster;
                continue;
            }

            destroyableObject destroyable = validHit.collider.GetComponent<destroyableObject>()
                ?? validHit.collider.GetComponentInParent<destroyableObject>();

            if (destroyable != null)
            {
                float totalDamage = damage + GetStrengthFrom(currentDamageSource);
                destroyable.takeDamage(totalDamage);
                break;
            }

            PlayerCombat target = validHit.collider.GetComponent<PlayerCombat>()
                ?? validHit.collider.GetComponentInParent<PlayerCombat>();

            if (target != null)
            {
                if (target.gameObject != currentDamageSource)
                {
                    if (!ReflectShieldSpell.HasActiveShieldOn(target) && !target.IsInvincible)
                    {
                        int attackerIndex = -1;
                        PlayerInput srcInput = currentDamageSource.GetComponent<PlayerInput>();
                        if (srcInput != null)
                            attackerIndex = srcInput.playerIndex;

                        int totalDamage = Mathf.RoundToInt(damage + GetStrengthFrom(currentDamageSource));
                        target.TakeDamage(totalDamage, attackerIndex, dir * knockbackForce);
                    }
                }

                break;
            }
            break;
        }
    }

    protected override void OnHitScanFinished(List<Vector3> points)
    {
        DrawLine(points);
        SpawnBeamVisuals(points);
        StartCoroutine(HideLine());
    }

    void DrawLine(List<Vector3> points)
    {
        if (lineRenderer == null)
            return;

        float scale = SpellStatScaling.GetSizeScale(caster);

        SpellStatScaling.ApplyLaserWidth(
            lineRenderer,
            baseStartWidth,
            baseEndWidth,
            scale);

        lineRenderer.enabled = showLineRenderer;
        lineRenderer.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }
    }

    void SpawnBeamVisuals(List<Vector3> points)
    {
        if (beamVFX == null)
            return;

        float safeBeamLength = Mathf.Max(0.01f, beamLength);

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 start = points[i];
            Vector3 end = points[i + 1];

            float distance = Vector3.Distance(start, end);
            if (distance <= 0.001f)
                continue;

            int count = Mathf.CeilToInt(distance / safeBeamLength);

            Vector3 direction = (end - start).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            for (int j = 0; j < count; j++)
            {
                float t = (j + 0.5f) / count;

                Vector3 pos = Vector3.Lerp(start, end, t);

                GameObject vfx = Instantiate(
                    beamVFX,
                    pos,
                    Quaternion.Euler(0f, 0f, angle + beamRotationOffset));

                vfx.transform.localScale = new Vector3(
                    safeBeamLength,
                    beamThickness,
                    1f);

                Destroy(vfx, beamLifetime);
            }
        }
    }

    IEnumerator HideLine()
    {
        yield return new WaitForSeconds(lineDuration);

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        Destroy(gameObject);
    }

    float GetStrengthFrom(GameObject source)
    {
        if (source == null) return 0f;

        PlayerStats stats = source.GetComponent<PlayerStats>();
        return stats != null ? stats.strength : 0f;
    }

    static bool IsColliderOnObject(GameObject root, Collider2D col)
    {
        if (root == null || col == null)
            return false;

        Transform t = col.transform;
        return t == root.transform || t.IsChildOf(root.transform);
    }
}