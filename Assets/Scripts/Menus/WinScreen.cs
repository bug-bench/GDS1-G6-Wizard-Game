using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    [Header("UI References")]
    public Image playerWonImg;
    public TextMeshProUGUI playerWonTxt;
    public TextMeshProUGUI damageDealtTxt;
    public TextMeshProUGUI eliminationsTxt;

    [SerializeField] private string mainMenuScene = "MainMenu";

    private Color[] colors = {
        HexColor("C2453A"),
        HexColor("3A6FBF"),
        HexColor("3DA65A"),
        HexColor("D4A83A"),
    };

    private void Start()
    {
        Debug.Log($"WinScreen — winnerIndex: {GameData.winnerIndex}, players: {GameData.players.Count}");
        ShowWinner();
    }

    private void ShowWinner()
    {
        int winnerIndex = GameData.winnerIndex;
        var data = GameData.players.Find(p => p.playerIndex == winnerIndex);

        foreach (var p in GameData.players)
            Debug.Log($"Player {p.playerIndex} — kills: {p.kills}, damage: {p.damageDealt}");

        Debug.Log($"winner data: {data != null}, kills: {data?.kills}, damage: {data?.damageDealt}");

        if (data == null)
        {
            playerWonTxt.text = "NO DATA";
            return;
        }

        // Show player sprite with their color
        if (data.playerSprite != null)
        {
            playerWonImg.sprite = data.playerSprite;
            playerWonImg.color = colors[data.colorIndex];
        }
        else
        {
            // fallback — just show color block
            playerWonImg.color = colors[data.colorIndex];
        }

        playerWonTxt.text = $"P{winnerIndex + 1} WINS!";
        damageDealtTxt.text = $"Damage Dealt: {Mathf.RoundToInt(data.damageDealt)}";
        eliminationsTxt.text = $"Eliminations: {data.kills}";
    }

    public void OnPlayAgainPressed()
    {
        GameData.players.Clear();
        GameData.winnerIndex = -1;
        SceneManager.LoadScene(mainMenuScene);
    }

    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }
}