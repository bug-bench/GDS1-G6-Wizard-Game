using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TimerUI : MonoBehaviour
{
    [Header("Timer Text")]
    public TMP_Text timerText;

    [Header("Progress Border")]
    public Image progressBorder;

    [Header("Colors")]
    public Color normalColor        = Color.white;
    public Color warningColor       = new Color(1f, 0.25f, 0.25f);
    public Color normalBorderColor  = Color.white;
    public Color warningBorderColor = new Color(1f, 0.25f, 0.25f);
    public float warningTime        = 30f;
    public float fadeDuration       = 0.5f;

    private float totalTime;
    private bool  initialized  = false;
    private bool  warningFired = false;

    public void Init(float duration)
    {
        totalTime    = duration;
        initialized  = true;
        warningFired = false;
        gameObject.SetActive(true);

        if (progressBorder != null)
        {
            progressBorder.fillAmount = 1f;
            progressBorder.color      = normalBorderColor;
        }

        if (timerText != null)
        {
            timerText.color = normalColor;
            timerText.text  = Mathf.CeilToInt(duration).ToString();
        }
    }

    public void UpdateTimer(float remaining)
    {
        if (!initialized) return;

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(remaining).ToString();

        if (progressBorder != null)
            progressBorder.fillAmount = Mathf.Clamp01(remaining / totalTime);

        // Trigger crossfade once when hitting warning time
        if (!warningFired && remaining <= warningTime)
        {
            warningFired = true;
            StartCoroutine(FadeToWarning());
        }
    }

    IEnumerator FadeToWarning()
    {
        float elapsed      = 0f;
        Color startText    = timerText   != null ? timerText.color        : normalColor;
        Color startBorder  = progressBorder != null ? progressBorder.color : normalBorderColor;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / fadeDuration;

            if (timerText    != null) timerText.color        = Color.Lerp(startText,   warningColor,       t);
            if (progressBorder != null) progressBorder.color = Color.Lerp(startBorder, warningBorderColor, t);

            yield return null;
        }

        if (timerText      != null) timerText.color        = warningColor;
        if (progressBorder != null) progressBorder.color   = warningBorderColor;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}