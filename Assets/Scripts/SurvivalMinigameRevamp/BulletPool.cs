using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance;

    [Header("Pool Settings")]
    [SerializeField] 
    private GameObject bulletPrefab;
    [SerializeField] 
    private int initialPoolSize = 200;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;

        CreatePool();
    }

    private void CreatePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);

            bullet.SetActive(false);

            pool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet()
    {
        if (pool.Count == 0)
        {
            ExpandPool();
        }

        GameObject bullet = pool.Dequeue();

        bullet.SetActive(true);

        return bullet;
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);

        pool.Enqueue(bullet);
    }

    private void ExpandPool()
    {
        GameObject bullet = Instantiate(bulletPrefab);

        bullet.SetActive(false);

        pool.Enqueue(bullet);
    }
}