using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class SplitScreenCamera : MonoBehaviour
{
    private Camera cam;
    private int playerIndex;
    private int totalPlayers;

    private void Awake()
    {
        PlayerInputManager.instance.onPlayerJoined += HandlePlayerJoined;
    }

    void HandlePlayerJoined(PlayerInput obj)
    {
        totalPlayers = PlayerInput.all.Count;
        SetupCamera();
    }

    void SetupCamera()
    {
        if (totalPlayers == 1)
        {
            cam.rect = new Rect(0, 0, 1, 1);
        }
        else if (totalPlayers == 2)
        {
            cam.rect = new Rect(playerIndex == 0 ? 0 : 0.5f, 0, 0.5f, 1);
        }
        else if (totalPlayers == 3)
        {
            cam.rect = new Rect(
                playerIndex == 0 ? 0 : (playerIndex == 1 ? 0.5f : 0),
                playerIndex < 2 ? 0.5f : 0,
                playerIndex < 2 ? 0.5f : 1,
                0.5f);
        }
        else
        {
            cam.rect = new Rect((playerIndex % 2) * 0.5f, (playerIndex < 2) ? 0.5f : 0, 0.5f, 0.5f);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerIndex = GetComponentInParent<PlayerInput>().playerIndex;
        totalPlayers = PlayerInput.all.Count;
        cam = GetComponent<Camera>();
        cam.depth = playerIndex;

        SetupCamera();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
