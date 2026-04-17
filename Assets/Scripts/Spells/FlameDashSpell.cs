using UnityEngine;
using System.Collections;

/// <summary>
/// 火焰冲刺：按住副键期间加速，并在身后不断留下火焰轨迹。
/// Flame Dash: Sprint while leaving a trail of fire hazards behind.
/// </summary>
public class FlameDashSpell : SpellBehavior
{
    [Header("冲刺数值 — Dash Tuning")]
    public float speedMultiplier = 2f;

    [Header("火焰轨迹 — Fire Trail")]
    [Tooltip("要留在地上的火焰预制体（需挂载 HazardArea 脚本） — Fire hazard prefab to leave behind.")]
    public GameObject fireTrailPrefab;
    
    [Tooltip("每隔多少秒生成一团火 — How often to spawn a fire hazard.")]
    public float spawnInterval = 0.15f;

    private PlayerController controller;
    private Coroutine trailRoutine;

    public override void Execute(GameObject caster, Transform firePoint)
    {
        controller = caster.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ApplySprintMultiplier(speedMultiplier);
            transform.SetParent(caster.transform);

            if (fireTrailPrefab != null)
            {
                trailRoutine = StartCoroutine(SpawnTrailRoutine(caster.transform));
            }
        }
    }

    IEnumerator SpawnTrailRoutine(Transform casterTransform)
    {
        while (true)
        {
            // 在脚下生成火焰
            GameObject trail = Instantiate(fireTrailPrefab, casterTransform.position, Quaternion.identity);
            HazardArea hazard = trail.GetComponent<HazardArea>();
            if (hazard != null)
            {
                hazard.caster = casterTransform.gameObject;
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public override void StopExecute()
    {
        if (controller != null)
            controller.ClearSprintMultiplier();
            
        if (trailRoutine != null)
            StopCoroutine(trailRoutine);

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (controller != null)
            controller.ClearSprintMultiplier();
    }
}