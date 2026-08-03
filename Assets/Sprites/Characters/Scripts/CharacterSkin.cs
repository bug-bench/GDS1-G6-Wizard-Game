using UnityEngine;
[CreateAssetMenu(fileName = "NewSkin", menuName = "Game/Character Skin")]
public class CharacterSkin : ScriptableObject
{
    public string skinName;

    [Header("Animator Controllers — one per layer")]
    public RuntimeAnimatorController bodyController;
    public RuntimeAnimatorController headwearController;
    public RuntimeAnimatorController bodywearController;
    public RuntimeAnimatorController facewearController;

    [Header("Color tinting")]
    public bool usesColorTint = true;

    [Header("Auto-extracted lobby preview — do not edit manually")]
    public Sprite lobbySprite; // auto-filled by editor script below
}