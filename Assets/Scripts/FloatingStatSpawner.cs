using UnityEngine;

public class FloatingStatSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject floatingTextPrefab;

    [Header("Spawn position within player HUD (local to FloatingTextCanvas)")]
    public Vector2 spawnLocalPos = new Vector2(0f, 0f);

    private Canvas floatingCanvas;

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

        GameObject obj = Instantiate(floatingTextPrefab, floatingCanvas.transform);
        RectTransform rt = obj.GetComponent<RectTransform>();

        if (rt != null)
        {
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        FloatingStatText ft = obj.GetComponent<FloatingStatText>();
        if (ft != null) ft.Play(statName, amount);
    }
}