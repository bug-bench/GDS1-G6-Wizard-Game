
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerStats : MonoBehaviour
{
    private List<string> collectedPickups = new List<string>();
    private Phase1Script p1s;
    private Phase2Script p2s;

    [Header("Base Stats")]
    public float health = 100f;
    public float speed = 5f;
    public float strength = 10f;
    public bool IsAliveArena = true;

    [Header("Status Effects (Read Only)")]
    public float stunEndTime;
    public float rootEndTime;
    public bool isStunned => Time.time < stunEndTime;
    public bool isRooted => Time.time < rootEndTime;

    void Start()
    {
        p1s = FindFirstObjectByType<Phase1Script>();
        p2s = FindFirstObjectByType<Phase2Script>();

        if (p1s == null)
        {
            Debug.LogError("EventManager (Phase1Script) not found in scene!");
        }
    }
    

    // =====================
    // PICKUP HANDLING
    // =====================

    public void RegisterPickup(string pickupID)
    {
        collectedPickups.Add(pickupID);
        Debug.Log(gameObject.name + " picked up: " + pickupID);
    }

    // =====================
    // HEALTH FUNCTIONS
    // =====================

    public void TakeDamage(float amount)
    {
        health -= amount;
        health = Mathf.Clamp(health, 0f, float.MaxValue);

        Debug.Log(gameObject.name + " took damage. Health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0f, float.MaxValue);

        Debug.Log(gameObject.name + " healed. Health: " + health);
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

    // =====================
    // GENERIC MODIFIER (not really needed entirely but useful)
    // =====================

    public void ModifyStat(string statName, float amount)
    {
        switch (statName)
        {
            case "Health":
                Heal(amount);
                break;
            case "Speed":
                IncreaseSpeed(amount);
                break;
            case "Strength":
                IncreaseStrength(amount);
                break;
            default:
                Debug.LogWarning("Invalid stat name: " + statName);
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
            DropRandomPickups();

        IsAliveArena = false;
        ArenaScript AS = FindFirstObjectByType<ArenaScript>();
        Debug.Log($"ArenaScript found: {AS != null}"); // ADD THIS
        if (AS != null) 
        {
            AS.PlayerEliminated(gameObject);
        }
    }

    void DropRandomPickups()
    {
        if (collectedPickups.Count == 0)
        {
            Debug.Log("No pickups to drop.");
            return;
        }

        int dropCount = Random.Range(2, 4);

        for (int i = 0; i < dropCount; i++)
        {
            if (collectedPickups.Count == 0) break;

            int index = Random.Range(0, collectedPickups.Count);
            string droppedPickup = collectedPickups[index];

            collectedPickups.RemoveAt(index);

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
            activeSlowCoroutine = StartCoroutine(SlowRoutine());
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
