using UnityEngine;
using UnityEngine.UI;
using System;

public class SpellSwapIcon : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite promptA;
    [SerializeField] private Sprite promptB;
    [SerializeField] private float swapInterval = 1f;

    private float timer;
    private bool showingA = true;

    public Action<bool> onSwap; // true = showing slot 1 (promptA), false = showing slot 2 (promptB)

    private void Start()
    {
        if (targetImage != null)
            targetImage.sprite = promptA;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= swapInterval)
        {
            timer = 0f;
            showingA = !showingA;

            if (targetImage != null)
                targetImage.sprite = showingA ? promptA : promptB;

            onSwap?.Invoke(showingA);
        }
    }

    public bool IsShowingA() => showingA;
}