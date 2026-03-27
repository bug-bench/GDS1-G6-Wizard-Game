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
            
            // 简单的面朝向转向 (可选，用于发射法术时的方向)
            if (moveInput != Vector2.zero)
            {
                transform.up = moveInput; 
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // 硬直时停下
        }
    }
}