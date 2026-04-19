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

    public List<OptionUI> options = new List<OptionUI>();

    void Start()
    {
        SpawnOptions();
        SpawnPlayers();
        
        votingLogic.SetMinigames(minigames); // ADD THIS
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
        Debug.Log($"PlayerInput.all count: {PlayerInput.all.Count}");
        
        foreach (var p in GameData.players)
        {
            Debug.Log($"Spawning vote marker for player {p.playerIndex}, device: {p.device?.displayName}");
            
            GameObject marker = Instantiate(markerPrefab);
            PlayerVoteController pvc = marker.GetComponent<PlayerVoteController>();
            if (pvc == null)
                pvc = marker.AddComponent<PlayerVoteController>();

            pvc.Init(p.playerIndex, p.colorIndex, this);
        }
    }

    // called by PlayerVoteController — pass the MinigameData directly
    public void RegisterVote(int playerIndex, int optionIndex)
    {
        MinigameData minigame = minigames[optionIndex];
        votingLogic.RegisterVote(playerIndex, minigame);
    }

    void OnVotingComplete(MinigameData winner)
    {
        // VotingLogic already handles scene loading
        // this callback is just for any extra UI you want to show on result
        Debug.Log($"Voting complete — {winner.minigameName} won");
    }
}