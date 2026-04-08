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
        CurrentPhase = currentPhase;
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
            GameObject playerObj = player.playerGameObject;
            if (playerObj.GetComponent<PersistentObject>() == null)
            {
                playerObj.AddComponent<PersistentObject>();
            }
        }

        // 进入 Phase2 竞技场前强制单相机（与 Phase2 共享镜头设计一致）；若竞技场也要分屏，勿在此处写死 false。
        // Force single shared camera before Phase2 arena; remove or gate this if arena should stay split-screen.
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
