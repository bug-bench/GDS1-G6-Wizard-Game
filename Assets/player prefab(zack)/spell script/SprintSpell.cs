using UnityEngine;

public class SprintSpell : SpellBehavior
{
    [Header("疾跑数值接口")]
    public float speedMultiplier = 2f;

    private PlayerController controller;
    private float originalSpeed;

    public override void Execute(GameObject caster, Transform firePoint)
    {
        controller = caster.GetComponent<PlayerController>();
        if (controller != null)
        {
            originalSpeed = controller.moveSpeed;
            controller.moveSpeed *= speedMultiplier;
            transform.SetParent(caster.transform);
        }
    }

    public override void StopExecute()
    {
        if (controller != null)
        {
            controller.moveSpeed = originalSpeed;
        }
        Destroy(gameObject);
    }
}
