using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class StatPickUp : MonoBehaviour
{
    private GameObject lastDroppedPlayer = null;
    private bool canAllPickup = true;
    private bool canLastPickup = true;
    public string statName;
    public float amount;

    private AudioSource pickupSound;

    private StatSpawner spawner;
    // Start is called once before the first execution of Update after the MonoBehaviour is created



    void Start()
    {
        pickupSound = GetComponent<AudioSource>();
    }

    public void RegisterLastPlayer(GameObject player)
    {
        lastDroppedPlayer = player;
    }

    public void StartDrop()
    {
        StartCoroutine(BeginInvincibility());
    }

    IEnumerator BeginInvincibility()
    {
        canAllPickup = false;
        canLastPickup = false;

        yield return new WaitForSeconds(0.75f);

        canAllPickup = true;

        yield return new WaitForSeconds(0.5f);

        canLastPickup = true;
    }

    public void SetSpawner(StatSpawner statSpawner)
    {
        spawner = statSpawner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (canAllPickup == false) return;
        if (other.gameObject == lastDroppedPlayer && canLastPickup == false) return;

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
