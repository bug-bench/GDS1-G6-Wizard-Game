using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Stats")]
    //public int health = 100;
    public bool isKnockedDown = false;
    public Transform firePoint;

    [Header("Equipped Spells (双槽位 / Dual Slots)")]
    public SpellData currentAttackSpell;
    public SpellData currentMovementSpell;

    [Header("Drop Settings")]
    public float dropForce = 8f;

    [Header("施法生成 / Spell Spawn")]
    [Tooltip("法术预制体生成点沿枪口前移，减少与自身碰撞体重叠（直接拖火球当 spellPrefab 时也生效）。 — Offset spell spawn along fire point forward to reduce overlap with the caster (also applies when using a fireball prefab as spellPrefab).")]
    public float spellSpawnForwardInset = 0.28f;

    private float attackCDTimer;
    private float movementCDTimer;
    private SpellBehavior activeMainSpell;
    private SpellBehavior activeSubSpell;
    /// <summary>
    /// Execute 内立刻 Destroy 的法术（火球/霰弹等）松键时 active 引用已丢，仍要按 SpellData 进「松开冷却」。
    /// For spells that Destroy themselves in Execute (fireball, shotgun, etc.), the active reference is gone on release; this flag still applies release-based cooldown from SpellData.
    /// </summary>
    private bool pendingMainReleaseCooldown;
    private bool pendingSubReleaseCooldown;

    private PlayerController controller;
    private Color originalColor;

    private PlayerInput playerInput;
    private InputAction castMainAction;
    private InputAction castSubAction;
    private PlayerStats playerStats;
    void Awake()
    {
        controller = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;
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
    /// On main-button release: end held main-slot spells (shield/sprint if wrongly on main) and optionally start attack cooldown.
    /// </summary>
    void CleanupHeldAttackEffects(bool applyReleaseCooldown)
    {
        bool hadTracked = activeMainSpell != null;
        if (activeMainSpell != null)
        {
            activeMainSpell.StopExecute();
            activeMainSpell = null;
        }

        if (applyReleaseCooldown && currentAttackSpell != null && currentAttackSpell.cooldownStartsOnRelease
            && (hadTracked || pendingMainReleaseCooldown))
            attackCDTimer = currentAttackSpell.cooldownTime;
        pendingMainReleaseCooldown = false;
    }

    /// <summary>
    /// 松开右键 / 被打断 / 丢副武器：副槽按住类 + 疾跑 + 盾实例。
    /// On sub-button release / interrupt / drop sub-weapon: clear held sub-slot, sprint, and shield instances.
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

        if (applyReleaseCooldown && currentMovementSpell != null && currentMovementSpell.cooldownStartsOnRelease
            && (hadTracked || pendingSubReleaseCooldown))
            movementCDTimer = currentMovementSpell.cooldownTime;
        pendingSubReleaseCooldown = false;
    }

    // Send Messages 模式下 PlayerInput 会查找 OnCastMain/OnCastSub；实际逻辑在下方 InputAction 订阅。
    // With Send Messages, PlayerInput looks up OnCastMain/OnCastSub; real logic is wired via InputAction callbacks below.
    void OnCastMain(InputValue _) { }
    void OnCastSub(InputValue _) { }

    void OnCastMainStarted(InputAction.CallbackContext _)
    {
        if (currentAttackSpell == null) return;
        if (currentAttackSpell.spellPrefab == null) return;
        if (activeMainSpell != null) return;
        if (isKnockedDown || attackCDTimer > 0f) return;

        DestroyAllReflectShieldsUnderRoot();
        activeMainSpell = ExecuteAndReturnSpell(currentAttackSpell, ref attackCDTimer);
        if (currentAttackSpell.cooldownStartsOnRelease)
            pendingMainReleaseCooldown = true;
    }

    void OnCastMainCanceled(InputAction.CallbackContext _)
    {
        CleanupHeldAttackEffects(applyReleaseCooldown: true);
    }

    void OnCastSubStarted(InputAction.CallbackContext _)
    {
        if (currentMovementSpell == null) return;
        if (currentMovementSpell.spellPrefab == null) return;
        if (activeSubSpell != null) return;
        if (isKnockedDown || movementCDTimer > 0f) return;

        activeSubSpell = ExecuteAndReturnSpell(currentMovementSpell, ref movementCDTimer);
        if (currentMovementSpell.cooldownStartsOnRelease)
            pendingSubReleaseCooldown = true;
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
            Debug.Log("主武器(左键) Main: " + newSpell.spellName);
            return true;
        }

        if (currentMovementSpell == null)
        {
            currentMovementSpell = newSpell;
            Debug.Log("副武器(右键) Sub: " + newSpell.spellName);
            return true;
        }

        Debug.Log("主副武器都已装备，先 Q/E 丢掉一把再捡。 | Both slots full; press Q/E to drop one before picking up.");
        return false;
    }

    void DropSpell(SpellData dataToDrop)
    {
        if (dataToDrop == null) return;
        if (dataToDrop.pickupPrefab == null)
        {
            Debug.LogWarning($"丢弃失败：「{dataToDrop.spellName}」的 SpellData 未指定 pickupPrefab，地上不会出现武器。 | Drop failed: '{dataToDrop.spellName}' SpellData has no pickupPrefab.");
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
            Debug.LogWarning($"「{dropObj.name}」实例上未找到 SpellPickup，已在根节点自动添加。请在工程里打开 Pickup 预制体检查引用并点 Apply，避免只靠运行时兜底。 | No SpellPickup on '{dropObj.name}'; added at root. Fix the pickup prefab in the project.");
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
        Debug.Log("丢弃了主武器 | Dropped main weapon");
    }

    void OnDropMovement(InputValue value)
    {
        if (!value.isPressed || currentMovementSpell == null) return;

        CleanupHeldMovementEffects(applyReleaseCooldown: false);

        DropSpell(currentMovementSpell);
        currentMovementSpell = null;
        Debug.Log("丢弃了副武器 | Dropped sub weapon");
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

    public void TakeDamage(int damage, int attackerIndex = -1)
    {
        Debug.Log($"TakeDamage called — damage: {damage}, attackerIndex: {attackerIndex}");
        if (isKnockedDown) return;
        if (playerStats == null) return;

        // Record damage dealt by attacking player
        if (attackerIndex >= 0)
            GameData.RecordDamage(attackerIndex, damage);
            Debug.Log($"RecordDamage called — attackerIndex: {attackerIndex}, amount: {damage}, total now: {GameData.players.Find(p => p.playerIndex == attackerIndex)?.damageDealt}");

        playerStats.TakeDamage(damage);

        if (playerStats.health <= 0)
        {
            // Record kill for attacking player
            if (attackerIndex >= 0)
                GameData.RecordKill(attackerIndex);

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

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.gray;

        yield return new WaitForSeconds(1.5f);

        if (sr != null)
            sr.color = originalColor;
        controller.canMove = true;
        isKnockedDown = false;
    }

    void Die()
    {
        CleanupHeldAttackEffects(applyReleaseCooldown: false);
        CleanupHeldMovementEffects(applyReleaseCooldown: false);
        Debug.Log(gameObject.name + " 被淘汰了！ | Eliminated!");
        gameObject.SetActive(false);
    }
}
