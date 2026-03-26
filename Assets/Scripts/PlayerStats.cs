
using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float health = 100f;
    public float speed = 5f;
    public float strength = 10f;

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
            //Die();
        }
    }

    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0f, float.MaxValue);

        Debug.Log(gameObject.name + " healed. Health: " + health);
    }

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
