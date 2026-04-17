
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

    //for use with spells that increase speed temporarily
    public void ApplyTemporarySpeedBoost(float amount, float duration)
    {
        StartCoroutine(SpeedBoostCoroutine(amount, duration));
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

    void Die()
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

        // if (p2s != null && p2s.GetCurrentMinigame() == "Arena")
        // {
        //     IsAliveArena = false;
        //     ArenaScript AS = FindFirstObjectByType<ArenaScript>();
        //     Debug.Log($"ArenaScript found: {AS != null}"); // ADD THIS
        //     if (AS != null) 
        //     {
        //         AS.PlayerEliminated(gameObject);
        //     }
        // }
        // else
        // {
        //     Debug.Log($"Die() — p2s null: {p2s == null}, minigame: {p2s?.GetCurrentMinigame()}");
        // }
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
    // COROUTINES
    // =====================

        IEnumerator SpeedBoostCoroutine(float amount, float duration)
    {
        //Apply boost
        speed += amount;
        Debug.Log(gameObject.name + " speed boosted to: " + speed);

        //Wait for duration
        yield return new WaitForSeconds(duration);

        //Remove boost
        speed -= amount;
        Debug.Log(gameObject.name + " speed boost ended. Speed: " + speed);
    }

    // =====================
    // STATUS EFFECTS
    // =====================

    public void ApplySpeedMultiplier(float multiplier, float duration)
    {
        StartCoroutine(SpeedMultiplierCoroutine(multiplier, duration));
    }

    IEnumerator SpeedMultiplierCoroutine(float multiplier, float duration)
    {
        float speedDrop = speed * (1f - multiplier);
        speed -= speedDrop;
        Debug.Log($"{gameObject.name} speed multiplied by {multiplier}. New speed: {speed}");

        yield return new WaitForSeconds(duration);

        speed += speedDrop;
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

    public void ApplyBurn(int totalDamage, float duration, float tickInterval = 0.5f, int attackerIndex = -1)
    {
        StartCoroutine(BurnCoroutine(totalDamage, duration, tickInterval, attackerIndex));
    }

    IEnumerator BurnCoroutine(int totalDamage, float duration, float tickInterval, int attackerIndex)
    {
        int ticks = Mathf.FloorToInt(duration / tickInterval);
        if (ticks <= 0) ticks = 1;
        int damagePerTick = Mathf.Max(1, totalDamage / ticks);

        for (int i = 0; i < ticks; i++)
        {
            yield return new WaitForSeconds(tickInterval);
            if (!IsAliveArena) break;

            health -= damagePerTick;
            health = Mathf.Clamp(health, 0f, float.MaxValue);
            
            if (attackerIndex >= 0)
                GameData.RecordDamage(attackerIndex, damagePerTick);

            if (health <= 0)
            {
                if (attackerIndex >= 0)
                    GameData.RecordKill(attackerIndex);
                Die();
                break;
            }
        }
    }
}
