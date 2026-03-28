using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Stats")]
    public int health = 100;
    public bool isKnockedDown = false;
    public Transform firePoint;

    [Header("Equipped Spells (双槽位)")]
    public SpellData currentAttackSpell;
    public SpellData currentMovementSpell;

    [Header("Drop Settings")]
    public float dropForce = 8f;

    [Header("施法生成")]
    [Tooltip("法术预制体生成点沿枪口前移，减少与自身碰撞体重叠（直接拖火球当 spellPrefab 时也生效）。")]
    public float spellSpawnForwardInset = 0.28f;

    private float attackCDTimer;
    private float movementCDTimer;
    private SpellBehavior activeSubSpell;

    private PlayerController controller;
    private Color originalColor;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    void Start()
    {
        originalColor = GetComponent<SpriteRenderer>().color;
    }

    void Update()
    {
        if (attackCDTimer > 0f) attackCDTimer -= Time.deltaTime;
        if (movementCDTimer > 0f) movementCDTimer -= Time.deltaTime;
    }

    /// <summary>
    /// 按拾取顺序：先填左键槽（主武器），再填右键槽（副武器）。双槽都有则不再捡起（不会覆盖）。
    /// </summary>
    public bool EquipSpell(SpellData newSpell)
    {
        if (newSpell == null) return false;

        if (currentAttackSpell == null)
        {
            currentAttackSpell = newSpell;
            Debug.Log("主武器(左键): " + newSpell.spellName);
            return true;
        }

        if (currentMovementSpell == null)
        {
            currentMovementSpell = newSpell;
            Debug.Log("副武器(右键): " + newSpell.spellName);
            return true;
        }

        Debug.Log("主副武器都已装备，先 Q/E 丢掉一把再捡。");
        return false;
    }

    void DropSpell(SpellData dataToDrop)
    {
        if (dataToDrop == null) return;
        if (dataToDrop.pickupPrefab == null)
        {
            Debug.LogWarning($"丢弃失败：「{dataToDrop.spellName}」的 SpellData 未指定 pickupPrefab，地上不会出现武器。");
            return;
        }

        GameObject dropObj = Instantiate(dataToDrop.pickupPrefab, transform.position, Quaternion.identity);

        // pickupPrefab 若误用成弹道预制体，SpellProjectile.Start 会销毁整颗物体；生成后立刻拆掉弹道脚本
        SpellProjectile[] strayProjectiles = dropObj.GetComponentsInChildren<SpellProjectile>(true);
        for (int i = 0; i < strayProjectiles.Length; i++)
            Object.DestroyImmediate(strayProjectiles[i]);

        SpellPickup pickup = dropObj.GetComponentInChildren<SpellPickup>(true);
        if (pickup == null)
        {
            // 常见原因：SpellData 仍指向旧预制体、预制体未 Apply、或 SpellPickup 变成 Missing Script
            pickup = dropObj.AddComponent<SpellPickup>();
            Debug.LogWarning($"「{dropObj.name}」实例上未找到 SpellPickup，已在根节点自动添加。请在工程里打开 Pickup 预制体检查引用并点 Apply，避免只靠运行时兜底。");
        }

        pickup.spellData = dataToDrop;

        Rigidbody2D rb = dropObj.GetComponentInChildren<Rigidbody2D>(true);
        if (rb != null && firePoint != null)
        {
            Vector2 dropDirection = firePoint.up;
            rb.AddForce(dropDirection * dropForce, ForceMode2D.Impulse);
        }
    }

    void OnDropAttack(InputValue value)
    {
        if (!value.isPressed || currentAttackSpell == null) return;

        DropSpell(currentAttackSpell);
        currentAttackSpell = null;
        Debug.Log("丢弃了主武器");
    }

    void OnDropMovement(InputValue value)
    {
        if (!value.isPressed || currentMovementSpell == null) return;

        if (activeSubSpell != null)
        {
            activeSubSpell.StopExecute();
            activeSubSpell = null;
        }

        DropSpell(currentMovementSpell);
        currentMovementSpell = null;
        Debug.Log("丢弃了副武器");
    }

    void OnCastMain(InputValue value)
    {
        if (!value.isPressed || isKnockedDown) return;
        if (currentAttackSpell == null || attackCDTimer > 0f) return;

        ExecuteAndReturnSpell(currentAttackSpell, ref attackCDTimer);
    }

    void OnCastSub(InputValue value)
    {
        if (currentMovementSpell == null) return;

        if (value.isPressed)
        {
            if (!isKnockedDown && movementCDTimer <= 0f)
            {
                activeSubSpell = ExecuteAndReturnSpell(currentMovementSpell, ref movementCDTimer);
            }
        }
        else if (activeSubSpell != null)
        {
            activeSubSpell.StopExecute();
            activeSubSpell = null;
        }
    }

    SpellBehavior ExecuteAndReturnSpell(SpellData data, ref float cdTimer)
    {
        if (data == null || data.spellPrefab == null) return null;

        Vector3 spawnPos = firePoint.position + firePoint.up * spellSpawnForwardInset;
        GameObject spellObj = Instantiate(data.spellPrefab, spawnPos, firePoint.rotation);

        // 若 spellPrefab 直接就是带 SpellProjectile 的火球（没有 BasicProjectileSpell），上面从来不会走 Execute，必须在这里认主
        SpellProjectile.RegisterWithCaster(spellObj, gameObject);

        SpellBehavior behavior = spellObj.GetComponentInChildren<SpellBehavior>(true);
        if (behavior != null)
            behavior.Execute(gameObject, firePoint);

        cdTimer = data.cooldownTime;
        return behavior;
    }

    public void TakeDamage(int damage)
    {
        if (isKnockedDown) return;

        health -= damage;
        Debug.Log(gameObject.name + " 受到伤害，剩余血量: " + health);

        if (health <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(KnockdownRoutine());
        }
    }

    IEnumerator KnockdownRoutine()
    {
        isKnockedDown = true;
        controller.canMove = false;

        GetComponent<SpriteRenderer>().color = Color.gray;

        yield return new WaitForSeconds(1.5f);

        GetComponent<SpriteRenderer>().color = originalColor;
        controller.canMove = true;
        isKnockedDown = false;
    }

    void Die()
    {
        Debug.Log(gameObject.name + " 被淘汰了！");
        gameObject.SetActive(false);
    }
}
