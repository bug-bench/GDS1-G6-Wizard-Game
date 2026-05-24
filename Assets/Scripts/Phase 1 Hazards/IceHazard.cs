using UnityEngine;

public class IceHazard : MonoBehaviour
{
    


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("inside ice");
        PlayerController controller =other.GetComponentInParent<PlayerController>();

        if(controller != null)
        {
            controller.applyIce();
        }
       
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController controller = other.GetComponentInParent<PlayerController>();

        if (controller != null)
        {
            controller.removeIce();
        }
    }

}
