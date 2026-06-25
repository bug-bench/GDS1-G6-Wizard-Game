using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;


[System.Serializable]
public class PatternSettings
{
    public SurvivalBulletSpawner.PatternType patternType;

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

public class SurvivalBulletSpawner : MonoBehaviour
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
    private Vector2 fallbackDirection;
    private PatternSettings currentSettings;
    private PatternType lastPattern;
    private PatternType secondLastPattern;

    private bool hasPatternHistory = false;

    private Transform targetPlayer;

    private SurvivalScript survival;

    private void Start()
    {
        fallbackDirection = Random.insideUnitCircle.normalized;

        if (fallbackDirection == Vector2.zero)
        {
            fallbackDirection = Vector2.right;
        }

        survival = FindFirstObjectByType<SurvivalScript>();

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

        if (targetPlayer == null)
        {
            fallbackDirection = Random.insideUnitCircle.normalized;

            if (fallbackDirection == Vector2.zero)
            {
                fallbackDirection = Vector2.right;
            }
        }

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
        ShootBullet(GetTargetDirection());
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
            currentSettings.rotationSpeed * 
            currentSettings.fireRate * 
            strafeDirection;

        if (Mathf.Abs(strafeAngle) >= currentSettings.spreadAngle)
        {
            strafeDirection *= -1f;
        }

        Vector2 baseDirection = GetTargetDirection();

        float baseAngle =
            Mathf.Atan2(baseDirection.y, baseDirection.x) *
            Mathf.Rad2Deg;
        
        Vector2 direction = DirectionFromAngle(baseAngle + strafeAngle);

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

        SurvivalBullet bullet =
            bulletObj.GetComponent<SurvivalBullet>();

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

    private Vector2 GetTargetDirection()
    {
        if (targetPlayer != null)
        {
            return (targetPlayer.position - transform.position).normalized;
        }

        if (survival != null && survival.IsUsingMouseDebugTarget())
        {
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();

            Camera cam = Camera.main;

            if (cam != null)
            {
                Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mouseScreenPos);

                mouseWorldPos.z = transform.position.z;

                Vector2 direction = (mouseWorldPos - transform.position).normalized;

                if (direction != Vector2.zero) return direction;
            }
        }

        return fallbackDirection;
    }
}