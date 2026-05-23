using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class PatternSettings
{
    public Spawner.PatternType patternType;

    [Header("Bullet")]
    public float bulletSpeed = 5f;
    public float bulletLifetime = 5f;

    [Header("Firing")]
    public float fireRate = 0.1f;
    public int bulletCount = 8;

    [Header("Rotation")]
    public float rotationSpeed = 90f;
    public float spreadAngle = 60f;
}

public class Spawner : MonoBehaviour
{
    public enum PatternType
    {
        Straight,
        Circle,
        Spin,
        Strafe,
        Spiral
    }

    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;

    [Header("Pattern Settings")]
    [SerializeField] private PatternType currentPattern;

    [Header("Pattern Configurations")]
    [SerializeField]
    private PatternSettings[] patternSettings;

    [Header("Pattern Switching")]
    [SerializeField] private float minPatternTime = 10f;
    [SerializeField] private float maxPatternTime = 15f;

    [Header("Startup")]
    [SerializeField] private float startupDelay = 5f;

    private float currentRotation;
    private float strafeAngle;
    private float strafeDirection = 1f;
    private PatternSettings currentSettings;
    private PatternType lastPattern;
    private PatternType secondLastPattern;

    private bool hasPatternHistory = false;

    private Transform targetPlayer;

    private void Start()
    {
        ChooseRandomPattern();

        StartCoroutine(FireRoutine());
        StartCoroutine(PatternRoutine());
    }

    // ==================================================
    // PATTERN SWITCHING
    // ==================================================

    private IEnumerator PatternRoutine()
    {
        while (true)
        {
            ChooseRandomPattern();

            float waitTime =
                Random.Range(minPatternTime, maxPatternTime);

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void ChooseRandomPattern()
    {
        if (patternSettings.Length == 0)
            return;

        List<PatternSettings> validPatterns = new List<PatternSettings>();

        foreach (PatternSettings pattern in patternSettings)
        {
            if (!hasPatternHistory)
            {
                validPatterns.Add(pattern);
                continue;
            }

            if (pattern.patternType != lastPattern && pattern.patternType != secondLastPattern)
            {
                validPatterns.Add(pattern);
            }
        }

        // Safety fallback
        if (validPatterns.Count == 0)
        {
            validPatterns.AddRange(patternSettings);
        }

        int randomIndex = Random.Range(0, validPatterns.Count);

        PatternSettings chosenPattern = validPatterns[randomIndex];

        // Shift history
        secondLastPattern = lastPattern;
        lastPattern = chosenPattern.patternType;

        hasPatternHistory = true;

        currentSettings = chosenPattern;
        currentPattern = chosenPattern.patternType;

        Debug.Log("New Pattern: " + currentPattern);
    }

    // ==================================================
    // FIRING
    // ==================================================

    private IEnumerator FireRoutine()
    {
        yield return new WaitForSeconds(startupDelay);

        while (true)
        {
            FindClosestPlayer();

            FirePattern();

            yield return new WaitForSeconds(currentSettings.fireRate);
        }
    }

    private void FirePattern()
    {
        switch (currentPattern)
        {
            case PatternType.Straight:
                FireStraight();
                break;

            case PatternType.Circle:
                FireCircle();
                break;

            case PatternType.Spin:
                FireSpin();
                break;

            case PatternType.Strafe:
                FireStrafe();
                break;

            case PatternType.Spiral:
                FireSpiral();
                break;
        }
    }

    // ==================================================
    // PATTERNS
    // ==================================================

    // ---------- STRAIGHT ----------
    // Targets closest player

    private void FireStraight()
    {
        if (targetPlayer == null)
            return;

        Vector2 direction =
            (targetPlayer.position - transform.position).normalized;

        ShootBullet(direction);
    }

    // ---------- CIRCLE ----------

    private void FireCircle()
    {
        float angleStep = 360f / currentSettings.bulletCount;

        for (int i = 0; i < currentSettings.bulletCount; i++)
        {
            float angle = angleStep * i;

            Vector2 direction =
                DirectionFromAngle(angle);

            ShootBullet(direction);
        }
    }

    // ---------- SPIN ----------
    // Touhou style rotating burst

    private void FireSpin()
    {
        currentRotation += currentSettings.rotationSpeed * currentSettings.fireRate;

        float angleStep = 360f / currentSettings.bulletCount;

        for (int i = 0; i < currentSettings.bulletCount; i++)
        {
            float angle =
                angleStep * i + currentRotation;

            Vector2 direction =
                DirectionFromAngle(angle);

            ShootBullet(direction);
        }
    }

    // ---------- STRAFE ----------
    // Sweeps back and forth

    private void FireStrafe()
    {
        strafeAngle +=
            currentSettings.rotationSpeed * currentSettings.fireRate * strafeDirection;

        if (Mathf.Abs(strafeAngle) >= currentSettings.spreadAngle)
        {
            strafeDirection *= -1f;
        }

        Vector2 direction =
            DirectionFromAngle(strafeAngle);

        ShootBullet(direction);
    }

    // ---------- SPIRAL ----------
    // Continuous rotating stream

    private void FireSpiral()
    {
        currentRotation += currentSettings.rotationSpeed * currentSettings.fireRate;

        Vector2 direction =
            DirectionFromAngle(currentRotation);

        ShootBullet(direction);
    }

    // ==================================================
    // PLAYER TARGETING
    // ==================================================

    private void FindClosestPlayer()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 0)
        {
            targetPlayer = null;
            return;
        }

        float closestDistance = Mathf.Infinity;

        Transform closest = null;

        foreach (GameObject player in players)
        {
            float distance =
                Vector2.Distance(
                    transform.position,
                    player.transform.position
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = player.transform;
            }
        }

        targetPlayer = closest;
    }

    // ==================================================
    // SHOOTING
    // ==================================================

    private void ShootBullet(Vector2 direction)
    {
        GameObject bulletObj =
            BulletPool.Instance.GetBullet();

        bulletObj.transform.position =
            transform.position;

        bulletObj.transform.rotation =
            Quaternion.identity;

        Bullet bullet =
            bulletObj.GetComponent<Bullet>();

        bullet.Initialize(
            direction,
            currentSettings.bulletSpeed,
            currentSettings.bulletLifetime
        );
    }

    // ==================================================
    // HELPERS
    // ==================================================

    private Vector2 DirectionFromAngle(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;

        return new Vector2(
            Mathf.Cos(radians),
            Mathf.Sin(radians)
        );
    }
}