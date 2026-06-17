using UnityEngine;
using UnityEngine.UI;

public class SpellSwapIcon : MonoBehaviour
{
    [SerializeField] private Image targetImage;

    [SerializeField] private Sprite iconA;
    [SerializeField] private Sprite iconB;

    [SerializeField] private float swapInterval = 1f;

    private float timer;
    private bool showingA = true;

    private void Start()
    {
        if (targetImage != null)
            targetImage.sprite = iconA;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= swapInterval)
        {
            timer = 0f;

            showingA = !showingA;

            if (targetImage != null)
            {
                targetImage.sprite = showingA ? iconA : iconB;
            }
        }
    }
}