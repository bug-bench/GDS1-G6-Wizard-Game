using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuIntro : MonoBehaviour
{
    public RectTransform titleImage;
    public Image titleGrayscale;
    public RectTransform buttonsGroup;
    public CanvasGroup cloudsGroup;
    public Image blackOverlay;

    private Vector2 titleStartPos;
    private Vector2 buttonsStartPos;
    private Vector2 titleSlam = new Vector2(1300, 230);

    void Start()
    {
        titleStartPos   = titleImage.anchoredPosition;
        buttonsStartPos = buttonsGroup.anchoredPosition;

        // Hide everything
        titleImage.anchoredPosition   = titleStartPos + titleSlam;
        buttonsGroup.anchoredPosition = buttonsStartPos - new Vector2(0, 600);
        cloudsGroup.alpha = 0;

        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        yield return new WaitForSeconds(0.3f);

        // Flash
        blackOverlay.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        blackOverlay.color = Color.black;
        yield return new WaitForSeconds(0.05f);

        // Title slams in
        float t = 0;
        float duration = 0.4f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = 1 - Mathf.Pow(1 - p, 3);
            titleImage.anchoredPosition = Vector2.Lerp(titleStartPos + titleSlam, titleStartPos, eased);
            yield return null;
        }
        titleImage.anchoredPosition = titleStartPos; // snap clean before bounce

        yield return null; // one frame pause

        // overshoot bounce
        titleImage.anchoredPosition = titleStartPos + new Vector2(5, -15);
        yield return new WaitForSeconds(0.08f);
        titleImage.anchoredPosition = titleStartPos;

        // Buttons fly in
        t = 0;
        duration = 0.3f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            float eased = 1 - Mathf.Pow(1 - p, 3);
            buttonsGroup.anchoredPosition = Vector2.Lerp(buttonsStartPos - new Vector2(0, 600), buttonsStartPos, eased);
            yield return null;
        }
        buttonsGroup.anchoredPosition = buttonsStartPos;

        // Clouds fade in
        t = 0;
        duration = 0.5f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cloudsGroup.alpha = t / duration;
            yield return null;
        }
        cloudsGroup.alpha = 1;

        // Fade out black overlay
        t = 0;
        duration = 0.4f;
        Color overlayStart = blackOverlay.color;
        Color titleColor = titleGrayscale.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            blackOverlay.color     = Color.Lerp(overlayStart, Color.clear, p);
            titleColor.a           = Mathf.Lerp(1f, 0f, p);
            titleGrayscale.color   = titleColor;
            yield return null;
        }
        blackOverlay.gameObject.SetActive(false);
        titleGrayscale.gameObject.SetActive(false);
    }
}