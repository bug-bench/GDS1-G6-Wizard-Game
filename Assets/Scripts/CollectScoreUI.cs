using TMPro;
using UnityEngine;

public class CollectScoreUI : MonoBehaviour
{
    public TMP_Text scoreText;

    private GameObject targetPlayer;
    private CollectManager collectManager;

    public void Setup(GameObject player, CollectManager cm)
    {
        targetPlayer = player;
        collectManager = cm;
    }

    void Update()
    {
        if (targetPlayer == null || collectManager == null)
            return;

        transform.position = targetPlayer.transform.position + Vector3.up * 2f;

        int score = collectManager.GetPlayerScore(targetPlayer);

        scoreText.text = score.ToString();

        transform.rotation = Camera.main.transform.rotation;
    }
}