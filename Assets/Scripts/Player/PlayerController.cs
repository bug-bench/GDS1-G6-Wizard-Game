using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    //

    /// <summary>
    /// 开局移速，疾跑等效果只在此基础上乘算，避免多次 *= 叠加速。
    /// Base move speed from stats; sprint multiplies this only, avoiding stacked *= runaway speed.
    /// </summary>
    //public float BaseMoveSpeed { get; private set; }

    public float sprintMultiplier = 1f;
    [HideInInspector] public float currentDeceleration;

    [Header("Aiming Settings")]
    [Range(0f, 0.95f)]
    [Tooltip("手柄瞄准死区，防止摇杆漂移。 — Gamepad aim deadzone to ignore stick drift.")]
    public float gamepadAimDeadzone = 0.2f;

    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public bool canMove = true;

    [Header("Movement feel (PUBG-style / 惯性加速)")]
    [Range(1f, 80f)]
    [Tooltip("有输入时每秒增加的速度量（沿输入方向累加，再限制最大速度）；越小起步越慢。 — Acceleration added per second along input (then clamped); lower = slower ramp-up.")]
    public float moveAcceleration = 14f;
    [Range(1f, 80f)]
    [Tooltip("无输入时每秒沿当前速度方向减少的速率（越小滑得越远）。惯性弱时检查 Rigidbody2D Linear Damping 建议为 0。 — Decel along velocity when no input; set RB Linear Damping to 0 if slide feels weak.")]
    public float moveDeceleration = 10f;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private Camera myCam;

    private PlayerData playerData;
    private PlayerStats playerStats;

    private Vector2 rawAimInput; // 临时存储手柄右摇杆的原始数据 — Raw right-stick aim before deadzone.

    [Header("Rotation")]
    public Transform rotationPivot;   
    public Transform playerSprite;

    //Ice harzard 
    private bool onIce = false;

    public float iceAccelerationMultiplier = 0.3f;
    public float iceDecelerationMultiplier = 0.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        myCam = GetComponentInChildren<Camera>(); // 寻找属于这个玩家自己的摄像机 — This player's child camera (split-screen).

        // 确保 Z 轴旋转锁死，防止万向节死锁导致的后空翻 — Freeze Z rotation to avoid physics flip from gimbal issues.
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

       

        playerStats = GetComponent<PlayerStats>();
        currentDeceleration = moveDeceleration;
    }

    public void ApplySprintMultiplier(float multiplier)
    {
        sprintMultiplier = multiplier;
    }

    public void ClearSprintMultiplier()
    {
        sprintMultiplier = 1f;
    }

    // Input System 自动调用的移动方法 (WASD 或 左摇杆) — Send Messages: move (WASD or left stick).
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Input System 自动调用的瞄准方法 (对应 Aim Action，通常是右摇杆) — Send Messages: aim (usually right stick).
    void OnAim(InputValue value)
    {
        rawAimInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            // 1. PUBG-like: 沿输入方向累加速度 + 上限，松手沿原方向摩擦减速（惯性），不是每帧直接设为目标速度。
            // Additive accel along input, clamp speed; release decelerates along current velocity (momentum slide).
            float maxSpeed = playerStats != null ? playerStats.speed * sprintMultiplier : 5f;
            
            bool canInputMove = playerStats == null || (!playerStats.isStunned && !playerStats.isRooted);
            Vector2 input = canInputMove ? Vector2.ClampMagnitude(moveInput, 1f) : Vector2.zero;
            
            Vector2 v = rb.linearVelocity;
            float dt = Time.fixedDeltaTime;
            float accel = onIce ? moveAcceleration * iceAccelerationMultiplier : moveAcceleration;
            float decel = onIce ? moveDeceleration * iceDecelerationMultiplier : currentDeceleration;

            if (input.sqrMagnitude > 1e-6f)
            {
                Vector2 accelDir = input.normalized;
                float stick = input.magnitude;
                v += accelDir * (accel *stick * dt);
                if (v.magnitude > maxSpeed)
                    v = v.normalized * maxSpeed;
            }
            else
            {
                float spd = v.magnitude;
                if (spd > 1e-4f)
                {
                    float drop = decel * dt;
                    if (spd <= drop)
                        v = Vector2.zero;
                    else
                        v -= v.normalized * drop;
                }
                else
                    v = Vector2.zero;
            }

            rb.linearVelocity = v;

            // 2. 瞄准/转向逻辑（分设备独立处理） — Aim/rotation per device (mouse vs gamepad).
            bool canRotate = playerStats == null || !playerStats.isStunned;
            if (canRotate)
            {
                HandleRotation();
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // 核心函数：根据不同设备单独处理转向 — Resolve facing from mouse (KeyMouse) or right stick (Gamepad).
    void HandleRotation()
    {
        if (playerInput == null) return;

        Vector2 lookDir = Vector2.zero;

        if (playerInput.currentControlScheme == "KeyMouse")
        {
            Camera activeCam = (myCam != null && myCam.gameObject.activeSelf)
                ? myCam
                : Camera.main;

            if (activeCam == null || Mouse.current == null) return;

            Vector2 mouseWorldPos = activeCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            lookDir = mouseWorldPos - rb.position;
        }
        else if (playerInput.currentControlScheme == "Gamepad")
        {
            if (rawAimInput.sqrMagnitude > gamepadAimDeadzone * gamepadAimDeadzone)
                lookDir = rawAimInput;
        }

        if (lookDir.sqrMagnitude < 0.01f) return;

        // Rotate only the aim pivot (arrow etc)
        if (rotationPivot != null)
        {
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            rotationPivot.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Flip the sprite based on look direction only
        if (playerSprite != null)
        {
            playerSprite.localScale = new Vector3(
                lookDir.x < 0 ? 1 : -1,  // flip X based on left/right
                1,
                1
            );
        }
    }

    public void Init(PlayerData data)
    {
        playerData = data;
    }

    public void applyIce()
    {
        onIce = true;
    }

    public void removeIce()
    {
        onIce = false;
    }

}
