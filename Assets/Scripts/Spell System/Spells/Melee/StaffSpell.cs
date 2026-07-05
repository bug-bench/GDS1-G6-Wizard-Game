using UnityEngine;

public class StaffSpell : MeleeSpellCore
{
    [Header("Swing")]
    public float swingDuration = 0.25f;

    public float startAngle = 100f;
    public float endAngle = -90f;

    [Header("Slash VFX")]
    public GameObject slashVisualPrefab;

    public Vector2 slashLocalOffset =
        new Vector2(0f, 1f);

    public Vector2 slashLocalScale =
        Vector2.one;

    public float slashRotationOffset;

    private float timer;

    private float currentStartAngle;
    private float currentEndAngle;

    public override void Execute()
    {
        ApplyScaling();

        transform.SetParent(
            firePoint);

        transform.localPosition =
            Vector3.zero;

        bool facingLeft =
            firePoint.up.x < 0f;

        currentStartAngle =
            facingLeft
            ? -startAngle
            : startAngle;

        currentEndAngle =
            facingLeft
            ? -endAngle
            : endAngle;

        SpawnSlashVisual(
            facingLeft);

        transform.localRotation =
            Quaternion.Euler(
                0,
                0,
                currentStartAngle);
    }

    void Update()
    {
        timer += Time.deltaTime;

        float t =
            timer / swingDuration;

        float eased =
            t * t * (3f - 2f * t);

        float angle =
            Mathf.Lerp(
                currentStartAngle,
                currentEndAngle,
                eased);

        transform.localRotation =
            Quaternion.Euler(
                0,
                0,
                angle);

        CheckHits();

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    void SpawnSlashVisual(
        bool facingLeft)
    {
        if (slashVisualPrefab == null)
            return;

        GameObject slash =
            Instantiate(
                slashVisualPrefab,
                transform);

        slash.transform.localPosition =
            slashLocalOffset;

        slash.transform.localScale =
            new Vector3(
                facingLeft
                ? -slashLocalScale.x
                : slashLocalScale.x,
                slashLocalScale.y,
                1f);

        slash.transform.localRotation =
            Quaternion.Euler(
                0,
                0,
                facingLeft
                ? -slashRotationOffset
                : slashRotationOffset);
    }

    void OnDrawGizmosSelected()
    {
        DrawHitboxGizmo();
    }
}