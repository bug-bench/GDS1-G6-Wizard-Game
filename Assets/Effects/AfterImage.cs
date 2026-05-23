using UnityEngine;

public class AfterImage : MonoBehaviour
{
    public float fadeTime = 0.25f;

    private SpriteRenderer sr;
    private float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        float alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);

        Color c = sr.color;
        c.a = alpha;
        sr.color = c;

        if (timer >= fadeTime)
            Destroy(gameObject);
    }
}
