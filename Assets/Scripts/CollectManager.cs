using System.Collections.Generic;
using UnityEngine;

public class CollectManager : MonoBehaviour
{
    public GameObject Pickup;
    public float dropforce = 3f;
    protected bool TimerEnded = false;
    private List<GameObject> players = new List<GameObject>();
    private Dictionary<GameObject, int> PlayerCollectTracker = new Dictionary<GameObject, int>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var player in GameData.players)
        {
            players.Add(player.playerGameObject);
            PlayerCollectTracker.Add(player.playerGameObject, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (TimerEnded == true)
        {
            CollectPickup[] remainingPickups = FindObjectsByType<CollectPickup>(FindObjectsSortMode.None);
            foreach (CollectPickup pickup in remainingPickups)
            {
                pickup.Deactivate();
            }
        }
    }

    public GameObject RegisterGameEnd()
    {
        TimerEnded = true;
        GameObject top = null;
        int highest = 0;
        foreach (var Player in players)
        {
            if (PlayerCollectTracker[Player] > highest)
            {
                highest = PlayerCollectTracker[Player];
                top = Player;
            }
        }
        return top;
    }

    public void DropPickup(GameObject player, int amount)
    {
        PlayerCollectTracker[player] -= amount;
        if (PlayerCollectTracker[player] < 0)
        {
            int buffer = PlayerCollectTracker[player] + amount;
            PlayerCollectTracker[player] = 0;
            for (int i = 0; i < buffer; i++)
            {
                //instantiate a collectible at the player's location
                
                GameObject NewPickup = DropPickup(player.transform.position);
                CollectPickup colp = NewPickup.GetComponent<CollectPickup>();
                if (colp != null)
                {
                    colp.RegisterLastPlayer(player);
                    colp.StartDrop();
                }
            }
            
        }
    }

    GameObject DropPickup(Vector2 position)
    {

        Vector2 offset = Random.insideUnitCircle * 1f;
        GameObject drop = Instantiate(Pickup, position + offset, Quaternion.identity);

        Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();

        if(rb !=null)
        {
            Vector2 force = Random.insideUnitCircle * dropforce;
            rb.AddForce(force, ForceMode2D.Impulse);
        }
        return drop;
    }

    public void RegisterPickup(GameObject player)
    {
        PlayerCollectTracker[player] += 1;
    }
}
