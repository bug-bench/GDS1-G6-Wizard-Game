using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Phase2StatCard : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI playerLabel;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI speedText;

    private PlayerStats stats;

    private Color[] colors = {
        HexColor("C2453A"),
        HexColor("3A6FBF"),
        HexColor("3DA65A"),
        HexColor("D4A83A"),
    };

    public void Init(PlayerStats playerStats, PlayerData data)
    {
        stats = playerStats;

        // Color the panel itself
        if (data != null)
        {
            GetComponent<Image>().color = colors[data.colorIndex];
            playerLabel.text = $"P{data.playerIndex + 1}";
        }

        UpdateDisplay();
    }

    private void Update()
    {
        if (stats == null) return;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        healthText.text = $"HP: {Mathf.RoundToInt(stats.CurrentHealth())}";
        strengthText.text = $"STR: {Mathf.RoundToInt(stats.CurrentStrength())}";
        speedText.text = $"SPD: {Mathf.RoundToInt(stats.CurrentSpeed())}";
    }

    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }
}