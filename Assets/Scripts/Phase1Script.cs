using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Phase1Script : MonoBehaviour
{
    private float timer;
    public float phaseDuration = 10f;
    public float transitionDelay = 3f;
    private bool timerRunning = true;
    public static int CurrentPhase { get; private set; } = 1;
    private int currentPhase = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = phaseDuration;
        CurrentPhase = 1;
        // currentPhase = 2;
        // CurrentPhase = 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPhase != 1) return;
        if (timerRunning)
        {
            timer -= Time.deltaTime;

            if (timer <= 0) {
                timer = 0;
                timerRunning = false;
                OnPhaseComplete();
            }
        }
    }

    public float GetCurrentTime() { return timer; }

    public int GetCurrentPhase()
    {
        return CurrentPhase;
    }

    void OnPhaseComplete()
    {
        Debug.Log("Phase 1 Complete!");
        //add code to transfer data and players to phase 2
        TransferPlayerDataToNextScene();
        StartCoroutine(PhaseTransition());
    }

    void TransferPlayerDataToNextScene()
    {
        Debug.Log("Transferring player data to Phase 2...");

        //Dummy logic for 4 players
        //if adding more players or playing with less, change this to a foreach (Player) function
        foreach (var player in GameData.players)
        {
            Debug.Log($"Player {player.playerIndex} — color: {player.colorIndex}, spells: {player.spells.Count}");
        }

        // Tell Phase 2 to use a single camera
        GameData.useSplitScreen = false;
    }

    IEnumerator PhaseTransition()
    {
        //wait 20 seconds before switching scenes
        yield return new WaitForSeconds(transitionDelay);
        // TransferPlayerDataToNextScene (); 

        //Load Phase 2 scene
        currentPhase = 2;
        CurrentPhase = 2;
        SceneManager.LoadScene("Phase2Arena");
    }
}
