using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class VotingLogic : MonoBehaviour
{
    [SerializeField] private List<MinigameData> availableMinigames = new List<MinigameData>();
    // assign in Inspector. Add as many minigames with name, scene, useSplitScreen bool

    private Dictionary<int, MinigameData> playerVotes = new Dictionary<int, MinigameData>();
    private bool votingActive = false;
    [SerializeField] private float votingDuration = 30f;
    private Action<MinigameData> onVotingComplete;

    public void SetMinigames(List<MinigameData> games)
    {
        availableMinigames = games;
    }

    public void BeginVoting(Action<MinigameData> callback)
    {
        votingActive = true;
        onVotingComplete = callback;
        Debug.Log("Voting started for " + votingDuration + " seconds");
        StartCoroutine(VotingTimer());
    }

    IEnumerator VotingTimer()
    {
        yield return new WaitForSeconds(votingDuration);
        EndVoting();
    }

    public void RegisterVote(int playerIndex, MinigameData minigame)
    {
        if (!votingActive)
        {
            Debug.LogWarning("Vote ignored - voting not active");
            return;
        }

        if (minigame == null)
        {
            Debug.LogWarning("Invalid vote");
            return;
        }

        playerVotes[playerIndex] = minigame;
        Debug.Log($"Player {playerIndex} voted for {minigame.minigameName}");
    }

    void EndVoting()
    {
        votingActive = false;

        MinigameData chosen;

        if (playerVotes.Count == 0)
        {
            Debug.Log("No votes — picking random");
            chosen = availableMinigames[UnityEngine.Random.Range(0, availableMinigames.Count)];
        }
        else
        {
            chosen = GetWeightedResult();
        }

        Debug.Log("Selected: " + chosen.minigameName);

        // Use the minigame's own splitScreen setting — no hardcoding
        GameData.useSplitScreen = chosen.useSplitScreen;
        GameData.selectedMinigame = chosen.minigameName;

        onVotingComplete?.Invoke(chosen);

        SceneManager.LoadScene(chosen.sceneName);
    }

    MinigameData GetWeightedResult()
    {
        Dictionary<MinigameData, int> voteCounts = new Dictionary<MinigameData, int>();

        foreach (var vote in playerVotes.Values)
        {
            if (!voteCounts.ContainsKey(vote))
                voteCounts[vote] = 0;
            voteCounts[vote]++;
        }

        List<MinigameData> weightedPool = new List<MinigameData>();

        foreach (var pair in voteCounts)
            for (int i = 0; i < pair.Value; i++)
                weightedPool.Add(pair.Key);

        foreach (var item in weightedPool)
            Debug.Log($"Pool entry: {item.minigameName}");

        return weightedPool[UnityEngine.Random.Range(0, weightedPool.Count)];
    }
}