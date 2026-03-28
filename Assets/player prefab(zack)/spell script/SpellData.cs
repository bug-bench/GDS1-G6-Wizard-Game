using UnityEngine;

public enum SpellType { Attack, Movement }

[CreateAssetMenu(fileName = "NewSpellData", menuName = "Game/Spell Data", order = 0)]
public class SpellData : ScriptableObject
{
    public string spellName;
    public SpellType spellType;
    public float cooldownTime = 1.5f;
    public GameObject spellPrefab;

    [Header("FPS Drop System")]
    public GameObject pickupPrefab;
}
