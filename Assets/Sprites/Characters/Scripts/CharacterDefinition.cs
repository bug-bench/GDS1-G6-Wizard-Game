using UnityEngine;

// CharacterDefinition.cs
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character Definition")]
public class CharacterDefinition : ScriptableObject
{
    public string characterId;
    public string displayName;
    public Sprite portrait;
    public CharacterSkin[] skins; // index 0 = default
}