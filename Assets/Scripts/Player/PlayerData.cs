using System.Collections.Generic;

public class PlayerData
{
    public int playerIndex;
    public int colorIndex;
    /// <summary>
    /// 大厅加入时设备的 InputDevice.deviceId，用于 Phase1 生成玩家时配对同一手柄/键盘。
    /// InputDevice.deviceId captured at lobby join; PlayerSpawner pairs the same gamepad/keyboard in Phase1.
    /// </summary>
    public int deviceId = -1;
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