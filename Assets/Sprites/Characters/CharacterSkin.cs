using UnityEngine;

[CreateAssetMenu(fileName = "NewSkin", menuName = "Game/Character Skin")]
public class CharacterSkin : ScriptableObject
{
    public string skinName;
    public Sprite previewSprite;           // for character select UI

    [Header("Animator Controllers — one per layer")]
    public RuntimeAnimatorController bodyController;
    public RuntimeAnimatorController headwearController;
    public RuntimeAnimatorController bodywearController;
    public RuntimeAnimatorController facewearController;

    [Header("Color tinting — affects clothing only")]
    public bool  usesColorTint = true;    // body ignores tint, clothing uses it
}