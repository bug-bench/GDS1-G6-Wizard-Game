using System.Collections;
using UnityEngine;

public class CollectPickup : MonoBehaviour
{
    protected CollectManager cm;
    protected GameObject lastDroppedPlayer = null;
    private bool canAllPickup = true;
    private bool canLastPickup = true;
    private CollectPickupSpawner spawner;

    void Start()
    {
        cm = FindFirstObjectByType<CollectManager>();
    }

    void Update()
    {
        if (cm == null)
        {
            cm = FindFirstObjectByType<CollectManager>();
        }
    }

    void SetSpawner(CollectPickupSpawner pickupSpawner)
    {
        spawner = pickupSpawner;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (cm == null) return;
        if (canAllPickup == false) return;
        if (col.gameObject == lastDroppedPlayer && canLastPickup == false) return;
        if (col.gameObject.CompareTag("Player"))
        {
            cm.RegisterPickup(col.gameObject);
            Destroy(gameObject);
            if (spawner != null)
            {
                spawner.RespawnPickups();
            }
        }
    }

    public void RegisterLastPlayer(GameObject player)
    {
        lastDroppedPlayer = player;
    }
    public void StartDrop()
    {
        StartCoroutine(BeginInvincibility());
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    IEnumerator BeginInvincibility()
    {
        canAllPickup = false;
        canLastPickup = false;
        yield return new WaitForSeconds(2f);
        canAllPickup = true;
        yield return new WaitForSeconds(1f);
        canLastPickup = true;
    }
}
