using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Phase2HUD : MonoBehaviour
{
    public GameObject statCardPrefab; // assign Phase2StatCard prefab in Inspector

    private List<Phase2StatCard> statCards = new List<Phase2StatCard>();

    private void Start()
    {
        StartCoroutine(InitNextFrame());
    }

    private IEnumerator InitNextFrame()
    {
        yield return null;

        var players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.InstanceID);

        foreach (var stats in players)
        {
            GameObject cardGO = Instantiate(statCardPrefab, transform);
            Phase2StatCard card = cardGO.GetComponent<Phase2StatCard>();

            var playerInput = stats.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            int index = playerInput != null ? playerInput.playerIndex : 0;
            var data = GameData.players.Find(p => p.playerIndex == index);

            card.Init(stats, data);
            statCards.Add(card);
        }
    }
}