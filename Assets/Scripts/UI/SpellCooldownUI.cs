using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellCooldownUI : MonoBehaviour
{
    [Header("Attack Slot Overlay")]
    public Image attackOverlay;       
    public TMP_Text attackCDText;     

    [Header("Movement Slot Overlay")]
    public Image movementOverlay;
    public TMP_Text movementCDText;

    private PlayerCombat combat;

    public void Init(PlayerCombat playerCombat)
    {
        combat = playerCombat;
        SetOverlayActive(attackOverlay,   attackCDText,   false);
        SetOverlayActive(movementOverlay, movementCDText, false);
    }

    void Update()
    {
        if (combat == null) return;
        UpdateSlot(attackOverlay,   attackCDText,   combat.AttackCDTimer,   combat.AttackCDTotal);
        UpdateSlot(movementOverlay, movementCDText, combat.MovementCDTimer, combat.MovementCDTotal);
    }

    void UpdateSlot(Image overlay, TMP_Text text, float timer, float total)
    {
        if (overlay == null) return;
        bool onCD = timer > 0f;
        SetOverlayActive(overlay, text, onCD);

        if (onCD)
        {
            overlay.fillAmount = timer / Mathf.Max(total, 0.01f);
            if (text != null) text.text = Mathf.CeilToInt(timer).ToString();
        }
    }

    void SetOverlayActive(Image overlay, TMP_Text text, bool active)
    {
        if (overlay != null) overlay.gameObject.SetActive(active);
        if (text    != null) text.gameObject.SetActive(active);
    }
}