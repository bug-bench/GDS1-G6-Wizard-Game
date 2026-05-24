using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [Header("Timer Text")]
    public TMP_Text timerText;

    [Header("Progress Border")]
    public Image progressBorder;   // Image type Filled, Fill Method Radial360

    [Header("Colors")]
    public Color normalColor  = Color.white;
    public Color warningColor = new Color(1f, 0.25f, 0.25f); // red
    public float warningTime  = 30f;

    private float totalTime;
    private bool  initialized = false;

    public void Init(float duration)
    {
        totalTime   = duration;
        initialized = true;
        gameObject.SetActive(true);

        if (progressBorder != null)
            progressBorder.fillAmount = 1f;

        if (timerText != null)
        {
            timerText.color = normalColor;
            timerText.text  = Mathf.CeilToInt(duration).ToString();
        }
    }

    public void UpdateTimer(float remaining)
    {
        if (!initialized) return;

        // Text
        int seconds = Mathf.CeilToInt(remaining);
        if (timerText != null)
        {
            timerText.text  = seconds.ToString();
            timerText.color = remaining <= warningTime ? warningColor : normalColor;
        }

        // Progress border wraps around — fillAmount 1 = full, 0 = empty
        if (progressBorder != null)
            progressBorder.fillAmount = Mathf.Clamp01(remaining / totalTime);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}