using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class SpellPickup : MonoBehaviour
{
    [Tooltip("生成后多少秒内不能被捡起（防丢立刻捡）；用时间判断，不再关碰撞体。 — Seconds after spawn before pickup is allowed (prevents instant re-pick after drop); uses time, not disabling colliders.")]
    public float pickupCooldown = 1f;

    [Tooltip("若 LineRenderer 点数不足或从法术预制体复制过来，在地面补一段本地短线（激光条）。 — If LineRenderer has too few points or was copied from a spell prefab, lay out a short local line on the ground (laser strip).")]
    public bool fixLineRendererIfNeeded = true;

    public SpellData spellData;

    private SpellSpawner spawner;

    private float pickupReadyTime;

    void Awake()
    {
        // 必须在 SpellProjectile.Start 之前关掉弹道脚本，否则 Destroy(gameObject, lifeTime) 会整包销毁拾取物 — Disable before SpellProjectile.Start or delayed Destroy will delete the whole pickup.
        foreach (var proj in GetComponentsInChildren<SpellProjectile>(true))
            proj.enabled = false;
    }

    void OnEnable()
    {
        pickupReadyTime = Time.time + pickupCooldown;
        RestorePickupVisuals();
    }

    /// <summary>
    /// 丢弃时 Instantiate 的实例常见问题：① 误挂 SpellProjectile 会飞走并自毁；② 复制狙击激光后 LineRenderer 被存成 disabled。
    /// Common issues on dropped instances: stray SpellProjectile flies and self-destructs; sniper laser copy leaves LineRenderer disabled.
    /// </summary>
    void RestorePickupVisuals()
    {
        foreach (var lr in GetComponentsInChildren<LineRenderer>(true))
        {
            lr.enabled = true;
            if (!fixLineRendererIfNeeded) continue;
            if (lr.positionCount < 2)
                lr.positionCount = 2;
            // 本地空间画一条「躺在地上」的激光条，避免两点都在世界原点看不见 — Draw a short flat line in local space so the laser strip is visible on the ground.
            Vector3 a = lr.GetPosition(0);
            Vector3 b = lr.GetPosition(1);
            if ((a - b).sqrMagnitude < 0.0001f)
            {
                if (lr.useWorldSpace)
                {
                    Vector2 p = transform.position;
                    lr.SetPosition(0, p + Vector2.left * 0.4f);
                    lr.SetPosition(1, p + Vector2.right * 0.4f);
                }
                else
                {
                    lr.SetPosition(0, new Vector3(-0.4f, 0f, 0f));
                    lr.SetPosition(1, new Vector3(0.4f, 0f, 0f));
                }
            }
        }

        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = true;

        foreach (var proj in GetComponentsInChildren<SpellProjectile>(true))
            proj.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (Time.time < pickupReadyTime) return;
        if (spellData == null) return;

        // 碰撞体常在子物体上，必须用父级查找 PlayerCombat — Collider is often on a child; use GetComponentInParent for PlayerCombat.
        PlayerCombat combat = hitInfo.GetComponentInParent<PlayerCombat>();
        if (combat == null) return;

        bool pickedUp = combat.EquipSpell(spellData);
        if (pickedUp)
        {
            if (spawner != null)
            {
                spawner.RespawnSpell();
            }
            Destroy(gameObject);
        }
        else
        {
            // 双槽都已装备时 EquipSpell 会失败，地上包还在——避免误以为「捡不起来是 Bug」 — Both slots full: EquipSpell fails and pickup remains; log so it is not mistaken for a bug.
            Debug.Log("主武器、副武器都已满，请先按 Q 或 E 丢掉一把，再捡。 | Both weapon slots full; press Q or E to drop one, then pick up.");
        }
    }

    public void SetSpawner(SpellSpawner spellSpawner)
    {
        spawner = spellSpawner;



    }
}
