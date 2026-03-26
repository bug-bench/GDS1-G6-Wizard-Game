using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ArenaScript : MonoBehaviour
{
    private GameObject[] players;
    private List<GameObject> playersAlive = new List<GameObject>();
    private List<GameObject> playersEliminated = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            playersAlive.Add(player);
        }
    }

    public void PlayerEliminated(GameObject player)
    {
        if (playersAlive.Contains(player))
        {
            playersAlive.Remove(player);
            playersEliminated.Add(player);
            Debug.Log(player.name + " has been eliminated. Players remaining: " + playersAlive.Count);
        }
        else
        {
            Debug.LogWarning(player.name + " was not found in playersAlive list.");
        }

        if (playersAlive.Count == 1)
        {
            Debug.Log("Winner is: " + playersAlive[0].name);
            EndGame(playersEliminated, playersAlive[0]);
        }
        else if (playersAlive.Count == 0)
        {
            Debug.Log("No players remaining. Draw!");
            EndGame(playersEliminated);
        }
    }

    //Only happens during a draw
    private void EndGame(List<GameObject> eliminations)
    {
        //add draw functionality here
        Debug.Log("The game is a draw! Nobody wins!");
        Debug.Log("Participating players: ");
        for (int i = 0; i < eliminations.Count; i++)
        {
            Debug.Log(eliminations[i].name);
        }
    }

    //happens when there is only one player left standing
    private void EndGame(List<GameObject> eliminations, GameObject winner)
    {
        //add winning functionality here
        Debug.Log("The game ends with " + winner.name + " as the victor! Here are the results: ");
        Debug.Log("1: " + winner.name);
        for (int i = 0; i < eliminations.Count; i++)
        {
            Debug.Log(i + ": " + eliminations[-1].name);
            eliminations.RemoveAt(-1);
        }
    }
}
