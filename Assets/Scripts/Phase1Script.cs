using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Phase1Script : MonoBehaviour
{
    private float timer;
    public float phaseDuration = 300f;
    private bool timerRunning = true;
    private int currentPhase = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = phaseDuration;
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

    void OnPhaseComplete()
    {
        Debug.Log("Phase 1 Complete!");
        //add code to transfer data and players to phase 2
        StartCoroutine(PhaseTransition());
    }

    void TransferPlayerDataToNextScene()
    {
        Debug.Log("Transferring player data to Phase 2...");

        //Dummy logic for 4 players
        //if adding more players or playing with less, change this to a foreach (Player) function
        for (int i = 0; i < 4; i++)
        {
            Debug.Log("Saving data for Player " + (i + 1));
        }
    }

    IEnumerator PhaseTransition()
    {
        //wait 20 seconds before switching scenes
        yield return new WaitForSeconds(20f);

        //call dummy function to carry over player data
        TransferPlayerDataToNextScene();

        //Load Phase 2 scene
        currentPhase = 2;
        SceneManager.LoadScene(1);
    }
}
