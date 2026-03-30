using System.Collections.Generic;

public class PlayerData
{
    public int playerIndex;
    public int colorIndex;
    public List<string> spells = new List<string>();
}

public static class GameData
{
    public static List<PlayerData> players = new List<PlayerData>();
    public static bool useSplitScreen = true;

    public static void AddSpell(int playerIndex, string spellName)
    {
        var data = players.Find(p => p.playerIndex == playerIndex);
        if (data != null)
            data.spells.Add(spellName);
    }
}