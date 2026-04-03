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

    [Header("Spell Slots")]
    public Image firstSpellSlot;       // assign in Inspector
    public Image secondSpellSlot;        // assign in Inspector

    // Shown when slot is empty
    public Sprite emptySlotSprite;    // assign a grey placeholder in Inspector

    private PlayerStats stats;
    private PlayerCombat combat;

    private Color[] colors = {
        UseHexColor.HexColor("C2453A"),
        UseHexColor.HexColor("3A6FBF"),
        UseHexColor.HexColor("3DA65A"),
        UseHexColor.HexColor("D4A83A"),
    };

    public void Init(PlayerStats playerStats, PlayerData data)
    {
        stats = playerStats;
        combat = playerStats.GetComponent<PlayerCombat>();

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
        // Stats
        healthText.text = $"{Mathf.RoundToInt(stats.CurrentHealth())}";
        strengthText.text = $"{Mathf.RoundToInt(stats.CurrentStrength())}";
        speedText.text = $"{Mathf.RoundToInt(stats.CurrentSpeed())}";

        // Spell icons — update every frame so equip/drop reflects instantly
        if (combat != null)
        {
            SetSlotIcon(firstSpellSlot, combat.currentAttackSpell);
            SetSlotIcon(secondSpellSlot, combat.currentMovementSpell);
        }
    }

    private void SetSlotIcon(Image slot, SpellData spell)
    {
        if (slot == null) return;

        Sprite icon = spell?.GetIcon();

        if (icon != null)
        {
            slot.sprite = icon;
            slot.color = spell.GetIconColor(); // use the prefab's sprite color
        }
        else
        {
            slot.sprite = emptySlotSprite;
            slot.color = new Color(1, 1, 1, 0.3f);
        }
    }
}