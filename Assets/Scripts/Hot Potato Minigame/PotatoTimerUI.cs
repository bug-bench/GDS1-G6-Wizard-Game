using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotatoTimerUI : MonoBehaviour
{
    public TMP_Text timerText;
    [Header("Potato Sprite")]
    [SerializeField] private Image potato;

    private GameObject targetPlayer;
    private PotatoPickup potatoPickup;

    public void Setup(GameObject player, PotatoPickup pp)
    {
        targetPlayer = player;
        potatoPickup = pp;
        if (potato != null)
        {
            potato.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (targetPlayer == null || potatoPickup == null)
            return;
        
        bool isHolder = potatoPickup.GetCurrentPlayer() == targetPlayer;
        
        //activates the timer and potato if the holder has the potato
        if (isHolder && (!potato.gameObject.activeInHierarchy || !timerText.gameObject.activeInHierarchy))
        {
            potato.gameObject.SetActive(isHolder);
            timerText.gameObject.SetActive(isHolder);
        }
        //deactivates the timer and potato if the holder does not have the potato
        if (!isHolder && (potato.gameObject.activeInHierarchy || timerText.gameObject.activeInHierarchy))
        {
            potato.gameObject.SetActive(isHolder);
            timerText.gameObject.SetActive(isHolder);
        }

        if (!isHolder) return;

        transform.position = targetPlayer.transform.position + Vector3.up * 2f;

        int time = potatoPickup.GetRemainingTime();

        timerText.text = time.ToString();

        transform.rotation = Camera.main.transform.rotation;
    }

    public void SetHolding(bool holding)
    {
        if (potato != null)
        {
            potato.gameObject.SetActive(holding);
        }
    }
}