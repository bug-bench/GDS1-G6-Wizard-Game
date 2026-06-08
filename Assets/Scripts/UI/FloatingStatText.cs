using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class FloatingStatText : MonoBehaviour
{
    public TMP_Text label;

    [Header("Animation")]
    public float floatSpeed   = 500f;
    public float fadeDuration = 1.2f;

    public void Play(string statName, float amount, Action onComplete = null)
    {
        if (label != null)
            label.text = $"+{amount:F0} {statName}";
        StartCoroutine(Animate(onComplete));
    }

    IEnumerator Animate(Action onComplete)
    {
        float elapsed    = 0f;
        Color startColor = label.color;
        RectTransform rt = GetComponent<RectTransform>();

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / fadeDuration;

            if (rt != null)
                rt.anchoredPosition += new Vector2(0f, floatSpeed * Time.deltaTime);

            label.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

            if (t >= 1f) break;
            yield return null;
        }

        onComplete?.Invoke();
        Destroy(gameObject);
    }
}