using UnityEngine;

public class SprintSpell : SpellBehavior
{
    [Header("疾跑数值接口 — Sprint Tuning")]
    public float speedMultiplier = 2f;

    private PlayerController controller;

    public override void Execute(GameObject caster, Transform firePoint)
    {
        controller = caster.GetComponent<PlayerController>();
        if (controller != null)
        {
            // 用基准移速乘算，禁止对当前 moveSpeed 再 *=（否则会叠乘） — Multiply base speed only; do not stack *= on current speed.
            controller.ApplySprintMultiplier(speedMultiplier);
            transform.SetParent(caster.transform);
        }
    }

    public override void StopExecute()
    {
        if (controller != null)
            controller.ClearSprintMultiplier();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (controller != null)
            controller.ClearSprintMultiplier();
    }
}
