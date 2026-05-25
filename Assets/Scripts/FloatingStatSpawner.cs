using UnityEngine;
using System.Collections.Generic;

public class FloatingStatSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject floatingTextPrefab;
    public Vector2 spawnOffset = new Vector2(0f, 5f);

    private Canvas floatingCanvas;
    private List<FloatingStatText> activeTexts = new List<FloatingStatText>();

    void Awake()
    {
        foreach (var c in GetComponentsInChildren<Canvas>())
        {
            if (c.gameObject.name == "FloatingTextCanvas")
            {
                floatingCanvas = c;
                break;
            }
        }
    }

    public void ShowFloatingText(string statName, float amount)
    {
        if (floatingTextPrefab == null || floatingCanvas == null) return;

        // Clean up destroyed texts from list
        activeTexts.RemoveAll(t => t == null);

        GameObject obj = Instantiate(floatingTextPrefab, floatingCanvas.transform);

        // Stack vertically based on how many are active
        float stackOffset = activeTexts.Count * 80f; // 80 units per text above
        RectTransform rt  = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(spawnOffset.x, spawnOffset.y + stackOffset);
        }

        FloatingStatText ft = obj.GetComponent<FloatingStatText>();
        if (ft != null)
        {
            activeTexts.Add(ft);
            ft.Play(statName, amount, () => activeTexts.Remove(ft));
        }
    }
}