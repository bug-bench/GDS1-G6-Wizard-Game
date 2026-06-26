using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerData
{
    public int playerIndex;
    public int colorIndex;
    public List<string> spells = new List<string>();
    public InputDevice device;
    public Sprite playerSprite; // saves sprite at spawn 
    public GameObject playerGameObject;
    public CharacterDefinition character;
    public CharacterSkin skin;


    // Combat stats
    public int kills;
    public float damageDealt;

    public static class PlayerColors
    {
        public static Color GetColor(int i)
        {
            Color[] colors =
            {
                new Color32(255, 80, 80, 255),
                new Color32(80, 140, 255, 255),
                new Color32(80, 220, 120, 255),
                new Color32(255, 220, 80, 255)
            };

            return colors[i % colors.Length];
        }
    }
}

public static class GameData
{
    public static List<PlayerData> players = new List<PlayerData>();

    public static bool useSplitScreen = true;
    public static int winnerIndex = -1; // set before loading win scene
    public static string selectedMinigame = "";

    public static void AddSpell(int playerIndex, string spellName)
    {
        var data = players.Find(p => p.playerIndex == playerIndex);
        if (data != null)
            data.spells.Add(spellName);
    }

    public static void RecordKill(int attackerIndex)
    {
        var data = players.Find(p => p.playerIndex == attackerIndex);
        if (data != null)
            data.kills++;
    }

    public static void RecordDamage(int attackerIndex, float amount)
    {
        var data = players.Find(p => p.playerIndex == attackerIndex);
        if (data != null)
            data.damageDealt += amount;
    }
}