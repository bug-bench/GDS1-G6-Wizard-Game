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
    public StatRow attackRow;
    public StatRow defenseRow;
    public StatRow speedRow;
    public StatRow frictionRow;
    public StatRow focusRow;
    public StatRow manaRow;

    [Header("Timing")]
    public float animDuration = 1.2f;

    [Header("Max pickup counts for bar scaling")]
    public float maxPickups = 20f;

    void Start()
    {
        healthRow?.Setup("Health",   new Color(0.90f, 0.20f, 0.20f)); // Red
        attackRow?.Setup("Attack",   new Color(1.00f, 0.55f, 0.15f)); // Orange
        defenseRow?.Setup("Defense", new Color(1.00f, 0.85f, 0.15f)); // Yellow
        speedRow?.Setup("Speed",     new Color(0.10f, 0.20f, 0.60f)); // Navy Blue
        frictionRow?.Setup("Friction", new Color(0.15f, 0.85f, 1.00f)); // Cyan Blue
        focusRow?.Setup("Focus",     new Color(0.20f, 0.85f, 0.30f)); // Green
        manaRow?.Setup("Mana",       new Color(0.70f, 0.30f, 1.00f)); // Purple
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
        var rows = new List<(StatRow row, float value)>
        {
            (healthRow,   stats.GetStatPickupCount("Health")),
            (attackRow,   stats.GetStatPickupCount("Attack")),
            (defenseRow,  stats.GetStatPickupCount("Defense")),
            (speedRow,    stats.GetStatPickupCount("Speed")),
            (frictionRow, stats.GetStatPickupCount("Friction")),
            (manaRow,     stats.GetStatPickupCount("Mana")),
            (focusRow,    stats.GetStatPickupCount("Focus")),
        };

        foreach (var (row, value) in rows)
        {
            if (row == null) continue;
            row.SetValue(value);
            row.SetBarFill(0f);
        }

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);

            foreach (var (row, value) in rows)
            {
                if (row == null) continue;
                row.SetBarFill(Mathf.Clamp01(value / maxPickups) * t);
            }

            yield return null;
        }

        foreach (var (row, value) in rows)
        {
            if (row == null) continue;
            row.SetBarFill(Mathf.Clamp01(value / maxPickups));
        }

        yield return new WaitForSeconds(2f);
        onComplete?.Invoke();
    }
}