using UnityEngine;
using UnityEngine.UI;

public class SlotSlotUI : MonoBehaviour
{
    public Image slot1Border;
    public Image slot2Border;

    public Sprite normalBorder;
    public Sprite highlightedBorder;

    public void ShowSlot1()
    {
        slot1Border.sprite = highlightedBorder;
        slot2Border.sprite = normalBorder;
    }

    public void ShowSlot2()
    {
        slot1Border.sprite = normalBorder;
        slot2Border.sprite = highlightedBorder;
    }

    public void Clear()
    {
        slot1Border.sprite = normalBorder;
        slot2Border.sprite = normalBorder;
    }
}