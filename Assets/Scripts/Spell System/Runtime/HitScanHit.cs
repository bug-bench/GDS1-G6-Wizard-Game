using UnityEngine;

public struct HitScanHit
{
    public Collider2D collider;
    public Vector2 point;
    public Vector2 normal;

    public HitScanHit(
        Collider2D collider,
        Vector2 point,
        Vector2 normal)
    {
        this.collider = collider;
        this.point = point;
        this.normal = normal;
    }
}