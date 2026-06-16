using UnityEngine;

public class WindHazard : MonoBehaviour
{
    public Vector2 windDirection = Vector2.right;
    public float strength = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        rb.linearVelocity += windDirection.normalized * strength * Time.deltaTime;
    }
}
