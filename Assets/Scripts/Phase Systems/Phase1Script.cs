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

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI[] timerTexts;    

    // [SerializeField] bool useSplitScreen = false; // Set Inspector to control split-screen for Phase 2
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = phaseDuration;
        CurrentPhase = currentPhase;
        // currentPhase = 2;
        // CurrentPhase = 2;
        StartCoroutine(FindTimerTextsNextFrame());
    }

    IEnumerator FindTimerTextsNextFrame()
    {
        yield return null; // wait for PlayerSpawner to spawn players
        
        var allTexts = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
        var found = new System.Collections.Generic.List<TMPro.TextMeshProUGUI>();
        
        foreach (var t in allTexts)
            if (t.gameObject.name == "TimerText") // match by name
                found.Add(t);
        
        timerTexts = found.ToArray();
        Debug.Log($"Phase1Script found {timerTexts.Length} timer texts");
    }

    void UpdateTimerUI(string value)
    {
        if (timerTexts == null) return;
        foreach (var t in timerTexts)
            if (t != null) t.text = $"Time Left: {value}";
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPhase != 1) return;
        if (timerRunning)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI(Mathf.CeilToInt(timer).ToString());

            if (timer <= 0)
            {
                timer = 0;
                timerRunning = false;
                UpdateTimerUI("0");
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
        // GameData.useSplitScreen = useSplitScreen;
    }

    IEnumerator PhaseTransition()
    {
        //wait 20 seconds before switching scenes
        yield return new WaitForSeconds(transitionDelay);
        // TransferPlayerDataToNextScene (); 

        SceneManager.LoadScene("VotingScene");

        //Load Phase 2 scene
        // currentPhase = 2;
        // CurrentPhase = 2;
        
    }
}
