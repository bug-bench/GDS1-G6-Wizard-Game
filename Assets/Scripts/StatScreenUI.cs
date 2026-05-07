using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StatScreenUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject statScreenPanel;

    [Header("Stat Rows")]
    public StatRow healthRow;
    public StatRow speedRow;
    public StatRow strengthRow;
    public StatRow defenseRow;
    public StatRow sizeRow;
    public StatRow focusRow;

    [Header("Timing")]
    public float animDuration = 1.2f;

    // Treat these as the max values for bar scaling
    private const float MAX_HEALTH   = 200f;
    private const float MAX_SPEED    = 15f;
    private const float MAX_STRENGTH = 30f;
    private const float MAX_DEFENSE  = 100f;
    private const float MAX_SIZE     = 3f;
    private const float MAX_FOCUS    = 0.9f;

    void Start()
    {
        healthRow?.Setup("Health",      new Color(0.87f, 0.19f, 0.19f)); // red
        speedRow?.Setup("Speed",        new Color(0.22f, 0.53f, 0.93f)); // blue
        strengthRow?.Setup("Strength",  new Color(0.93f, 0.35f, 0.22f)); // orange
        defenseRow?.Setup("Defense",    new Color(0.22f, 0.85f, 0.45f)); // green
        sizeRow?.Setup("Size",          new Color(0.75f, 0.22f, 0.93f)); // purple
        focusRow?.Setup("Focus",        new Color(0.93f, 0.78f, 0.22f)); // yellow
    }

    public void Show(PlayerStats stats, System.Action onComplete)
    {
        statScreenPanel.SetActive(true);
        StartCoroutine(AnimateStats(stats, onComplete));
    }

    public void Hide()
    {
        statScreenPanel.SetActive(false);
    }

    IEnumerator AnimateStats(PlayerStats stats, System.Action onComplete)
    {
        var rows = new List<(StatRow row, float value, float max)>
        {
            (healthRow,   stats.health,           MAX_HEALTH),
            (speedRow,    stats.speed,             MAX_SPEED),
            (strengthRow, stats.strength,          MAX_STRENGTH),
            (defenseRow,  stats.defense,           MAX_DEFENSE),
            (sizeRow,     stats.sizeMultiplier,    MAX_SIZE),
            (focusRow,    stats.cooldownReduction, MAX_FOCUS),
        };

        // Set values and zero bars
        foreach (var (row, value, max) in rows)
        {
            if (row == null) continue;
            row.SetValue(value);
            row.SetBarFill(0f);
        }

        // Animate all bars simultaneously
        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);

            foreach (var (row, value, max) in rows)
            {
                if (row == null) continue;
                row.SetBarFill(Mathf.Clamp01(value / max) * t);
            }

            yield return null;
        }

        // Snap to final values
        foreach (var (row, value, max) in rows)
        {
            if (row == null) continue;
            row.SetBarFill(Mathf.Clamp01(value / max));
        }

        yield return new WaitForSeconds(2f);
        onComplete?.Invoke();
    }
}