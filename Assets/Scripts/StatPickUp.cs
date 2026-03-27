using UnityEngine;

public class StatPickUp : MonoBehaviour
{
    public string name;
    public float amount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStats playerstats = other.GetComponent<PlayerStats> ();

        if(playerstats != null)
        {
            playerstats.ModifyStat(name, amount);
            playerstats.RegisterPickup(name);

            Destroy(gameObject);
        }
    }
}
