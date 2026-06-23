using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserSpell : HitScanSpellCore
{
    [Header("Visuals")]
    public float lineDuration = 0.1f;

    public bool showLineRenderer = true;

    public GameObject beamVFX;

    public float beamThickness = 1f;

    public float beamLength = 1f;

    public float beamLifetime = 0.15f;

    public float beamRotationOffset = 0f;

    private LineRenderer lineRenderer;

    private float baseStartWidth;
    private float baseEndWidth;

    private void Awake()
    {
        lineRenderer =
            GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            baseStartWidth =
                lineRenderer.startWidth;

            baseEndWidth =
                lineRenderer.endWidth;
        }
    }

    protected override void OnHitScanFinished(
        List<Vector3> points)
    {
        DrawLine(points);

        SpawnBeamVisuals(points);

        StartCoroutine(HideLine());
    }

    void DrawLine(List<Vector3> points)
    {
        if (lineRenderer == null)
            return;

        float scale =
            SpellStatScaling.GetSizeScale(caster);

        SpellStatScaling.ApplyLaserWidth(
            lineRenderer,
            baseStartWidth,
            baseEndWidth,
            scale);

        lineRenderer.enabled =
            showLineRenderer;

        lineRenderer.positionCount =
            points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(
                i,
                points[i]);
        }
    }

    void SpawnBeamVisuals(
        List<Vector3> points)
    {
        if (beamVFX == null)
            return;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 start = points[i];
            Vector3 end = points[i + 1];

            float distance =
                Vector3.Distance(start, end);

            int count =
                Mathf.CeilToInt(
                    distance / beamLength);

            Vector3 direction =
                (end - start).normalized;

            float angle =
                Mathf.Atan2(
                    direction.y,
                    direction.x) *
                Mathf.Rad2Deg;

            for (int j = 0; j < count; j++)
            {
                float t =
                    (j + 0.5f) /
                    count;

                Vector3 pos =
                    Vector3.Lerp(
                        start,
                        end,
                        t);

                GameObject vfx =
                    Instantiate(
                        beamVFX,
                        pos,
                        Quaternion.Euler(
                            0,
                            0,
                            angle + beamRotationOffset));

                vfx.transform.localScale =
                    new Vector3(
                        beamLength,
                        beamThickness,
                        1f);

                Destroy(
                    vfx,
                    beamLifetime);
            }
        }
    }

    IEnumerator HideLine()
    {
        yield return new WaitForSeconds(
            lineDuration);

        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }
}