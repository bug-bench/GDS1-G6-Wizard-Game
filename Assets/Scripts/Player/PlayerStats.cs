
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;


public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PickupPrefabHolder pickupHolder;
    [SerializeField] private float dropForce = 5f;
    private List<string> lastCollectedPickups = new List<string>();
    private Phase1Script p1s;
    private float nextDropThreshold;

    [Header("Base Stats / 核心基础属性")]
    [Tooltip("当前生命值。受到伤害会减少，吃道具会增加（无上限），归零时玩家死亡。\nCurrent health. Reduces on taking damage, increases on healing (no cap), player dies when it reaches 0.")]
    public float health = 100f;

    public float MaxHealth = 100f;

    [Tooltip("移动速度。数值越高跑得越快，但惯性（刹车距离）也会变大。\nMovement speed. Higher value means faster movement, but also increases inertia (stopping distance).")]
    public float speed = 5f;

    [Tooltip("减速度（刹车能力）。数值越高，停下得越快（惯性越小）。\nDeceleration (braking power). Higher value means faster stopping (less inertia).")]
    public float deceleration = 50f;

    [Tooltip("法术强度（力量）。直接附加到法术的基础伤害上（总伤害 = 法术伤害 + Strength）。\nSpell power (Strength). Added directly to base spell damage (Total Damage = Spell Damage + Strength).")]
    public float strength = 10f;

    [Tooltip("护甲 / 防御力。使用 LOL 护甲公式：实际受到伤害 = 原始伤害 * (100 / (100 + Defense))。\nArmor / Defense. Uses LOL armor formula: Actual Damage Taken = Raw Damage * (100 / (100 + Defense)).")]
    public float defense = 0f;

    [Tooltip("法术 Size 属性（默认 1.0，吃豆累加）。技能缩放公式见 SpellScalingConfig。\nRaw Size stat (default 1.0). See SpellScalingConfig for spell scale formula.")]
    public float sizeMultiplier = 1f;

    [Tooltip("留空则用场景 SpellScalingConfigProvider 或 Resources/SpellScalingConfig。\nOptional override; otherwise uses global SpellScalingConfig.")]
    public SpellScalingConfig scalingConfig;

    [Tooltip("冷却缩减 (Focus)。0.2 表示减少 20% 的技能冷却时间。最高生效 90%。\nCooldown reduction (Focus). 0.2 means 20% CD reduction. Capped at 90%.")]
    public float cooldownReduction = 0f;

    public bool IsAliveArena = true;

    [Header("Status Effects (Read Only)")]
    public float stunEndTime;
    public float rootEndTime;
    public bool isStunned => Time.time < stunEndTime;
    public bool isRooted => Time.time < rootEndTime;

    private Dictionary<string, int> statPickupCounts = new Dictionary<string, int>();

    void Start()
    {
        p1s = FindFirstObjectByType<Phase1Script>();

        if (p1s == null)
        {
            Debug.LogError("EventManager (Phase1Script) not found in scene!");
        }
        nextDropThreshold = MaxHealth - 20f;
    }

    void Update()
    {
        if (pickupHolder == null)
        {
            pickupHolder = FindFirstObjectByType<PickupPrefabHolder>();
        }
    }


    // =====================
    // PICKUP HANDLING
    // =====================

    public void RegisterPickup(string pickupID)
    {
        lastCollectedPickups.Add(pickupID);

        Debug.Log(gameObject.name + " picked up: " + pickupID);
    }

    // Add at the top with other private fields
    // Add this public method
    public int GetStatPickupCount(string statName)
    {
        return statPickupCounts.TryGetValue(statName, out int count) ? count : 0;
    }

    public void IncrementStatCount(string statName)
    {
        if (!statPickupCounts.ContainsKey(statName))
            statPickupCounts[statName] = 0;
        statPickupCounts[statName]++;
    }

    // =====================
    // HEALTH FUNCTIONS
    // =====================

    public void TakeDamage(float amount)
    {
        // LOL 护甲公式：实际伤害 = 原始伤害 * (100 / (100 + 护甲))
        // 如果 defense 是负数（比如被破甲），伤害会放大
        // LOL Armor Formula: Actual Damage = Raw Damage * (100 / (100 + Armor))
        // If defense is negative (armor shred), damage is amplified.
        float damageMultiplier = 100f / (100f + Mathf.Max(0, defense));
        float actualDamage = amount * damageMultiplier;

        health -= actualDamage;
        health = Mathf.Max(0f, health);

        Debug.Log($"{gameObject.name} took {actualDamage:F1} damage (Original: {amount}, Armor: {defense}). Health: {health:F1}");

        while (health <= nextDropThreshold && nextDropThreshold > 0)
            {
                int dropAmount = Random.Range(1, 3);
                DropRandomPickups(dropAmount);

                nextDropThreshold -= 20f;
            }

        if (health <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        float oldMaxHealth = MaxHealth;

        health += amount;

        if (health > MaxHealth) MaxHealth = health;

        if (MaxHealth > oldMaxHealth)
        {
            float maxHealthIncrease = MaxHealth - oldMaxHealth;
            nextDropThreshold += maxHealthIncrease;
        }

        Debug.Log(gameObject.name + " healed. Health: " + health);
    }

    public void RespawnHeal()
    {
        health = MaxHealth;
    }

    public float CurrentHealth() { return health; }

    // =====================
    // SPEED FUNCTIONS
    // =====================

    public void IncreaseSpeed(float amount)
    {
        speed += amount;
        Debug.Log(gameObject.name + " speed increased to: " + speed);
    }

    public void DecreaseSpeed(float amount)
    {
        speed -= amount;
        speed = Mathf.Max(1f, speed);

        Debug.Log(gameObject.name + " speed decreased to: " + speed);
    }

    private Coroutine activeSpeedBoostCoroutine;
    private float speedBoostEndTime;
    private float currentSpeedBoostAmount;

    //for use with spells that increase speed temporarily
    public void ApplyTemporarySpeedBoost(float amount, float duration)
    {
        speedBoostEndTime = Time.time + duration;

        if (activeSpeedBoostCoroutine == null)
        {
            currentSpeedBoostAmount = amount;
            speed += currentSpeedBoostAmount;
            Debug.Log(gameObject.name + " speed boosted to: " + speed);
            activeSpeedBoostCoroutine = StartCoroutine(SpeedBoostRoutine());
        }
    }

    IEnumerator SpeedBoostRoutine()
    {
        while (Time.time < speedBoostEndTime)
        {
            yield return null;
        }

        speed -= currentSpeedBoostAmount;
        currentSpeedBoostAmount = 0f;
        activeSpeedBoostCoroutine = null;
        Debug.Log(gameObject.name + " speed boost ended. Speed: " + speed);
    }

    public float CurrentSpeed() { return speed; }

    // =====================
    // STRENGTH FUNCTIONS
    // =====================

    public void IncreaseStrength(float amount)
    {
        strength += amount;
        Debug.Log(gameObject.name + " strength increased to: " + strength);
    }

    public void DecreaseStrength(float amount)
    {
        strength -= amount;
        strength = Mathf.Max(1f, strength);

        Debug.Log(gameObject.name + " strength decreased to: " + strength);
    }

    public float CurrentStrength() { return strength; }

    public float GetEffectiveCooldown(float baseCooldown)
    {
        return SpellStatScaling.GetEffectiveCooldown(this, baseCooldown);
    }

    // =====================
    // GENERIC MODIFIER (用于吃豆人机制的统一接口 / Unified interface for Pac-Man style pickups)
    // =====================

    /// <summary>
    /// 供地图上的“小豆豆”道具调用的统一接口。
    /// Unified interface for map pickups ("dots") to modify player stats.
    /// </summary>
    /// <param name="statName">属性名称 (Health, Speed, Strength, Defense, Size, Focus)</param>
    /// <param name="amount">增加的数值 (Amount to add)</param>
    public void ModifyStat(string statName, float amount)
    {
        switch (statName)
        {
            case "Health":
                // 增加生命值（无上限） / Increase health (no cap)
                Heal(amount);
                break;
            case "Speed":
                // 增加移速 / Increase movement speed
                IncreaseSpeed(amount);
                break;
            case "Attack":
                // 增加法强 / Increase spell power
                IncreaseStrength(amount);
                break;
            case "Defense":
            case "Defence":
                // 增加护甲 / Increase armor（预制体拼写 Defence 也兼容）
                defense += amount;
                break;
            case "Mana":
                // 增加法术体积倍率 (例如传入 0.5 表示变大 50%) / Increase spell size multiplier (e.g., 0.5 means +50% size)
                sizeMultiplier += amount;
                break;
            case "Focus":
                // 增加冷却缩减 (例如传入 0.1 表示减 10% CD) / Increase CD reduction (e.g., 0.1 means 10% CDR)
                cooldownReduction += amount;
                // 负面效果：获得减 CD 的同时，小幅降低法强 (Trade-off: gain CDR but lose a bit of spell power)
                DecreaseStrength(amount * 10f); 
                break;
            case "Friction":
                deceleration += amount;
                break;
            default:
                Debug.LogWarning("Invalid stat name: " + statName);
                break;
        }
    }

    void RemoveStat(string statName)
    {
        switch (statName)
        {
            case "Health":
                MaxHealth -= 10f;
                health = Mathf.Min(health, MaxHealth);
                break;
            case "Speed":
                speed -= 1f;
                speed = Mathf.Max(1f, speed);
                break;
            case "Attack":
                strength -= 5f;
                strength = Mathf.Max(1f, strength);
                break;
            case "Defense":
                defense -= 1f;
                defense = Mathf.Max(0f, defense);
                break;
            case "Mana":
                sizeMultiplier -= 1f;
                sizeMultiplier = Mathf.Max(1f, sizeMultiplier);
                break;
            case "Focus":
                cooldownReduction -= 1f;
                cooldownReduction = Mathf.Max(1f, cooldownReduction);

                strength += 10f;
                break;
            case "Friction":
                deceleration -= 1f;
                deceleration = Mathf.Max(1f, deceleration);
                break;
            case "Range":
                //modify this if we switch back to range stat
                break;
            default:
                Debug.LogWarning("Invalid stat removal: " + statName);
                break;
        }
    }

    // =====================
    // PHASE ONE DEATH/DROP HANDLING
    // =====================

    public void Die()
    {
        Debug.Log(gameObject.name + " has died.");

        if (p1s != null && p1s.GetCurrentPhase() == 1)
        {
            int dropCount = Random.Range(2, 5);
            DropRandomPickups(dropCount);
        }

        Phase2Script p2 = p1s != null ? p1s.GetComponent<Phase2Script>() : null;

        if (p2 != null && p2.GetCurrentMinigame() == "Arena")
        {
            IsAliveArena = false;

            ArenaScript AS = FindFirstObjectByType<ArenaScript>();
            Debug.Log($"ArenaScript found: {AS != null}"); // ADD THIS
            if (AS != null) 
            {
                AS.PlayerEliminated(gameObject);
            }
        }
    }

    void DropRandomPickups(int dropCount)
    {
        if (lastCollectedPickups.Count == 0)
        {
            Debug.Log("No pickups to drop.");
            return;
        }

        if (pickupHolder == null)
        {
            Debug.LogWarning("PickupPrefabHolder is missing!");
            return;
        }

        for (int i = 0; i < dropCount; i++)
        {
            if (lastCollectedPickups.Count == 0) break;

            int index = Random.Range(0, lastCollectedPickups.Count);
            string droppedPickup = lastCollectedPickups[index];

            lastCollectedPickups.RemoveAt(index);
            RemoveStat(droppedPickup);

            GameObject pickupPrefab = pickupHolder.GetPickupReference(droppedPickup);

            if (pickupPrefab == null)
            {
                Debug.LogWarning("No prefab found for pickup: " + droppedPickup);
                continue;
            }

            Vector2 offset = Random.insideUnitCircle * 1f;

            GameObject spawnedPickup = Instantiate(
                pickupPrefab,
                (Vector2)transform.position + offset,
                Quaternion.identity
            );

            Rigidbody2D rb = spawnedPickup.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 force = Random.insideUnitCircle * dropForce;
                rb.AddForce(force, ForceMode2D.Impulse);
            }

            StatPickUp statPickUp = spawnedPickup.GetComponent<StatPickUp>();

            if (statPickUp != null)
            {
                statPickUp.RegisterLastPlayer(gameObject);
                statPickUp.StartDrop();
            }

            if (statPickupCounts.ContainsKey(droppedPickup))
            {
                statPickupCounts[droppedPickup]--;

                if (statPickupCounts[droppedPickup] <= 0)
                {
                    statPickupCounts.Remove(droppedPickup);
                }
            }

            Debug.Log(gameObject.name + " dropped: " + droppedPickup);
        }
    }

    // =====================
    // STATUS EFFECTS
    // =====================

    private Coroutine activeSlowCoroutine;
    private float slowEndTime;
    private float currentSpeedDrop;

    public void ApplySpeedMultiplier(float multiplier, float duration)
    {
        slowEndTime = Time.time + duration;

        if (activeSlowCoroutine == null)
        {
            currentSpeedDrop = speed * (1f - multiplier);
            speed -= currentSpeedDrop;
            Debug.Log($"{gameObject.name} speed multiplied by {multiplier}. New speed: {speed}");
            if (gameObject.activeInHierarchy)
            {
                activeSlowCoroutine = StartCoroutine(SlowRoutine());
            }
        }
    }

    IEnumerator SlowRoutine()
    {
        while (Time.time < slowEndTime)
        {
            yield return null;
        }

        speed += currentSpeedDrop;
        currentSpeedDrop = 0f;
        activeSlowCoroutine = null;
        Debug.Log($"{gameObject.name} speed restored. Speed: {speed}");
    }

    public void ApplyStun(float duration)
    {
        stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);
        Debug.Log($"{gameObject.name} is stunned for {duration}s");
    }

    public void ApplyRoot(float duration)
    {
        rootEndTime = Mathf.Max(rootEndTime, Time.time + duration);
        Debug.Log($"{gameObject.name} is rooted for {duration}s");
    }

    private Coroutine activeBurnCoroutine;
    private float burnEndTime;
    private int currentBurnDamagePerTick;
    private int currentBurnAttacker;

    public void ApplyBurn(int totalDamage, float duration, float tickInterval = 0.5f, int attackerIndex = -1)
    {
        int ticks = Mathf.FloorToInt(duration / tickInterval);
        if (ticks <= 0) ticks = 1;
        int damagePerTick = Mathf.Max(1, totalDamage / ticks);

        burnEndTime = Time.time + duration;
        currentBurnDamagePerTick = damagePerTick;
        currentBurnAttacker = attackerIndex;

        if (activeBurnCoroutine == null)
        {
            activeBurnCoroutine = StartCoroutine(BurnRoutine(tickInterval));
        }
    }

    IEnumerator BurnRoutine(float tickInterval)
    {
        while (Time.time < burnEndTime && IsAliveArena)
        {
            yield return new WaitForSeconds(tickInterval);
            if (Time.time >= burnEndTime) break;

            health -= currentBurnDamagePerTick;
            health = Mathf.Clamp(health, 0f, float.MaxValue);
            
            if (currentBurnAttacker >= 0)
                GameData.RecordDamage(currentBurnAttacker, currentBurnDamagePerTick);

            if (health <= 0)
            {
                if (currentBurnAttacker >= 0)
                    GameData.RecordKill(currentBurnAttacker);
                Die();
                break;
            }
        }
        activeBurnCoroutine = null;
    }
}
