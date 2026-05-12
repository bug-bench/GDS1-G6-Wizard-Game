using UnityEngine;
using UnityEngine.Audio;

public class StatPickUp : MonoBehaviour
{
    public string statName;
    public float amount;

    private AudioSource pickupSound;

    private StatSpawner spawner;
    // Start is called once before the first execution of Update after the MonoBehaviour is created



    void Start()
    {
        pickupSound = GetComponent<AudioSource>();
    }

    public void SetSpawner(StatSpawner statSpawner)
    {
        spawner = statSpawner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStats playerstats = other.GetComponent<PlayerStats> ();

        if (playerstats != null)
        {
            playerstats.ModifyStat(statName, amount);
            playerstats.IncrementStatCount(statName);
            playerstats.RegisterPickup(statName);

            other.GetComponent<FloatingStatSpawner>()?.ShowFloatingText(statName, 1);

            if (pickupSound != null)
            {
                pickupSound.Play();
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<Collider2D>().enabled = false;
                Destroy(gameObject, pickupSound.clip.length);
            }
            else
            {
                Destroy(gameObject);
            }

            if (spawner != null)
                spawner.RespawnStats();
        }
    }
}
