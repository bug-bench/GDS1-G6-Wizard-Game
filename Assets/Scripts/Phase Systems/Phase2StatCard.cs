using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Phase2StatCard : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI playerLabel;
    public TextMeshProUGUI healthText;

    [Header("Player Portrait")]
    public Image playerPortrait;

    [Header("Health Bar")]
    public RectTransform healthBarOuter;
    public Image healthBarInner;
    public float baseBarWidth;

    [Header("Spell Slots")]
    public Image firstSpellSlot;
    public Image secondSpellSlot;
    public Sprite emptySlotSprite;

    private PlayerStats stats;
    private PlayerCombat combat;

    private Color[] colors = {
        UseHexColor.HexColor("C2453A"),
        UseHexColor.HexColor("3A6FBF"),
        UseHexColor.HexColor("3DA65A"),
        UseHexColor.HexColor("D4A83A"),
    };

    void Awake()
    {
        // Capture the bar's designed width as the baseline for 100 HP
        if (healthBarOuter != null)
            baseBarWidth = healthBarOuter.sizeDelta.x;
    }

    public void Init(PlayerStats playerStats, PlayerData data)
    {
        stats  = playerStats;
        combat = playerStats.GetComponent<PlayerCombat>();

        if (data != null)
        {
            GetComponent<Image>().color = colors[data.colorIndex];
            playerLabel.text = $"P{data.playerIndex + 1}";

            if (playerPortrait != null && data.playerSprite != null)
            {
                playerPortrait.sprite = data.playerSprite;
                playerPortrait.color  = colors[data.colorIndex];
            }
        }

        UpdateDisplay();
        GetComponent<SpellCooldownUI>()?.Init(combat);
    }

    private void Update()
    {
        if (stats == null) return;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        float current = stats.CurrentHealth();
        float max     = stats.MaxHealth;

        healthText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";

        if (healthBarOuter != null)
        {
            float scale = max / 100f;
            healthBarOuter.sizeDelta = new Vector2(baseBarWidth * scale, healthBarOuter.sizeDelta.y);
        }

        if (healthBarInner != null)
        {
            healthBarInner.rectTransform.anchorMin = Vector2.zero;
            healthBarInner.rectTransform.anchorMax = Vector2.one;
            healthBarInner.rectTransform.offsetMin = Vector2.zero;
            healthBarInner.rectTransform.offsetMax = Vector2.zero;
            healthBarInner.fillAmount = Mathf.Clamp01(current / max);
        }

        if (combat != null)
        {
            SetSlotIcon(firstSpellSlot,  combat.currentAttackSpell);
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
            slot.color  = spell.GetIconColor();
        }
        else
        {
            slot.sprite = emptySlotSprite;
            slot.color  = new Color(1, 1, 1, 0.3f);
        }
    }
}