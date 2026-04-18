using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Aiming Settings")]
    public float gamepadAimDeadzone = 0.2f; // 手柄瞄准死区，防止摇杆漂移 — Gamepad aim deadzone to ignore stick drift.

    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public bool canMove = true;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private Camera myCam;

    private PlayerData playerData;
    private PlayerStats playerStats;

    private Vector2 rawAimInput; // 临时存储手柄右摇杆的原始数据 — Raw right-stick aim before deadzone.

    [Header("Rotation")]
    public Transform rotationPivot;   
    public Transform playerSprite;


    //Ice Hazard 
    private Vector2 currentVelocity;
    public float acceleration=10f;
    public float IceAcceleration = 2f;

    private bool onIce = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        myCam = GetComponentInChildren<Camera>(); // 寻找属于这个玩家自己的摄像机 — This player's child camera (split-screen).

        // 确保 Z 轴旋转锁死，防止万向节死锁导致的后空翻 — Freeze Z rotation to avoid physics flip from gimbal issues.
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

       

        playerStats = GetComponent<PlayerStats>();

        
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
            // 1. 移动逻辑（两者通用，只受 WASD/左摇杆 影响） — Movement from WASD / left stick only.
            //rb.linearVelocity = moveInput * moveSpeed;

            float currentSpeed = playerStats != null ? playerStats.speed * sprintMultiplier : 5f;
            //rb.linearVelocity = moveInput * currentSpeed;
            float accel = onIce ? IceAcceleration : acceleration;
            currentVelocity = Vector2.Lerp(currentVelocity, moveInput * currentSpeed, accel * Time.fixedDeltaTime);

            rb.linearVelocity = currentVelocity;

            // 2. 瞄准/转向逻辑（分设备独立处理） — Aim/rotation per device (mouse vs gamepad).
            HandleRotation();
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

    public void ApplyIce()
    {
       
        onIce = true;
       
    }

    public void removeIce()
    {
        onIce = false;
      
    }
}
