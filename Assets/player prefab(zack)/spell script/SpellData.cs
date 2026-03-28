using UnityEngine;

public enum SpellType { Attack, Movement }

[CreateAssetMenu(fileName = "NewSpellData", menuName = "Game/Spell Data", order = 0)]
public class SpellData : ScriptableObject
{
    public string spellName;
    public SpellType spellType;
    public float cooldownTime = 1.5f;
    public GameObject spellPrefab;

    [Tooltip("勾选：按住副键期间不算冷却，松开副键后才开始 cooldown（疾跑、弧形盾等）。不勾选：按下瞬间进冷却（闪现等）。")]
    public bool cooldownStartsOnRelease;

    [Header("FPS Drop System")]
    public GameObject pickupPrefab;
}
