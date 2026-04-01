using UnityEngine;

public enum SpellType { Attack, Movement }

[CreateAssetMenu(fileName = "NewSpellData", menuName = "Game/Spell Data", order = 0)]
public class SpellData : ScriptableObject
{
    public string spellName;
    public SpellType spellType;
    public float cooldownTime = 1.5f;
    public GameObject spellPrefab;
    public Sprite icon;

    [Tooltip("勾选：按住副键期间不算冷却，松开副键后才开始 cooldown（疾跑、弧形盾等）。不勾选：按下瞬间进冷却（闪现等）。 — If true: cooldown starts on sub-button release (sprint, shield). If false: cooldown on press (blink, etc.).")]
    public bool cooldownStartsOnRelease;

    [Header("FPS Drop System")]
    public GameObject pickupPrefab;
    public Sprite GetIcon()
    {
        if (icon != null) return icon; // In case we want to assign an icon directly to the spell data

        if (pickupPrefab != null)
        {
            var sr = pickupPrefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null) return sr.sprite;
        }
        // use spellPrefab as fallback if no icon is assigned
        if (spellPrefab != null)
        {
            var sr = spellPrefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null) return sr.sprite;
        }

        return null;
    }

    public Color GetIconColor()
    {
        if (pickupPrefab != null)
        {
            var sr = pickupPrefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) return sr.color;
        }

        if (spellPrefab != null)
        {
            var sr = spellPrefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) return sr.color;
        }

        return Color.white;
    }
}

