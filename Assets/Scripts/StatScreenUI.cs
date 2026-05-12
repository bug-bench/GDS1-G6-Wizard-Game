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

    [Header("Max pickup counts for bar scaling")]
    public float maxPickups = 20f;

    void Start()
    {
        healthRow?.Setup("Health",      new Color(0.87f, 0.19f, 0.19f));
        speedRow?.Setup("Speed",        new Color(0.22f, 0.53f, 0.93f));
        strengthRow?.Setup("Strength",  new Color(0.93f, 0.35f, 0.22f));
        defenseRow?.Setup("Defense",    new Color(0.22f, 0.85f, 0.45f));
        sizeRow?.Setup("Size",          new Color(0.75f, 0.22f, 0.93f));
        focusRow?.Setup("Focus",        new Color(0.93f, 0.78f, 0.22f));
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
            (speedRow,    stats.GetStatPickupCount("Speed")),
            (strengthRow, stats.GetStatPickupCount("Strength")),
            (defenseRow,  stats.GetStatPickupCount("Defense")),
            (sizeRow,     stats.GetStatPickupCount("Size")),
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