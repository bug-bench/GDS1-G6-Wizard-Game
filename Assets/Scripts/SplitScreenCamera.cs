using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class SplitScreenCamera : MonoBehaviour
{
    private Camera cam;
    private int playerIndex;
    private int totalPlayers;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Read from GameData instead of PlayerInput.all
        // since players joined in the lobby, not this scene
        playerIndex = GetComponentInParent<PlayerInput>().playerIndex;
        totalPlayers = GameData.players.Count;
        cam.depth = playerIndex;

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
}