using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class SpellPickup : MonoBehaviour
{
    [Header("Pickup")]
    [Range(0f, 2f)]
    public float pickupCooldown = 0.05f;

    public SpellData spellData;

    [Header("Visual Fixes")]
    [Tooltip("Fix copied LineRenderers (laser pickups).")]
    public bool fixLineRendererIfNeeded = true;

    private SpellSpawner spawner;

    private AudioSource pickupAudio;

    private float pickupReadyTime;

    #region Unity

    void Awake()
    {
        DisableSpellBehaviours();
    }

    void Start()
    {
        pickupAudio = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        pickupReadyTime = Time.time + pickupCooldown;

        RestoreVisuals();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPickupReady())
            return;

        if (spellData == null)
            return;

        PlayerCombat combat =
            other.GetComponentInParent<PlayerCombat>();

        if (combat == null)
            return;

        if (!combat.EquipSpell(spellData))
            return;

        OnPickedUp();
    }

    #endregion

    #region Pickup

    public bool IsPickupReady()
    {
        return Time.time >= pickupReadyTime;
    }

    public void OnPickedUp()
    {
        if (spawner != null)
        {
            spawner.RespawnSpell();
        }

        PlayPickupSound();

        Destroy(gameObject);
    }

    #endregion

    #region Visuals

    void RestoreVisuals()
    {
        RestoreLineRenderers();

        RestoreSpriteRenderers();

        DisableSpellBehaviours();
    }

    void RestoreSpriteRenderers()
    {
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.enabled = true;
        }
    }

    void RestoreLineRenderers()
    {
        foreach (LineRenderer line in GetComponentsInChildren<LineRenderer>(true))
        {
            line.enabled = true;

            if (!fixLineRendererIfNeeded)
                continue;

            if (line.positionCount < 2)
                line.positionCount = 2;

            Vector3 a = line.GetPosition(0);
            Vector3 b = line.GetPosition(1);

            if ((a - b).sqrMagnitude > 0.0001f)
                continue;

            if (line.useWorldSpace)
            {
                Vector2 pos = transform.position;

                line.SetPosition(0, pos + Vector2.left * 0.4f);
                line.SetPosition(1, pos + Vector2.right * 0.4f);
            }
            else
            {
                line.SetPosition(0, new Vector3(-0.4f, 0f));
                line.SetPosition(1, new Vector3(0.4f, 0f));
            }
        }
    }

    #endregion

    #region Spell Behaviour

    /// <summary>
    /// Disables every SpellBehaviour attached to the pickup.
    /// This prevents projectile movement, utility execution, etc.
    /// </summary>
    void DisableSpellBehaviours()
    {
        foreach (SpellBehavior spell in GetComponentsInChildren<SpellBehavior>(true))
        {
            spell.enabled = false;
        }
    }

    #endregion

    #region Audio

    void PlayPickupSound()
    {
        if (pickupAudio == null)
            return;

        if (pickupAudio.clip == null)
            return;

        AudioSource.PlayClipAtPoint(
            pickupAudio.clip,
            transform.position);
    }

    #endregion

    #region Public

    public void SetSpawner(SpellSpawner newSpawner)
    {
        spawner = newSpawner;
    }

    #endregion
}