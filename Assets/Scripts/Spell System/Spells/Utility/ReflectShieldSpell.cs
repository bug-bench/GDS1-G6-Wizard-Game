using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ReflectShieldSpell : UtilitySpellCore
{
    public const float DefaultMaxHoldDuration = 3f;

    public GameObject CurrentCaster => caster;

    public static bool HasActiveShieldOn(PlayerCombat target)
    {
        if (target == null) return false;

        ReflectShieldSpell[] shields = target.GetComponentsInChildren<ReflectShieldSpell>(true);
        for (int i = 0; i < shields.Length; i++)
        {
            if (shields[i] != null &&
                shields[i].isActiveAndEnabled &&
                shields[i].gameObject.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    [Header("Shield Shape")]
    [Tooltip("Radius of the circular shield around the player.")]
    public float radius = 0.65f;

    [Tooltip("Number of points used to approximate the circle.")]
    public int segments = 36;

    [Tooltip("Vertical local offset from the player root. Usually 0.")]
    public float localCenterOffsetY = 0f;

    [Header("Line Renderer Visual")]
    [Tooltip("If enabled, show a circular line around the shield.")]
    public bool showLineRenderer = false;

    public float lineWidth = 0.08f;
    public Color shieldColor = new Color(0.35f, 0.75f, 1f, 0.95f);
    public int sortingOrder = 12;

    [Header("Shield VFX")]
    [Tooltip("Optional animated visual prefab that sits on top of the shield.")]
    public GameObject shieldVisualPrefab;

    [Tooltip("Scale multiplier for the shield visual prefab.")]
    public float visualScale = 1f;

    [Tooltip("Local offset for the shield visual prefab.")]
    public Vector2 visualLocalOffset = new Vector2(0f, 0.25f);

    private GameObject activeShieldVisual;

    private LineRenderer lineRenderer;
    private EdgeCollider2D edgeCollider;
    private Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake();

        if (maxHoldDuration <= 0f)
            maxHoldDuration = DefaultMaxHoldDuration;
    }

    public override void Execute()
    {
        BuildShield();
    }

    public override void StopExecute()
    {
        CleanupShield();
        Destroy(gameObject);
    }

    private void BuildShield()
    {
        if (caster == null)
        {
            Debug.LogWarning("[ReflectShieldSpell] Execute called before Initialize / caster was assigned.");
            return;
        }

        EnsureComponents();

        // Parent to the caster root so the shield wraps around the player,
        // not around the fire point or the spell spawn position.
        transform.SetParent(caster.transform, worldPositionStays: false);
        transform.localPosition = Vector3.up * localCenterOffsetY;
        transform.localRotation = Quaternion.identity;

        BuildCircleVisual();
        BuildCircleCollider();
        SpawnShieldVisual();
    }

    private void EnsureComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 4;
        lineRenderer.startColor = shieldColor;
        lineRenderer.endColor = shieldColor;
        lineRenderer.sortingOrder = sortingOrder;
        lineRenderer.enabled = showLineRenderer;

        if (lineRenderer.material == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
                lineRenderer.material = new Material(shader);
        }

        edgeCollider = GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
            edgeCollider = gameObject.AddComponent<EdgeCollider2D>();

        edgeCollider.isTrigger = true;
    }

    private void BuildCircleVisual()
    {
        if (lineRenderer == null)
            return;

        if (segments < 3)
            segments = 3;

        lineRenderer.positionCount = segments;

        float step = (Mathf.PI * 2f) / segments;

        for (int i = 0; i < segments; i++)
        {
            float a = i * step;

            // Match your original orientation
            float x = Mathf.Sin(a) * radius;
            float y = Mathf.Cos(a) * radius;

            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }

    private void BuildCircleCollider()
    {
        if (edgeCollider == null)
            return;

        if (segments < 3)
            segments = 3;

        Vector2[] points = new Vector2[segments + 1];
        float step = (Mathf.PI * 2f) / segments;

        for (int i = 0; i < segments; i++)
        {
            float a = i * step;
            float x = Mathf.Sin(a) * radius;
            float y = Mathf.Cos(a) * radius;
            points[i] = new Vector2(x, y);
        }

        // Close the ring
        points[segments] = points[0];
        edgeCollider.points = points;
    }

    private void SpawnShieldVisual()
    {
        if (shieldVisualPrefab == null)
            return;

        if (activeShieldVisual != null)
            Destroy(activeShieldVisual);

        activeShieldVisual = Instantiate(shieldVisualPrefab, transform);
        activeShieldVisual.transform.localPosition = visualLocalOffset;
        activeShieldVisual.transform.localRotation = Quaternion.identity;
        activeShieldVisual.transform.localScale = Vector3.one * visualScale;
    }

    private void CleanupShield()
    {
        if (activeShieldVisual != null)
        {
            Destroy(activeShieldVisual);
            activeShieldVisual = null;
        }
    }

    /// <summary>
    /// Called by SpellProjectile when it hits this shield.
    /// The projectile is rotated 180 degrees and ownership is transferred
    /// to the player holding the shield.
    /// </summary>
    public void ApplyReflectToProjectile(SpellProjectile incomingProjectile)
    {
        if (incomingProjectile == null)
            return;

        if (caster == null)
            return;

        // Flip projectile direction
        incomingProjectile.transform.Rotate(0f, 0f, 180f);

        // Transfer ownership to shield owner and re-register collision ignores
        incomingProjectile.ReflectToNewCaster(caster);
    }
}