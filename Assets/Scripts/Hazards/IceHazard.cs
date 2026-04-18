using UnityEngine;

public class IceHazard : MonoBehaviour
{
    


    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController controller =other.GetComponent<PlayerController>();

        controller.applyIce();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();

        controller.removeIce();
    }

}
