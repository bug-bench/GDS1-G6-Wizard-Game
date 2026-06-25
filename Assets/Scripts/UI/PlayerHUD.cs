using UnityEngine;

public class SpellPickupHUDReceiver : MonoBehaviour
{
    [Header("Slot Highlight overlays — sit on top of each spell slot Image")]
    public GameObject slot1Highlight; // overlay on first spell slot
    public GameObject slot2Highlight; // overlay on second spell slot

    public void ShowSlotHighlight(bool isSlot1)
    {
        if (slot1Highlight != null) slot1Highlight.SetActive(isSlot1);
        if (slot2Highlight != null) slot2Highlight.SetActive(!isSlot1);
    }

    public void HidePrompt()
    {
        if (slot1Highlight != null) slot1Highlight.SetActive(false);
        if (slot2Highlight != null) slot2Highlight.SetActive(false);
    }
}