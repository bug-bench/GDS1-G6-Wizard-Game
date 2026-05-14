using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    private float baseBarWidth;

    [Header("Overflow FX")]
    public float overflowThreshold     = 150f;
    public float superSaiyanThreshold  = 200f;
    public Color normalBarColor        = new Color(0.91f, 0.19f, 0.19f);
    public Color normalOuterColor      = new Color(0.4f, 0.05f, 0.05f);
    public Color superSaiyanColor      = new Color(1f, 0.95f, 0.2f);
    public Color superSaiyanOuterColor = new Color(0.8f, 0.6f, 0f);

    [Header("Spell Slots")]
    public Image firstSpellSlot;
    public Image secondSpellSlot;
    public Sprite emptySlotSprite;

    private PlayerStats stats;
    private PlayerCombat combat;
    private float shakeTimer      = 0f;
    private Vector2 barOriginalPos;
    private bool barPosInitialized = false;

    private Color[] colors = {
        UseHexColor.HexColor("C2453A"),
        UseHexColor.HexColor("3A6FBF"),
        UseHexColor.HexColor("3DA65A"),
        UseHexColor.HexColor("D4A83A"),
    };

    void Awake()
    {
        if (healthBarOuter != null)
            baseBarWidth = healthBarOuter.sizeDelta.x;
    }

    void Start()
    {
        StartCoroutine(InitBarPos());
    }

    IEnumerator InitBarPos()
    {
        yield return null; // wait one frame for layout to settle
        if (healthBarOuter != null)
        {
            barOriginalPos     = healthBarOuter.anchoredPosition;
            barPosInitialized  = true;
        }
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

        if (healthBarOuter != null && barPosInitialized)
        {
            // Only overflow past threshold
            float effectiveScale = max >= overflowThreshold
                ? max / overflowThreshold
                : 1f;

            healthBarOuter.sizeDelta = new Vector2(
                baseBarWidth * effectiveScale,
                healthBarOuter.sizeDelta.y
            );

            Image outerImg = healthBarOuter.GetComponent<Image>();

            if (max >= superSaiyanThreshold)
            {
                Debug.Log($"SUPER SAIYAN — max: {max}");

                float pulse = (Mathf.Sin(Time.time * 8f) + 1f) / 2f;

                if (healthBarInner != null)
                    healthBarInner.color = Color.Lerp(normalBarColor, superSaiyanColor, pulse);

                if (outerImg != null)
                    outerImg.color = Color.Lerp(normalOuterColor, superSaiyanOuterColor, pulse);

                shakeTimer += Time.deltaTime;
                float shakeX = Mathf.Sin(shakeTimer * 40f) * 3f;
                float shakeY = Mathf.Sin(shakeTimer * 37f) * 2f;
                healthBarOuter.anchoredPosition = barOriginalPos + new Vector2(shakeX, shakeY);
            }
            else
            {
                if (healthBarInner != null)
                    healthBarInner.color = normalBarColor;

                if (outerImg != null)
                    outerImg.color = normalOuterColor;

                healthBarOuter.anchoredPosition = barOriginalPos;
                shakeTimer = 0f;
            }
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