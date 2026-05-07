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
    public List<MinigameData> minigames;

    [Header("Logic")]
    public VotingLogic votingLogic;
    public SelectionAnimator selectionAnimator;

    public List<OptionUI> options = new List<OptionUI>();

    // track vote counts per option index for live display
    private int[] voteCounts;

    void Start()
    {
        voteCounts = new int[minigames.Count];
        SpawnOptions();
        SpawnPlayers();
        votingLogic.SetMinigames(minigames);
        votingLogic.BeginVoting(OnVotingComplete);
    }

    void SpawnOptions()
    {
        foreach (var game in minigames)
        {
            GameObject obj = Instantiate(optionPrefab, container);
            OptionUI option = obj.GetComponent<OptionUI>();
            option.Setup(game.minigameName, game.sceneName);
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
        // Update live vote count on the card
        voteCounts[optionIndex]++;
        options[optionIndex].SetVotes(voteCounts[optionIndex]);

        MinigameData minigame = minigames[optionIndex];
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

        // Guard — if animator missing, just load directly
        if (selectionAnimator == null)
        {
            Debug.LogError("SelectionAnimator not assigned on VotingManager — loading scene directly.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(winner.sceneName);
            return;
        }

        selectionAnimator.PlaySelection(votedOptions, winner);
    }
}