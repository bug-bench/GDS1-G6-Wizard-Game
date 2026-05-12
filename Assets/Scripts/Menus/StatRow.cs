using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatRow : MonoBehaviour
{
    public TMP_Text label;
    public TMP_Text valueText;
    public Image fillBar;

    public void Setup(string labelName, Color barColor)
    {
        if (label != null) label.text = labelName;
        if (fillBar != null) fillBar.color = barColor;
    }

    public void SetValue(float value)
    {
        if (valueText != null)
            valueText.text = Mathf.RoundToInt(value).ToString();
    }

    public void SetBarFill(float normalized)
    {
        if (fillBar != null)
            fillBar.fillAmount = Mathf.Clamp01(normalized);
    }
}