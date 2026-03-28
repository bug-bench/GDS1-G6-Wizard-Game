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
    private SpellBehavior activeMainSpell;
    private SpellBehavior activeSubSpell;

    private PlayerController controller;
    private Color originalColor;

    private PlayerInput playerInput;
    private InputAction castMainAction;
    private InputAction castSubAction;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    void Start()
    {
        originalColor = GetComponent<SpriteRenderer>().color;
    }

    void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null || playerInput.actions == null) return;

        castMainAction = playerInput.actions.FindAction("Player/CastMain", throwIfNotFound: false)
                         ?? playerInput.actions.FindAction("CastMain", throwIfNotFound: false);
        if (castMainAction != null)
        {
            castMainAction.started += OnCastMainStarted;
            castMainAction.canceled += OnCastMainCanceled;
        }

        castSubAction = playerInput.actions.FindAction("Player/CastSub", throwIfNotFound: false)
                        ?? playerInput.actions.FindAction("CastSub", throwIfNotFound: false);
        if (castSubAction != null)
        {
            castSubAction.started += OnCastSubStarted;
            castSubAction.canceled += OnCastSubCanceled;
        }
    }

    void OnDisable()
    {
        if (castMainAction != null)
        {
            castMainAction.started -= OnCastMainStarted;
            castMainAction.canceled -= OnCastMainCanceled;
            castMainAction = null;
        }

        if (castSubAction != null)
        {
            castSubAction.started -= OnCastSubStarted;
            castSubAction.canceled -= OnCastSubCanceled;
            castSubAction = null;
        }

        CleanupHeldAttackEffects(applyReleaseCooldown: false);
        CleanupHeldMovementEffects(applyReleaseCooldown: false);
    }

    void Update()
    {
        if (attackCDTimer > 0f) attackCDTimer -= Time.deltaTime;
        if (movementCDTimer > 0f) movementCDTimer -= Time.deltaTime;
    }

    /// <summary>
    /// 松开左键：结束主槽按住类技能（盾/疾跑若错放主槽）并可选进入攻击冷却。
    /// </summary>
    void CleanupHeldAttackEffects(bool applyReleaseCooldown)
    {
        bool hadTracked = activeMainSpell != null;
        if (activeMainSpell != null)
        {
            activeMainSpell.StopExecute();
            activeMainSpell = null;
        }

        if (applyReleaseCooldown && hadTracked && currentAttackSpell != null && currentAttackSpell.cooldownStartsOnRelease)
            attackCDTimer = currentAttackSpell.cooldownTime;
    }

    /// <summary>
    /// 松开右键 / 被打断 / 丢副武器：副槽按住类 + 疾跑 + 盾实例。
    /// </summary>
    void CleanupHeldMovementEffects(bool applyReleaseCooldown)
    {
        bool hadTracked = activeSubSpell != null;

        if (activeSubSpell != null)
        {
            activeSubSpell.StopExecute();
            activeSubSpell = null;
        }

        if (controller != null)
            controller.ClearSprintMultiplier();

        foreach (var s in GetComponentsInChildren<SprintSpell>(true))
        {
            if (s != null) Destroy(s.gameObject);
        }

        DestroyAllReflectShieldsUnderRoot();

        if (applyReleaseCooldown && hadTracked && currentMovementSpell != null && currentMovementSpell.cooldownStartsOnRelease)
            movementCDTimer = currentMovementSpell.cooldownTime;
    }

    // Send Messages 模式下 PlayerInput 会查找 OnCastMain/OnCastSub；实际逻辑在下方 InputAction 订阅。
    void OnCastMain(InputValue _) { }
    void OnCastSub(InputValue _) { }

    void OnCastMainStarted(InputAction.CallbackContext _)
    {
        if (currentAttackSpell == null) return;
        if (activeMainSpell != null) return;
        if (isKnockedDown || attackCDTimer > 0f) return;

        DestroyAllReflectShieldsUnderRoot();
        activeMainSpell = ExecuteAndReturnSpell(currentAttackSpell, ref attackCDTimer);
    }

    void OnCastMainCanceled(InputAction.CallbackContext _)
    {
        CleanupHeldAttackEffects(applyReleaseCooldown: true);
    }

    void OnCastSubStarted(InputAction.CallbackContext _)
    {
        if (currentMovementSpell == null) return;
        if (activeSubSpell != null) return;
        if (isKnockedDown || movementCDTimer > 0f) return;

        activeSubSpell = ExecuteAndReturnSpell(currentMovementSpell, ref movementCDTimer);
    }

    void OnCastSubCanceled(InputAction.CallbackContext _)
    {
        CleanupHeldMovementEffects(applyReleaseCooldown: true);
    }

    void DestroyAllReflectShieldsUnderRoot()
    {
        Transform root = transform.root;
        foreach (ReflectShieldSpell sh in root.GetComponentsInChildren<ReflectShieldSpell>(true))
        {
            if (sh == null) continue;
            if ((Object)activeSubSpell == (Object)sh)
                activeSubSpell = null;
            if ((Object)activeMainSpell == (Object)sh)
                activeMainSpell = null;
            Destroy(sh.gameObject);
        }
    }

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

        SpellProjectile[] strayProjectiles = dropObj.GetComponentsInChildren<SpellProjectile>(true);
        for (int i = 0; i < strayProjectiles.Length; i++)
            Object.DestroyImmediate(strayProjectiles[i]);

        SpellPickup pickup = dropObj.GetComponentInChildren<SpellPickup>(true);
        if (pickup == null)
        {
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

        CleanupHeldAttackEffects(applyReleaseCooldown: false);

        DropSpell(currentAttackSpell);
        currentAttackSpell = null;
        Debug.Log("丢弃了主武器");
    }

    void OnDropMovement(InputValue value)
    {
        if (!value.isPressed || currentMovementSpell == null) return;

        CleanupHeldMovementEffects(applyReleaseCooldown: false);

        DropSpell(currentMovementSpell);
        currentMovementSpell = null;
        Debug.Log("丢弃了副武器");
    }

    SpellBehavior ExecuteAndReturnSpell(SpellData data, ref float cdTimer)
    {
        if (data == null || data.spellPrefab == null) return null;

        Vector3 spawnPos = firePoint.position + firePoint.up * spellSpawnForwardInset;
        GameObject spellObj = Instantiate(data.spellPrefab, spawnPos, firePoint.rotation);

        SpellProjectile.RegisterWithCaster(spellObj, gameObject);

        SpellBehavior behavior = spellObj.GetComponentInChildren<SpellBehavior>(true);
        if (behavior != null)
            behavior.Execute(gameObject, firePoint);

        if (!data.cooldownStartsOnRelease)
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
        CleanupHeldAttackEffects(applyReleaseCooldown: false);
        CleanupHeldMovementEffects(applyReleaseCooldown: false);

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
        CleanupHeldAttackEffects(applyReleaseCooldown: false);
        CleanupHeldMovementEffects(applyReleaseCooldown: false);
        Debug.Log(gameObject.name + " 被淘汰了！");
        gameObject.SetActive(false);
    }
}
