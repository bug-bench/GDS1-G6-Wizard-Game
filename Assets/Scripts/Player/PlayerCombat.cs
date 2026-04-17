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

    [Header("Hit feedback (Isaac-style / 以撒受击)")]
    [Range(0f, 40f)]
    [Tooltip("受击沿击退方向的冲量（Rigidbody2D）。 — Impulse knockback on hit.")]
    public float hitKnockbackImpulse = 7f;
    [Tooltip("闪白颜色。 — Sprite tint during hit frames.")]
    public Color hitFlashColor = Color.white;
    [Range(0, 12)]
    [Tooltip("闪白次数（每次先白再回原色）。 — Number of white flash cycles.")]
    public int hitFlashBlinkCount = 3;
    [Range(0.01f, 0.35f)]
    [Tooltip("单次闪白或恢复的时长（秒）。 — One white-flash half-cycle in seconds.")]
    public float hitFlashHalfDuration = 0.04f;

    [Header("Invincibility / 无敌帧")]
    [Range(0.1f, 5f)]
    [Tooltip("受伤后多少秒内免疫伤害。 — Invulnerability duration after hit.")]
    public float invincibilityDuration = 1.5f;
    [Range(0.05f, 2f)]
    [Tooltip("无敌闪烁：每段「不透明」的时长（秒）。 — Seconds renderer stays opaque per blink.")]
    public float invincibilityBlinkVisibleDuration = 0.05f;
    [Range(0.05f, 2f)]
    [Tooltip("无敌闪烁：每段「全透明」的时长（秒）。 — Seconds renderer stays transparent per blink.")]
    public float invincibilityBlinkHiddenDuration = 0.05f;
    [Tooltip("参与无敌闪烁的精灵（通过 alpha 开关显示）；留空则自动使用根/子物体上的第一个 SpriteRenderer。 — Sprites toggled during i-blink; empty = auto-find one body sprite.")]
    public SpriteRenderer[] invincibilityBlinkSpriteRenderers;

    private float invincibleUntil;
    private Rigidbody2D playerRb;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer[] _invincibilityBlinkTargets;
    private Color[] _invincibilityBlinkOriginalColors;
    private Coroutine hitFeedbackRoutine;

    private PlayerInput playerInput;
    private InputAction castMainAction;
    private InputAction castSubAction;
    private PlayerStats playerStats;
    void Awake()
    {
        controller = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
        playerRb = GetComponent<Rigidbody2D>();
        BuildInvincibilityBlinkTargets();
    }

    void BuildInvincibilityBlinkTargets()
    {
        int validCount = 0;
        if (invincibilityBlinkSpriteRenderers != null)
        {
            foreach (var s in invincibilityBlinkSpriteRenderers)
            {
                if (s != null) validCount++;
            }
        }

        if (validCount > 0)
        {
            _invincibilityBlinkTargets = new SpriteRenderer[validCount];
            int i = 0;
            foreach (var s in invincibilityBlinkSpriteRenderers)
            {
                if (s != null)
                    _invincibilityBlinkTargets[i++] = s;
            }
        }
        else
        {
            var sr = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            _invincibilityBlinkTargets = sr != null ? new[] { sr } : System.Array.Empty<SpriteRenderer>();
        }

        // 初始化 OriginalColors 数组，但此时的颜色可能是白色的预制体默认颜色
        _invincibilityBlinkOriginalColors = new Color[_invincibilityBlinkTargets.Length];
        for (int i = 0; i < _invincibilityBlinkTargets.Length; i++)
        {
            if (_invincibilityBlinkTargets[i] != null)
                _invincibilityBlinkOriginalColors[i] = _invincibilityBlinkTargets[i].color;
        }

        spriteRenderer = _invincibilityBlinkTargets.Length > 0 ? _invincibilityBlinkTargets[0] : null;
    }

    /// <summary>
    /// 在 PlayerSpawner 修改完玩家颜色后调用，更新闪烁恢复时的基准颜色。
    /// Called after PlayerSpawner sets the player's color, so blinking restores to the correct color.
    /// </summary>
    public void UpdateOriginalBlinkColors()
    {
        if (_invincibilityBlinkTargets == null || _invincibilityBlinkOriginalColors == null) return;
        for (int i = 0; i < _invincibilityBlinkTargets.Length; i++)
        {
            if (_invincibilityBlinkTargets[i] != null)
                _invincibilityBlinkOriginalColors[i] = _invincibilityBlinkTargets[i].color;
        }
    }

    void SetInvincibilityBlinkAlpha(float alpha)
    {
        if (_invincibilityBlinkTargets == null) return;
        for (int i = 0; i < _invincibilityBlinkTargets.Length; i++)
        {
            if (_invincibilityBlinkTargets[i] != null)
            {
                Color c = _invincibilityBlinkTargets[i].color; // 获取当前颜色（包括被 Spawner 换过的颜色）
                c.a = alpha;
                _invincibilityBlinkTargets[i].color = c;
            }
        }
    }

    void SetAllInvincibilityBlinkColors(Color c)
    {
        if (_invincibilityBlinkTargets == null) return;
        foreach (var s in _invincibilityBlinkTargets)
        {
            if (s != null)
                s.color = c;
        }
    }

    void RestoreInvincibilityBlinkOriginalColors()
    {
        if (_invincibilityBlinkTargets == null || _invincibilityBlinkOriginalColors == null) return;
        for (int i = 0; i < _invincibilityBlinkTargets.Length; i++)
        {
            if (_invincibilityBlinkTargets[i] != null)
                _invincibilityBlinkTargets[i].color = _invincibilityBlinkOriginalColors[i];
        }
    }

    void RestoreInvincibilityVisuals()
    {
        RestoreInvincibilityBlinkOriginalColors();
    }

    /// <summary>弹道等可查询：无敌期间不扣血且弹丸应穿过。 — True while post-hit i-frames are active.</summary>
    public bool IsInvincible => Time.time < invincibleUntil;

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

        if (hitFeedbackRoutine != null)
        {
            StopCoroutine(hitFeedbackRoutine);
            hitFeedbackRoutine = null;
        }

        invincibleUntil = 0f;
        RestoreInvincibilityVisuals();
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
        if (playerStats != null && playerStats.isStunned) return;

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
        if (playerStats != null && playerStats.isStunned) return;

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

    /// <param name="knockbackWorldDir">击退方向（世界空间），零向量则只做闪白不做冲量。 — World-space push direction; zero = flash only.</param>
    public void TakeDamage(int damage, int attackerIndex = -1, Vector2 knockbackWorldDir = default)
    {
        if (isKnockedDown) return;
        if (playerStats == null)
        {
            Debug.LogWarning($"{name}: TakeDamage ignored — no PlayerStats.");
            return;
        }

        if (Time.time < invincibleUntil)
            return;

        // 先开无敌窗口，避免同一帧多发子弹重复结算。 — Open i-frames before applying damage to block same-frame multi-hit.
        invincibleUntil = Time.time + invincibilityDuration;

        if (attackerIndex >= 0)
            GameData.RecordDamage(attackerIndex, damage);

        playerStats.TakeDamage(damage);

        if (playerRb != null && knockbackWorldDir.sqrMagnitude > 0.0001f)
            playerRb.AddForce(knockbackWorldDir.normalized * hitKnockbackImpulse, ForceMode2D.Impulse);

        if (_invincibilityBlinkTargets != null && _invincibilityBlinkTargets.Length > 0)
        {
            if (hitFeedbackRoutine != null)
                StopCoroutine(hitFeedbackRoutine);
            hitFeedbackRoutine = StartCoroutine(HitInvincibilityVisualRoutine());
        }

        if (playerStats.health <= 0)
        {
            if (attackerIndex >= 0)
                GameData.RecordKill(attackerIndex);
            Die();
        }
    }

    IEnumerator HitInvincibilityVisualRoutine()
    {
        if (_invincibilityBlinkTargets == null || _invincibilityBlinkTargets.Length == 0)
        {
            Debug.LogWarning($"{name}: HitInvincibilityVisualRoutine aborted because _invincibilityBlinkTargets is empty!");
            hitFeedbackRoutine = null;
            yield break;
        }

        // Debug.Log($"{name}: Starting Hit Blink Routine. Targets count: {_invincibilityBlinkTargets.Length}");

        float endTime = invincibleUntil;

        for (int i = 0; i < hitFlashBlinkCount; i++)
        {
            SetAllInvincibilityBlinkColors(hitFlashColor);
            yield return new WaitForSeconds(hitFlashHalfDuration);
            RestoreInvincibilityBlinkOriginalColors();
            yield return new WaitForSeconds(hitFlashHalfDuration);
        }

        // 无敌剩余时间：可调「不透明 / 全透明」时长 — Toggle alpha between 100% and 0%.
        while (Time.time < endTime)
        {
            SetInvincibilityBlinkAlpha(0f);
            float waitOff = Mathf.Min(invincibilityBlinkHiddenDuration, endTime - Time.time);
            if (waitOff > 0f)
                yield return new WaitForSeconds(waitOff);
            if (Time.time >= endTime) break;

            RestoreInvincibilityBlinkOriginalColors();
            float waitOn = Mathf.Min(invincibilityBlinkVisibleDuration, endTime - Time.time);
            if (waitOn > 0f)
                yield return new WaitForSeconds(waitOn);
        }

        RestoreInvincibilityVisuals();
        hitFeedbackRoutine = null;
    }

    void Die()
    {
        CleanupHeldAttackEffects(applyReleaseCooldown: false);
        CleanupHeldMovementEffects(applyReleaseCooldown: false);

        if (hitFeedbackRoutine != null)
        {
            StopCoroutine(hitFeedbackRoutine);
            hitFeedbackRoutine = null;
        }

        RestoreInvincibilityVisuals();

        invincibleUntil = 0f;

        Debug.Log(gameObject.name + " 被淘汰了！ | Eliminated!");
        gameObject.SetActive(false);
    }
}
