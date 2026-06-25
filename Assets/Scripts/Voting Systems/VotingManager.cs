using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class VotingManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject optionPrefab;
    public GameObject markerPrefab;

    [Header("UI")]
    public Transform container;

    [Header("Data")]
    public List<MinigameData> minigames; // all minigames

    [Header("Voting Options")]
    [Range(2, 6)]
    public int numberOfOptions = 2; // limit of options to pick from the pool, can be set in inspector

    [Header("Logic")]
    public VotingLogic votingLogic;
    public SelectionAnimator selectionAnimator;

    public List<OptionUI> options = new List<OptionUI>();
    private List<MinigameData> selectedMinigames = new List<MinigameData>(); // the picked subset

    private int[] voteCounts;

    void Start()
    {
        selectedMinigames = PickRandomMinigames();
        voteCounts = new int[selectedMinigames.Count];
        SpawnOptions();
        SpawnPlayers();
        votingLogic.SetMinigames(selectedMinigames);
        votingLogic.BeginVoting(OnVotingComplete);
    }

    List<MinigameData> PickRandomMinigames()
    {
        // Shuffle a copy of the pool and take the first numberOfOptions
        List<MinigameData> pool = new List<MinigameData>(minigames);

        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = pool[i];
            pool[i] = pool[j];
            pool[j] = temp;
        }

        int count = Mathf.Min(numberOfOptions, pool.Count);
        return pool.GetRange(0, count);
    }

    void SpawnOptions()
    {
        foreach (var game in selectedMinigames)
        {
            GameObject obj = Instantiate(optionPrefab, container);
            OptionUI option = obj.GetComponent<OptionUI>();
            option.Setup(game);
            options.Add(option);
        }
    }

    void SpawnPlayers()
    {
        foreach (var p in GameData.players)
        {
            GameObject marker = Instantiate(markerPrefab);
            PlayerVoteController pvc = marker.GetComponent<PlayerVoteController>();
            if (pvc == null) pvc = marker.AddComponent<PlayerVoteController>();
            pvc.Init(p.playerIndex, p.colorIndex, this);
        }
    }

    public void RegisterVote(int playerIndex, int optionIndex)
    {
        voteCounts[optionIndex]++;
        options[optionIndex].SetVotes(voteCounts[optionIndex]);

        MinigameData minigame = selectedMinigames[optionIndex];
        votingLogic.RegisterVote(playerIndex, minigame);
    }

    void OnVotingComplete(MinigameData winner)
    {
        List<OptionUI> votedOptions = new List<OptionUI>();
        for (int i = 0; i < options.Count; i++)
            if (voteCounts[i] > 0)
                votedOptions.Add(options[i]);

        if (votedOptions.Count == 0)
            votedOptions = new List<OptionUI>(options);

        if (selectionAnimator == null)
        {
            Debug.LogError("SelectionAnimator not assigned — loading scene directly.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(winner.sceneName);
            return;
        }

        selectionAnimator.PlaySelection(votedOptions, winner);
    }
}