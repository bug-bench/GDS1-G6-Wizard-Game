using UnityEngine;
using UnityEngine.SceneManagement;

public class Phase2Script : MonoBehaviour
{
    private Phase1Script p1s;
    private string currentMinigame;

    void Start()
    {
        p1s = GetComponent<Phase1Script>();

        if (p1s == null)
        {
            Debug.LogError("EventManager not found!");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (p1s.GetCurrentPhase() == 2)
        {
            MinigameChosen("Arena");
        }
    }

    public string GetCurrentMinigame()
    {
        return currentMinigame;
    }

    public void MinigameChosen(string minigameName)
    {
        if (p1s == null || p1s.GetCurrentPhase() != 2)
        {
            Debug.LogWarning("Tried to start minigame outside of Phase 2");
            return;
        }

        Debug.Log("Minigame selected: " + minigameName);

        switch (minigameName)
        {
            case "Arena":
                SceneManager.LoadScene(1);
                StartArenaMode();
                break;
            default:
                Debug.LogWarning("Unknown minigame: " + minigameName);
                break;
        }
        currentMinigame = minigameName;
    }

    void StartArenaMode()
    {
        Debug.Log("Starting Arena Battle Royale mode...");
        gameObject.AddComponent<ArenaScript>();
    }
}
