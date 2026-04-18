using UnityEngine;
using System.Collections;

/// <summary>
/// 冰霜滑行：按住副键期间，速度变快，且摩擦力降到极低，实现“溜冰”手感。
/// Ice Slide: While holding sub button, increase speed and drop friction to simulate sliding on ice.
/// </summary>
public class IceSlideSpell : SpellBehavior
{
    [Header("滑行数值接口 — Ice Slide Tuning")]
    [Tooltip("滑行时的速度倍率 — Speed multiplier during slide.")]
    public float speedMultiplier = 2.5f;

    [Tooltip("滑行时的摩擦力（越小滑得越远，很难拐弯） — Friction during slide (lower = harder to turn).")]
    public float slideFriction = 1f;

    private PlayerController controller;
    private float originalDeceleration;

    public override void Execute(GameObject caster, Transform firePoint)
    {
        controller = caster.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ApplySprintMultiplier(speedMultiplier);
            originalDeceleration = controller.moveDeceleration;
            controller.currentDeceleration = slideFriction;
            
            transform.SetParent(caster.transform);
        }
    }

    public override void StopExecute()
    {
        if (controller != null)
        {
            controller.ClearSprintMultiplier();
            controller.currentDeceleration = originalDeceleration;
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (controller != null)
        {
            controller.ClearSprintMultiplier();
            controller.currentDeceleration = originalDeceleration; // 兜底恢复
        }
    }
}