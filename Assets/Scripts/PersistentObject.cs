using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    public static PersistentObject Instance;
    void Awake()
    {
        // if (Instance != null && Instance != this)
        // {
        //     Destroy(gameObject);
        //     return;
        // }

        // Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
