using UnityEngine;

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
    }
}
