using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : PlayerBattle
{
    // 위치와 액션 (Enum)
    public enum PositionState { Grounded, Airborne }
    public enum ActionState { None, Locomotion, FastFalling, Dashing, Attacking, SkillUsing }

    [Header("Parallel States")]
    [SerializeField] protected PositionState currentPosition = PositionState.Grounded;
    [SerializeField] protected ActionState currentAction = ActionState.None;

    [Header("Jump & Fall")]
    [SerializeField] private float jumpForce = 35f;
    [SerializeField] private float fastFallForce = 80f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.1f;

    [Header("Dash")]
    [SerializeField] private float dashForce = 50f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashCooldown = 0.1f;

    protected Rigidbody2D rb;
    protected Vector2 moveInput;
    protected bool isGrounded;

    protected float dashTimeLeft;
    protected float dashCooldownTimer;
    protected float lastDirectionX = 1f; // 대시 방향 보존용 플래그

    // ⚡ [요청하신 새로운 C# 프로퍼티] 한 줄로 깔끔하게 하드웨어 실시간 입력 판정
    protected bool _isInputW => moveInput.y > 0.5f;
    protected bool isPressingS => moveInput.y < -0.5f;
    protected bool isPressingAD => moveInput.x != 0f;

    // 1. 초기화
    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // 물리연산 중단 방지
    }

    // 2. 프레임 (상태별 조건식 정의 및 시간 계산 부서)
    protected override void Update()
    {
        base.Update();

        // 쿨타임 타이머 관리
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // 현재 조작 상태 상자에 맞춰 분할 처리
        switch (currentAction)
        {
            case ActionState.None:
                if (currentPosition == PositionState.Airborne && isPressingS)
                {
                    currentAction = ActionState.FastFalling;
                }
                else if (isPressingAD)
                {
                    currentAction = ActionState.Locomotion;
                }
                break;

            case ActionState.Locomotion:
                if (currentPosition == PositionState.Airborne && isPressingS)
                {
                    currentAction = ActionState.FastFalling;
                }
                else if (!isPressingAD)
                {
                    currentAction = ActionState.None;
                }
                break;

            case ActionState.FastFalling:
                if (currentPosition == PositionState.Grounded || !isPressingS)
                {
                    currentAction = isPressingAD ? ActionState.Locomotion : ActionState.None;
                }
                break;

            case ActionState.Dashing:
                // 대시 지속 시간 마모 계산
                dashTimeLeft -= Time.deltaTime;
                if (dashTimeLeft <= 0)
                {
                    currentAction = isPressingAD ? ActionState.Locomotion : ActionState.None;
                }
                break;
        }
    }

    // 3. 물리연산 및 실시간 업데이트 메서드 (순수 물리 주입 공장)
    protected virtual void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        currentPosition = isGrounded ? PositionState.Grounded : PositionState.Airborne;

        switch (currentAction)
        {
            case ActionState.Dashing:
                rb.linearVelocity = new Vector2(lastDirectionX * dashForce, 0f);
                break;

            case ActionState.FastFalling:
                rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, -fastFallForce);
                break;

            case ActionState.Locomotion:
                // 물리 주기 시점에 실시간으로 W(위쪽) 키 입력 확인 시 즉시 점프 주입
                float velYLoco = (currentPosition == PositionState.Grounded && _isInputW) ? jumpForce : rb.linearVelocity.y;
                rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, velYLoco);
                break;

            case ActionState.None:
                float targetX = (currentPosition == PositionState.Grounded) ? 0f : moveInput.x * moveSpeed;
                // 물리 주기 시점에 실시간으로 W(위쪽) 키 입력 확인 시 즉시 점프 주입
                float velYNone = (currentPosition == PositionState.Grounded && _isInputW) ? jumpForce : rb.linearVelocity.y;
                rb.linearVelocity = new Vector2(targetX, velYNone);
                break;
        }
    }

    // 4. 이벤트 기반 콜백 메서드 (입력 데이터 수집 및 방향 보존)
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        // 좌우 이동 신호(A, D)가 실시간으로 들어올 때만 정직하게 바라보는 방향 최신화
        if (isPressingAD)
        {
            lastDirectionX = moveInput.x > 0f ? 1f : -1f;
        }
    }

    private void OnDash()
    {
        // 쿨타임 중이거나 이미 대시 중이면 무시
        if (dashCooldownTimer > 0 || currentAction == ActionState.Dashing) return;

        // 대시가 켜지는 찰나에 조작 방향키(A, D) 정보를 최종 강제 확정
        if (isPressingAD)
        {
            lastDirectionX = moveInput.x > 0f ? 1f : -1f;
        }

        // 대시 타이머 및 상태 자산 정의
        dashTimeLeft = dashDuration;
        dashCooldownTimer = dashCooldown;
        currentAction = ActionState.Dashing;
    }
}