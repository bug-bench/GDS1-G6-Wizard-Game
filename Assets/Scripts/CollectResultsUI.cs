using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerResultUI : MonoBehaviour
{
    public TMP_Text placementText;
    public TMP_Text scoreText;
    public TMP_Text playerText;

    public Image playerImage;
    public RectTransform bar;

    public void Setup(int placement, int playerIndex, int colorIndex, int score, float height)
    {
        placementText.text = placement + GetSuffix(placement);

        scoreText.text = score.ToString();

        playerText.text = "P" + (playerIndex + 1);

        playerImage.color = PlayerData.PlayerColors.GetColor(colorIndex);
        bar.GetComponent<Image>().color = PlayerData.PlayerColors.GetColor(colorIndex);

        Vector2 size = bar.sizeDelta;
        size.y = height;
        bar.sizeDelta = size;
    }

    Color[] colors =
    {
        new Color32(255, 80, 80, 255),
        new Color32(80, 140, 255, 255),
        new Color32(80, 220, 120, 255),
        new Color32(255, 220, 80, 255)
    };

    string GetSuffix(int num)
    {
        switch (num)
        {
            case 1: return "st";
            case 2: return "nd";
            case 3: return "rd";
            default: return "th";
        }
    }
}