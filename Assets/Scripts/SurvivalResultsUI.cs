using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SurvivalPlayerResult : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text rankText;
    public TMP_Text playerLabel;
    public TMP_Text timeText;
    public Image    playerColor;
    public GameObject crownIcon;

    public void Setup(int rank, int playerIndex, int colorIndex, string timeStr, bool isWinner)
    {
        if (rankText    != null) rankText.text    = rank == 1 ? "1st" :
                                                    rank == 2 ? "2nd" :
                                                    rank == 3 ? "3rd" : "4th";
        if (playerLabel != null) playerLabel.text = $"P{playerIndex + 1}";
        if (timeText    != null) timeText.text    = timeStr;
        if (playerColor != null) playerColor.color = PlayerData.PlayerColors.GetColor(colorIndex);
        if (crownIcon   != null) crownIcon.SetActive(isWinner);
    }
}

public class SurvivalResultsUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject resultsPanel;
    public Transform podiumContainer;
    public GameObject resultSlotPrefab;

    public void Show(List<KeyValuePair<GameObject, float>> rankings)
    {
        resultsPanel.SetActive(true);

        foreach (Transform child in podiumContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < rankings.Count; i++)
        {
            GameObject player = rankings[i].Key;
            float time        = rankings[i].Value;

            var data = GameData.players.Find(p => p.playerGameObject == player);
            if (data == null) continue;

            int mins       = Mathf.FloorToInt(time / 60f);
            int secs       = Mathf.FloorToInt(time % 60f);
            string timeStr = $"{mins:00}:{secs:00}";

            GameObject slot         = Instantiate(resultSlotPrefab, podiumContainer);
            PlayerResultUI ui = slot.GetComponent<PlayerResultUI>();
            if (ui == null) continue;
            ui.Setup(i + 1, data.playerIndex, data.colorIndex, timeStr, 300f - (i * 60f));
        }
    }
}