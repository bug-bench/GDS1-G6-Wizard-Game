using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmptyCardUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text promptText;
    public Image buttonIcon;

    void Start()
    {

    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}