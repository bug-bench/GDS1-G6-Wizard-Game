using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class CountdownUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text countdownText;
    public CanvasGroup canvasGroup;

    [Header("Animation")]
    public float scaleUpDuration  = 0.3f;  // time to grow big
    public float slamDuration     = 0.15f; // time to slam down to normal
    public float holdDuration     = 0.3f;  // time to hold before fading
    public float fadeDuration     = 0.25f; // time to fade out

    [Header("Scale")]
    public float startScale       = 0.3f;
    public float peakScale        = 1.6f;
    public float normalScale      = 1.0f;

    [Header("Colors")]
    public Color color3           = new Color(1f, 1f, 1f, 1f);
    public Color color2           = new Color(1f, 0.8f, 0.2f, 1f); // yellow
    public Color color1           = new Color(1f, 0.3f, 0.3f, 1f); // red
    public Color colorGo          = new Color(0.3f, 1f, 0.4f, 1f); // green

    public void Play(Action onComplete)
    {
        gameObject.SetActive(true);
        StartCoroutine(RunCountdown(onComplete));
    }

    IEnumerator RunCountdown(Action onComplete)
    {
        string[] steps = { "3", "2", "1", "GO!" };
        Color[]  colors = { color3, color2, color1, colorGo };

        foreach (var (text, color) in ZipSteps(steps, colors))
        {
            countdownText.text  = text;
            countdownText.color = color;
            yield return StartCoroutine(AnimateNumber());
        }

        gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    IEnumerator AnimateNumber()
    {
        // Scale up from small to peak
        float elapsed = 0f;
        canvasGroup.alpha = 1f;

        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / scaleUpDuration;
            float s  = Mathf.Lerp(startScale, peakScale, t);
            countdownText.transform.localScale = Vector3.one * s;
            yield return null;
        }

        // Slam down to normal scale
        elapsed = 0f;
        while (elapsed < slamDuration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / slamDuration;
            float s  = Mathf.Lerp(peakScale, normalScale, t);
            countdownText.transform.localScale = Vector3.one * s;
            yield return null;
        }

        countdownText.transform.localScale = Vector3.one * normalScale;

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed       += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    // Simple zip helper
    System.Collections.Generic.IEnumerable<(string, Color)> ZipSteps(string[] texts, Color[] colors)
    {
        for (int i = 0; i < texts.Length; i++)
            yield return (texts[i], colors[i]);
    }
}