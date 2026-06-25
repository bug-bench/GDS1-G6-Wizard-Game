using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectScoreUI : MonoBehaviour
{
    public TMP_Text scoreText;
    [Header("Leader Crown")]
    [SerializeField] private Image crownImage;

    private GameObject targetPlayer;
    private CollectManager collectManager;

    public void Setup(GameObject player, CollectManager cm)
    {
        targetPlayer = player;
        collectManager = cm;
        if (crownImage != null)
        {
            crownImage.gameObject.SetActive(false);
        }
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

    public void SetCrowned(bool crowned)
    {
        if (crownImage != null)
        {
            crownImage.gameObject.SetActive(crowned);
        }
    }
}