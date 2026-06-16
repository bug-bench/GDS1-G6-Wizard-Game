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
        healthRow?.Setup("Health",   new Color(0.851f, 0.341f, 0.388f)); // #D95763
        attackRow?.Setup("Attack",   new Color(1.000f, 0.404f, 0.000f)); // #FF6700
        defenseRow?.Setup("Defense", new Color(0.890f, 0.847f, 0.000f)); // #E3D800
        speedRow?.Setup("Speed",     new Color(0.200f, 0.373f, 0.533f)); // #335F88
        frictionRow?.Setup("Friction", new Color(0.000f, 0.596f, 0.569f)); // #009891
        focusRow?.Setup("Focus",     new Color(0.204f, 0.831f, 0.369f)); // #34D45E
        manaRow?.Setup("Mana",       new Color(0.463f, 0.259f, 0.541f)); // #76428A
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