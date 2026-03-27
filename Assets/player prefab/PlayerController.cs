using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public bool canMove = true; // 用于受击硬直控制

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Input System 自动调用的方法 (对应 Move Action)
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Debug.Log("收到移动输入了！当前方向：" + moveInput); // <--- 加入这一行
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            // 俯视角平滑移动
            rb.linearVelocity = moveInput * moveSpeed;
            
            // 纯正 2D 转向：只改 Z，避免向下走时 Quaternion 把 X 翻到 -180（摄像机子物体被甩飞）
            if (moveInput != Vector2.zero)
            {
                float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // 硬直时停下
        }
    }
}