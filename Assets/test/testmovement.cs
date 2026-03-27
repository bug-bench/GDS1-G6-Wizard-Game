using UnityEngine;

public class testmovement : MonoBehaviour
{
    //public float movementspeed = 5F;
    private Rigidbody2D rb;
    private Vector2 movement;

   private  PlayerStats stats;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        stats = GetComponent<PlayerStats>();
    }

   
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement = movement.normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * stats.speed;
    }
}
