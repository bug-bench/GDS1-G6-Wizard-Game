using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

public class VotingLogic : MonoBehaviour
{
    private Dictionary<int, string> playerVotes = new Dictionary<int, string>();
    private bool votingActive = false;
    private float votingDuration = 30f;
    private Action<string> onVotingComplete;
    private List<string> availableMinigames = new List<string>() {
        "Arena",
        "Survival"
    };

    public void BeginVoting(Action<string> callback)
    {
        votingActive = true;
        onVotingComplete = callback;

        Debug.Log("Voting started for " + votingDuration + " seconds");
        StartCoroutine(VotingTimer());
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator VotingTimer()
    {
        yield return new WaitForSeconds(votingDuration);
        EndVoting();
    }

    // CALLED BY UI BUTTONS
    public void RegisterVote(int playerIndex, string minigameName)
    {
        if (!votingActive)
        {
            Debug.LogWarning("Vote ignored - voting not active");
            return;
        }

        if (!availableMinigames.Contains(minigameName))
        {
            Debug.LogWarning("Invalid minigame vote: " + minigameName);
            return;
        }

        playerVotes[playerIndex] = minigameName;

        Debug.Log($"Player {playerIndex} voted for {minigameName}");
    }

    void EndVoting()
    {
        votingActive = false;

        if (playerVotes.Count == 0)
        {
            Debug.Log("No votes cast - selecting random minigame");
            string randomPick = availableMinigames[UnityEngine.Random.Range(0, availableMinigames.Count)];
            onVotingComplete?.Invoke(randomPick);
            return;
        }

        string chosen = GetWeightedResult();
        onVotingComplete?.Invoke(chosen);
    }

    string GetWeightedResult()
    {
        Dictionary<string, int> voteCounts = new Dictionary<string, int>();

        // Count votes
        foreach (var vote in playerVotes.Values)
        {
            if (!voteCounts.ContainsKey(vote))
                voteCounts[vote] = 0;

            voteCounts[vote]++;
        }

        // Build weighted list
        List<string> weightedPool = new List<string>();

        foreach (var pair in voteCounts)
        {
            for (int i = 0; i < pair.Value; i++)
                weightedPool.Add(pair.Key);
        }

        // Random pick
        int randIndex = UnityEngine.Random.Range(0, weightedPool.Count);
        return weightedPool[randIndex];
    }
}
