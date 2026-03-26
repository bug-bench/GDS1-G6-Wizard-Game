
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

        if (p1s.GetCurrentPhase() == 1)
        {
            DropRandomPickups();
        }

        if (p2s.GetCurrentMinigame() == "Arena")
        {
            //add arena death logic here



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
}
