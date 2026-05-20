
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        
    }

    public void SetupPlayer(GameObject player)
    {
        players.Add(player);
        PlayerCollectTracker.Add(player, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Phase2Collect")) return;
        CollectPickupSpawner cps = FindFirstObjectByType<CollectPickupSpawner>();
        if (TimerEnded == true)
        {
            cps.StopSpawning();
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
        else
        {
            for (int i = 0; i < amount; i++)
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

    public int GetPlayerScore(GameObject player)
    {
        if (PlayerCollectTracker.ContainsKey(player))
            return PlayerCollectTracker[player];

        return 0;
    }

    public Dictionary<GameObject, int> GetScores()
    {
        return PlayerCollectTracker;
    }

    public List<KeyValuePair<GameObject, int>> GetRankings()
    {
        List<KeyValuePair<GameObject, int>> rankings =
            new List<KeyValuePair<GameObject, int>>(PlayerCollectTracker);

        rankings.Sort((a, b) => b.Value.CompareTo(a.Value));

        return rankings;
    }
}
