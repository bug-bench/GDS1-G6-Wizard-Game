using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class SelectionAnimator : MonoBehaviour
{
    [Header("Highlight")]
    public Image highlightImage;

    [Header("Timing")]
    public float animDuration = 3.5f;
    public float startInterval = 0.08f;
    public float endInterval = 0.45f;

    [Header("Scene load")]
    public float holdOnWinner = 1.2f;

    [Header("Optional")]
    public TMP_Text resultText;

    public void PlaySelection(List<OptionUI> votedOptions, MinigameData winner)
    {
        Debug.Log($"PlaySelection called — highlightImage null: {highlightImage == null}, options: {votedOptions.Count}");
        StartCoroutine(AnimateSelection(votedOptions, winner));
    }

    IEnumerator AnimateSelection(List<OptionUI> votedOptions, MinigameData winner)
    {
        if (highlightImage == null)
        {
            Debug.LogWarning("SelectionAnimator: no highlight image assigned — skipping animation.");
            yield return new WaitForSeconds(holdOnWinner);
            LoadWinner(winner);
            yield break;
        }

        highlightImage.gameObject.SetActive(true);

        // Size the highlight to match the cards
        RectTransform firstCard = votedOptions[0].GetComponent<RectTransform>();
        if (firstCard != null)
        {
            highlightImage.rectTransform.sizeDelta = firstCard.sizeDelta;
        }

        float elapsed = 0f;
        int current = 0;

        int winnerInVoted = 0;
        for (int i = 0; i < votedOptions.Count; i++)
        {
            if (votedOptions[i].minigameName == winner.minigameName)
            {
                winnerInVoted = i;
                break;
            }
        }

        while (elapsed < animDuration)
        {
            MoveHighlightTo(votedOptions[current]);

            float t = elapsed / animDuration;
            float interval = Mathf.Lerp(startInterval, endInterval, t * t);

            yield return new WaitForSeconds(interval);
            elapsed += interval;

            if (elapsed >= animDuration * 0.85f)
            {
                if (current != winnerInVoted)
                    current += (winnerInVoted > current) ? 1 : -1;
            }
            else
            {
                current = (current + 1) % votedOptions.Count;
            }
        }

        MoveHighlightTo(votedOptions[winnerInVoted]);

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = winner.minigameName + " wins!";
        }

        yield return new WaitForSeconds(holdOnWinner);
        LoadWinner(winner);
    }

    void MoveHighlightTo(OptionUI option)
    {
        RectTransform cardRect = option.GetComponent<RectTransform>();
        RectTransform hlRect = highlightImage.rectTransform;

        hlRect.SetParent(cardRect, false);
        hlRect.anchorMin        = Vector2.zero;
        hlRect.anchorMax        = Vector2.one;
        hlRect.offsetMin        = Vector2.zero;
        hlRect.offsetMax        = Vector2.zero;
        hlRect.localScale       = Vector3.one;
        hlRect.localPosition    = Vector3.zero;
        hlRect.sizeDelta        = Vector2.zero;
    }

    void LoadWinner(MinigameData winner)
    {
        SceneManager.LoadScene(winner.sceneName);
    }
}