using UnityEngine;

/// <summary>
/// 护盾需带 Collider2D；若子弹为 Trigger，护盾 Collider2D 建议勾选 Is Trigger，并挂 Kinematic Rigidbody2D。
/// </summary>
public class ReflectShieldSpell : SpellBehavior
{
    public override void Execute(GameObject caster, Transform firePoint)
    {
        transform.position = firePoint.position;
        transform.rotation = firePoint.rotation;
        transform.SetParent(firePoint);
    }

    public override void StopExecute()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        SpellProjectile incomingProjectile = hitInfo.GetComponent<SpellProjectile>();
        if (incomingProjectile == null) return;

        incomingProjectile.transform.Rotate(0, 0, 180f);

        Transform firePointTr = transform.parent;
        Transform playerRoot = firePointTr != null ? firePointTr.parent : null;
        if (playerRoot != null)
            incomingProjectile.caster = playerRoot.gameObject;
    }
}
