using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SurvivalResultsUI : MonoBehaviour
{
    public TextMeshProUGUI winnerText;
    public Image winnerImage;

    public void ShowWinner(PlayerData winner)
    {
        if (winner == null)
        {
            winnerText.text = "No Winner";
            winnerImage.enabled = false;
            return;
        }

        winnerText.text = "P" + (winner.playerIndex + 1) + " Wins!";

        // show sprite if you saved one
        if (winner.playerSprite != null)
        {
            winnerImage.sprite = winner.playerSprite;
            winnerImage.enabled = true;
        }
        else
        {
            winnerImage.enabled = true;
            winnerImage.sprite = null;

            // fallback = color block
            winnerImage.color = PlayerData.PlayerColors.GetColor(winner.colorIndex);
        }
    }
}