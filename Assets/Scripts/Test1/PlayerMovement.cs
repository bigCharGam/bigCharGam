using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : PlayerStats
{
    // 위치와 액션 (Enum)
    public enum PositionState { Grounded, Airborne }
    public enum ActionState { None, Locomotion, FastFalling, Dashing }

    [Header("Parallel States")]
    [SerializeField] private PositionState currentPosition = PositionState.Grounded;
    [SerializeField] private ActionState currentAction = ActionState.None;

    [Header("Jump & Fall")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float fastFallForce = 20f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.1f;

    [Header("Dash")]
    [SerializeField] private float dashForce = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float doubleTapTime = 0.25f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;

    private float dashTimeLeft;
    private float dashCooldownTimer;
    private float lastInputTimeX;
    private float lastDirectionX;

    // 1. 초기화
    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }

    // 2. 프레임 (타이머 수치 계산만 수행하는 매서드)
    protected override void Update()
    {
        base.Update();

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // 대시 중일 때의 독립적인 타이머 관리
        if (currentAction == ActionState.Dashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                // 대시가 끝난 순간 조작 상태를 재연산하여 복구
                currentAction = (moveInput.x != 0f) ? ActionState.Locomotion : ActionState.None;
            }
        }
    }

    // 3. 물리연산 및 실시간 업데이트 메서드
    private void FixedUpdate()
    {
        // 메서드 완성시 없앨 편의성 변수
        bool isPressingUp = moveInput.y > 0.5f;
        bool isPressingDown = moveInput.y < -0.5f;
        bool isPressingAD = moveInput.x != 0f;

        // [ 1) 위치] 실시간 위치 업데이트
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        currentPosition = isGrounded ? PositionState.Grounded : PositionState.Airborne;

        // [ 2) 액션] 대시 중이 아닐 때만 실시간 조작 상태 판단 및 전환
        if (currentAction != ActionState.Dashing)
        {
            // 공중이면서 아래 키를 누르고 있다면 -> 강하 조작 상태
            if (currentPosition == PositionState.Airborne && isPressingDown)
            {
                currentAction = ActionState.FastFalling;
            }
            // 그 외에 좌우 입력이 들어가고 있다면 -> 이동 조작 상태
            else if (isPressingAD)
            {
                currentAction = ActionState.Locomotion;
            }
            // 아무 조작도 안 하거나 키를 뗐다면 -> 기본 조작 상태
            else
            {
                currentAction = ActionState.None;
            }
        }

        // [ 3) 최종 물리 주입] 위치와 액션을 조합하여 물리 제어
        switch (currentAction)
        {
            case ActionState.Dashing:
                rb.linearVelocity = new Vector2(lastDirectionX * dashForce, 0f);
                break;

            case ActionState.FastFalling:
                rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, -fastFallForce);
                break;

            case ActionState.Locomotion:
                rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

                // 땅 위에서 위쪽 키 입력 시 점프 물리 즉시 발동
                if (currentPosition == PositionState.Grounded && isPressingUp)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
                break;

            case ActionState.None:
                // 땅 위면 멈춤, 공중이면 좌우 조작값만 반영하되 자연스러운 중력 하강 허용
                float targetX = (currentPosition == PositionState.Grounded) ? 0f : moveInput.x * moveSpeed;
                rb.linearVelocity = new Vector2(targetX, rb.linearVelocity.y);

                // 제자리 정지 중 위쪽 키 입력 시 점프 물리 즉시 발동
                if (currentPosition == PositionState.Grounded && isPressingUp)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
                break;
        }
    }

    // 4. 외부 신호 (오직 입력 데이터 수집 및 '순수 대시 조건'만 가볍게 판정)
    private void OnMove(InputValue value)
    {
        Vector2 previousMoveInput = moveInput;
        moveInput = value.Get<Vector2>();

        // 대시 중에는 새로운 방향 전환 및 연속 대시 트리거 연산을 차단
        if (currentAction == ActionState.Dashing) return;

        // 더블 탭 대시 판정
        if (previousMoveInput.x == 0f && moveInput.x != 0f)
        {
            if (moveInput.x == lastDirectionX && (Time.time - lastInputTimeX) <= doubleTapTime && dashCooldownTimer <= 0)
            {
                dashTimeLeft = dashDuration;
                dashCooldownTimer = dashCooldown;
                currentAction = ActionState.Dashing; // 조작 상태를 대시로 즉시 선점
                return;
            }
            else
            {
                lastDirectionX = moveInput.x;
            }
            lastInputTimeX = Time.time;
        }
    }
}