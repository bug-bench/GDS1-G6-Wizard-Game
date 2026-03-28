using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class SpellPickup : MonoBehaviour
{
    [Tooltip("生成后多少秒内不能被捡起（防丢立刻捡）；用时间判断，不再关碰撞体。")]
    public float pickupCooldown = 1f;

    [Tooltip("若 LineRenderer 点数不足或从法术预制体复制过来，在地面补一段本地短线（激光条）。")]
    public bool fixLineRendererIfNeeded = true;

    public SpellData spellData;

    private float pickupReadyTime;

    void Awake()
    {
        // 必须在 SpellProjectile.Start 之前关掉弹道脚本，否则 Destroy(gameObject, lifeTime) 会整包销毁拾取物
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
    /// </summary>
    void RestorePickupVisuals()
    {
        foreach (var lr in GetComponentsInChildren<LineRenderer>(true))
        {
            lr.enabled = true;
            if (!fixLineRendererIfNeeded) continue;
            if (lr.positionCount < 2)
                lr.positionCount = 2;
            // 本地空间画一条「躺在地上」的激光条，避免两点都在世界原点看不见
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

        // 碰撞体常在子物体上，必须用父级查找 PlayerCombat
        PlayerCombat combat = hitInfo.GetComponentInParent<PlayerCombat>();
        if (combat == null) return;

        bool pickedUp = combat.EquipSpell(spellData);
        if (pickedUp)
            Destroy(gameObject);
    }
}
