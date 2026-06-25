using System.Collections.Generic;
using UnityEngine;

public class FireHazard : MonoBehaviour
{
    public float fireDamage = 2f;
    public float tick = 3f;
    private Dictionary<PlayerStats, float> timers = new Dictionary<PlayerStats, float>();
    private List<PlayerStats> PlayersInside = new List<PlayerStats>();

     void Update()
    {
        foreach (PlayerStats stats in PlayersInside)
        {
            timers[stats] += Time.deltaTime;
            if (timers[stats] >= tick)
            {
                
                stats.TakeDamage(fireDamage);
                timers[stats] = 0f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(" inside fire");

        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats == null) return;
        if(!PlayersInside.Contains(stats))
        {
            PlayersInside.Add(stats);
            timers[stats] = 0f;
        }
        



        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerStats stats = other.GetComponent<PlayerStats>();
        if(stats != null)
        {
            PlayersInside.Remove(stats);
            timers.Remove(stats);
        }
    }
}
