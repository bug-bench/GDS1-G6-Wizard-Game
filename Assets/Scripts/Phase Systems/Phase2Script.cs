using UnityEngine;
using UnityEngine.SceneManagement;

public class Phase2Script : MonoBehaviour
{
    private Phase1Script p1s;
    private VotingLogic vL;
    private string currentMinigame;
    private bool phaseChosen = false;

    void Start()
    {
        p1s = GetComponent<Phase1Script>();
        vL = GetComponent<VotingLogic>();

        if (p1s == null)
        {
            Debug.LogError("EventManager not found!");
            return;
        }
        if (vL == null)
        {
            Debug.LogError("VotingLogic missing!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (p1s.GetCurrentPhase() == 2 && !phaseChosen)
        {
            phaseChosen = true;
            StartVotingPhase();
        }
    }

    void StartVotingPhase()
    {
        Debug.Log("Voting phase started...");
        vL.BeginVoting(OnMinigameChosen);
    }

    void OnMinigameChosen(string minigameName)
    {
        Debug.Log("Final Minigame: " + minigameName);
        currentMinigame = minigameName;

        switch (minigameName)
        {
            case "Arena":
                StartArenaMode();
                SceneManager.LoadScene("Phase2Arena");
                break;

            case "Survival":
                StartSurvivalMode();
                SceneManager.LoadScene("Phase2Survival");
                break;

            default:
                Debug.LogWarning("Unknown minigame: " + minigameName);
                break;
        }
    }

    void StartSurvivalMode()
    {
        gameObject.AddComponent<SurvivalScript>();
    }

    void StartArenaMode()
    {
        Debug.Log("Starting Arena Battle Royale mode...");
        gameObject.AddComponent<ArenaScript>();
    }

    public string GetCurrentMinigame()
    {
        return currentMinigame;
    }
}
