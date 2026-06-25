using UnityEngine;

public class DrunkSystem : MonoBehaviour
{
    [Header("Drunkness")]
    [Range(0f, 100f)]
    public float drunkness = 0f;

    [Header("Movement")]
    public float maxWobble = 0.25f;
    public float maxSpeedBonus = 0.5f;
    public float maxFrictionLoss = 0.6f;

    [Header("Combat")]
    public float maxDamageBonus = 1f;
    public float maxSizeBonus = 0.5f;
    public float maxAimSpread = 6f;

    [Header("Optional Chaos")]
    public bool enableJags = true;
    public float jagChancePerSecond = 0.02f;

    PlayerStats stats;
    PlayerController controller;
    PlayerCombat combat;
    Rigidbody2D rb;
    Transform firePoint;

    float baseSpeed;
    float baseStrength;
    float baseSize;
    float baseDecel;

    float nextJagTime;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        controller = GetComponent<PlayerController>();
        combat = GetComponent<PlayerCombat>();
        rb = GetComponent<Rigidbody2D>();

        if (combat != null)
            firePoint = combat.firePoint;

        if (stats == null || controller == null || combat == null)
        {
            Debug.LogError("BeerPrototypeSystem missing required components.");
            enabled = false;
            return;
        }

        baseSpeed = stats.speed;
        baseStrength = stats.strength;
        baseSize = stats.sizeMultiplier;
        baseDecel = stats.deceleration;
    }

    void Update()
    {
        float t = drunkness / 100f;

        // STATS MODIF
        stats.speed = baseSpeed * (1f + maxSpeedBonus * t);
        stats.strength = baseStrength * (1f + maxDamageBonus * t);
        stats.sizeMultiplier = baseSize * (1f + maxSizeBonus * t);
        stats.deceleration = baseDecel * (1f - maxFrictionLoss * t);

        // MOVEMENT WOBBLE 
        if (controller != null)
        {
            float wobble = Mathf.Sin(Time.time * 6f) * maxWobble * t;
            controller.moveInput.x += wobble;
        }

        // AIM WOBBLE (SPREADS OUT SOMETIMES)
        if (controller != null && controller.rotationPivot != null)
        {
            float angleWobble = Mathf.Sin(Time.time * 4f) * (maxAimSpread * 0.3f) * t;
            controller.rotationPivot.Rotate(0, 0, angleWobble * Time.deltaTime);
        }

        // RANDOM JAG
        if (enableJags && t > 0.4f)
        {
            if (Time.time > nextJagTime)
            {
                nextJagTime = Time.time + Random.Range(3f, 6f);

                if (Random.value < jagChancePerSecond * 10f * t)
                {
                    if (rb != null)
                        rb.AddForce(Random.insideUnitCircle * 2f, ForceMode2D.Impulse);
                }
            }
        }
    }

    // replace GameObject spellObj =
    //     Instantiate(data.spellPrefab, spawnPos, rot);
    //
    // with
    //
    // Quaternion rot = firePoint.rotation;

    // var beer = GetComponent<DrunkSystem>();
    // if (beer != null)
    //     rot = beer.ApplyAimSpread(rot);
    // GameObject spellObj =
    //     Instantiate(data.spellPrefab, spawnPos, rot);

    public Quaternion ApplyAimSpread(Quaternion rot)
    {
        float t = drunkness / 100f;
        float spread = Random.Range(-maxAimSpread, maxAimSpread) * t;
        return rot * Quaternion.Euler(0, 0, spread);
    }

    public Vector2 ApplyProjectileSpread(Vector2 dir)
    {
        float t = drunkness / 100f;
        float spread = Random.Range(-maxAimSpread, maxAimSpread) * t;
        return Quaternion.Euler(0, 0, spread) * dir;
    }

    // DEBUG
    public void AddBeer(float amount)
    {
        drunkness = Mathf.Clamp(drunkness + amount, 0f, 100f);
    }

    public void RemoveBeer(float amount)
    {
        drunkness = Mathf.Clamp(drunkness - amount, 0f, 100f);
    }
}