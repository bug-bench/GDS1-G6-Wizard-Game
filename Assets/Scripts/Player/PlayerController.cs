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
    /// 基础移动速度，冲刺只会乘在这个速度上。
    /// Base move speed; sprint multiplies this value.
    /// </summary>
    //public float BaseMoveSpeed { get; private set; }

    public float sprintMultiplier = 1f;
    [HideInInspector] public float currentDeceleration;

    [Header("Aiming Settings")]
    [Range(0f, 0.95f)]
    [Tooltip("手柄瞄准死区，防止摇杆漂移。 — Gamepad aim deadzone.")]
    public float gamepadAimDeadzone = 0.2f;

    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public bool canMove = true;

    [Header("Movement feel (PUBG-style / 惯性加速)")]
    [Range(1f, 200f)]
    [Tooltip("移动加速度，数值越高起步越快。 — Movement acceleration.")]
    public float moveAcceleration = 60f;

    [Range(1f, 200f)]
    [Tooltip("基础减速度，数值越高停得越快。 — Base deceleration.")]
    public float moveDeceleration = 50f;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private Camera myCam;

    private PlayerData playerData;
    private PlayerStats playerStats;
    private PlayerCombat playerCombat;

    private Vector2 rawAimInput; // 手柄右摇杆输入 — Raw right-stick aim input.

    [Header("Rotation")]
    public Transform rotationPivot;
    public Transform playerSprite;

    // 冰面状态 — Ice state.
    private bool onIce = false;

    public float iceAccelerationMultiplier = 0.3f;
    public float iceDecelerationMultiplier = 0.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        myCam = GetComponentInChildren<Camera>(); // 玩家自己的摄像机 — This player's camera.

        // 锁定旋转，防止物理翻转。
        // Lock rotation to prevent physics flipping.
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        playerStats = GetComponent<PlayerStats>();
        playerCombat = GetComponent<PlayerCombat>();
        currentDeceleration = playerStats != null ? playerStats.deceleration : moveDeceleration;
    }

    public void ApplySprintMultiplier(float multiplier)
    {
        sprintMultiplier = multiplier;
    }

    public void ClearSprintMultiplier()
    {
        sprintMultiplier = 1f;
    }

    // 移动输入方法 — Movement input method.
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // 瞄准输入方法 — Aim input method.
    void OnAim(InputValue value)
    {
        rawAimInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            if (playerCombat != null && playerCombat.IsInKnockback)
            {
                if (playerStats == null || !playerStats.isStunned)
                    HandleRotation();
                return;
            }

            // 使用加速度移动，而不是直接设置速度。
            // Move using acceleration instead of setting velocity directly.
            float baseSpeed = playerStats != null ? playerStats.speed : 5f;
            float maxSpeed = baseSpeed * sprintMultiplier;

            bool canInputMove = playerStats == null || (!playerStats.isStunned && !playerStats.isRooted);
            Vector2 input = canInputMove ? Vector2.ClampMagnitude(moveInput, 1f) : Vector2.zero;

            Vector2 v = rb.linearVelocity;
            float dt = Time.fixedDeltaTime;

            // 读取当前减速度。
            // Read current deceleration.
            if (playerStats != null) currentDeceleration = playerStats.deceleration;
            else currentDeceleration = moveDeceleration;

            // 如果减速度大于或等于速度，松开移动键后立即停止。
            // If deceleration is equal to or higher than speed, stop instantly.
            bool frictionCanInstantStop = currentDeceleration >= maxSpeed;

            // 比较减速度和速度。
            // Compare deceleration against speed.
            float frictionToSpeedRatio = maxSpeed > 0f
                ? Mathf.Clamp01(currentDeceleration / maxSpeed)
                : 1f;

            // 保持起步灵敏。
            // Keep movement start responsive.
            float actualAccel = moveAcceleration;

            // 速度越高、减速度越低，玩家越滑。
            // Higher speed and lower deceleration means more sliding.
            float actualDecel = frictionCanInstantStop
                ? currentDeceleration
                : currentDeceleration * frictionToSpeedRatio;

            float accel = onIce ? actualAccel * iceAccelerationMultiplier : actualAccel;
            float decel = onIce ? actualDecel * iceDecelerationMultiplier : actualDecel;

            if (input.sqrMagnitude > 1e-6f)
            {
                Vector2 accelDir = input.normalized;
                float stick = input.magnitude;

                v += accelDir * (accel * stick * dt);

                if (v.magnitude > maxSpeed)
                    v = v.normalized * maxSpeed;
            }
            else
            {
                // 没有输入时减速。
                // Slow down when there is no input.
                if (frictionCanInstantStop && !onIce)
                {
                    v = Vector2.zero;
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
                    {
                        v = Vector2.zero;
                    }
                }
            }

            rb.linearVelocity = v;

            // 处理瞄准和转向。
            // Handle aiming and rotation.
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

    // 根据输入设备处理转向。
    // Handle rotation based on input device.
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

        // 旋转瞄准点。
        // Rotate the aim pivot.
        if (rotationPivot != null)
        {
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            rotationPivot.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 根据瞄准方向翻转角色。
        // Flip the player based on aim direction.
        if (playerSprite != null)
        {
            playerSprite.localScale = new Vector3(
                lookDir.x < 0 ? 1 : -1,
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