using UnityEngine;

public abstract class SpellBehavior : MonoBehaviour
{
    public abstract void Execute(GameObject caster, Transform firePoint);

    public virtual void StopExecute() { }
}
