using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingStatText : MonoBehaviour
{
    public TMP_Text label;

    [Header("Animation")]
    public float floatSpeed   = 2000f;  // UI units per second
    public float fadeDuration = 1.3f;

    public void Play(string statName, float amount)
    {
        if (label != null)
            label.text = $"+{amount:F0} {statName}";
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float elapsed = 0f;
        Color startColor = label.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            Vector3 before = transform.localPosition;
            transform.localPosition += new Vector3(0f, floatSpeed * Time.deltaTime, 0f);
            Vector3 after = transform.localPosition;

            Debug.Log($"Before: {before.y}, After: {after.y}, floatSpeed: {floatSpeed}, dt: {Time.deltaTime}");

            label.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            yield return null;
        }

        Destroy(gameObject);
    }
}