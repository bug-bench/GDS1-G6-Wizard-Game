using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    /// <summary>开局移速，疾跑等效果只在此基础上乘算，避免多次 *= 叠加速。</summary>
    public float BaseMoveSpeed { get; private set; }

    [Header("Aiming Settings")]
    public float gamepadAimDeadzone = 0.2f; // 手柄瞄准死区，防止摇杆漂移

    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public bool canMove = true;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private Camera myCam;

    private Vector2 rawAimInput; // 临时存储手柄右摇杆的原始数据

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        myCam = GetComponentInChildren<Camera>(); // 寻找属于这个玩家自己的摄像机

        // 确保 Z 轴旋转锁死，防止万向节死锁导致的后空翻
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        BaseMoveSpeed = moveSpeed;
    }

    public void ApplySprintMultiplier(float multiplier)
    {
        moveSpeed = BaseMoveSpeed * multiplier;
    }

    public void ClearSprintMultiplier()
    {
        moveSpeed = BaseMoveSpeed;
    }

    // Input System 自动调用的移动方法 (WASD 或 左摇杆)
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Input System 自动调用的瞄准方法 (对应 Aim Action，通常是右摇杆)
    void OnAim(InputValue value)
    {
        rawAimInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            // 1. 移动逻辑（两者通用，只受 WASD/左摇杆 影响）
            rb.linearVelocity = moveInput * moveSpeed;

            // 2. 瞄准/转向逻辑（分设备独立处理）
            HandleRotation();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // 核心函数：根据不同设备单独处理转向
    void HandleRotation()
    {
        if (playerInput == null) return;

        // ==========================================================
        // 【键鼠模式】：360度鼠标指针瞄准
        // ==========================================================
        if (playerInput.currentControlScheme == "KeyMouse")
        {
            if (Mouse.current != null && myCam != null)
            {
                // 获取鼠标在屏幕上的坐标，并转化为属于这个玩家自己的摄像机世界坐标
                Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
                Vector2 mouseWorldPos = myCam.ScreenToWorldPoint(mouseScreenPos);

                // 计算玩家到鼠标的向量方向
                Vector2 lookDir = mouseWorldPos - rb.position;

                // 如果鼠标离玩家太近（比如重合），则不执行旋转，防止画面抽搐
                if (lookDir.sqrMagnitude < 0.01f) return;

                float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
                // 强制只在 Z 轴旋转
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
        // ==========================================================
        // 【手柄模式】：双摇杆射击 (Move=左摇杆，Aim=右摇杆)
        // ==========================================================
        else if (playerInput.currentControlScheme == "Gamepad")
        {
            // 检查右摇杆是否有输入（超过死区）
            if (rawAimInput.sqrMagnitude > gamepadAimDeadzone * gamepadAimDeadzone)
            {
                // 用右摇杆的绝对方向来计算角度，与左摇杆移动彻底拆分
                float angle = Mathf.Atan2(rawAimInput.y, rawAimInput.x) * Mathf.Rad2Deg - 90f;
                // 强制只在 Z 轴旋转
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
}
