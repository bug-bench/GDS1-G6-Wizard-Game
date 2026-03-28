using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerCard : MonoBehaviour
{
    public Image characterImage;
    public GameObject readyText;

    private PlayerInput player;
    private int colorIndex = 0;

    private Color[] colors = {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };

    public bool isReady = false;

    private float inputCooldown = 0.2f;
    private float lastInputTime = 0f;

    public void SetPlayer(PlayerInput input)
    {
        player = input;
        UpdateColor();
        readyText.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        var move = player.actions["Move"].ReadValue<Vector2>();

        if (!isReady && Time.time - lastInputTime > inputCooldown)
        {
            if (move.x > 0.5f)
            {
                colorIndex = (colorIndex + 1) % colors.Length;
                UpdateColor();
                lastInputTime = Time.time;
            }
            else if (move.x < -0.5f)
            {
                colorIndex--;
                if (colorIndex < 0) colorIndex = colors.Length - 1;
                UpdateColor();
                lastInputTime = Time.time;
            }
        }

        if (player.actions["Submit"].triggered)
        {
            isReady = !isReady;
            readyText.SetActive(isReady);
        }
    }

    void UpdateColor()
    {
        characterImage.color = colors[colorIndex];
    }
}