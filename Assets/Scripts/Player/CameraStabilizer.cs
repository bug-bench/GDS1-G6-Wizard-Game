using UnityEngine;

public class CameraStabilizer : MonoBehaviour
{
    void LateUpdate()
    {
        // 永远将摄像机的旋转角度锁死为 0，不论父物体（玩家）怎么转，它都不转。
        transform.rotation = Quaternion.identity;
    }
}