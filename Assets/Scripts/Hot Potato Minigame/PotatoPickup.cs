using UnityEngine;

public class PotatoPickup : MonoBehaviour
{
    [SerializeField]
    private float potatoTime = 60f;
    private float timer;
    private GameObject currentHolder;
    private PotatoManager manager;

    public void AssignPlayer(GameObject player)
    {
        currentHolder = player;

        Debug.Log($"{player.name} now has the potato.");
    }

    public void ResetTimer()
    {
        timer = potatoTime;
    }

    public void RemovePlayer()
    {
        currentHolder = null;
    }

    public GameObject GetCurrentPlayer()
    {
        return currentHolder;
    }

    public int GetRemainingTime()
    {
        return Mathf.CeilToInt(timer);
    }

    public bool HasHolder()
    {
        return currentHolder != null;
    }

    void Update()
    {
        if (!HasHolder()) return;

        if (timer <= 0f) return;
        
        timer -= Time.deltaTime;
        
        if (timer <= 0f)
        {
            timer = 0f;

            Explode();
        }
    }

    private void Explode()
    {
        if (currentHolder == null) return;

        Debug.Log($"{currentHolder.name} exploded!");

        PlayerCombat pc = currentHolder.GetComponent<PlayerCombat>();

        if (pc != null)
        {
            pc.TakeDamage(int.MaxValue, gameObject);
        }
    }
}
