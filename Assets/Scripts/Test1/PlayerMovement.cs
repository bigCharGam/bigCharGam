using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : PlayerBattle
{
    public enum PositionState { Grounded, Airborne }
    public enum ActionState { None, Locomotion, FastFalling, Dashing, Attacking, SkillUsing, Parrying }
    public enum BaldoState { Nabdo, Baldo }

    [Header("Parallel States")]
    [SerializeField] protected PositionState currentPosition = PositionState.Grounded;
    [SerializeField] protected ActionState currentAction = ActionState.None;

    [Header("Baldo System")]
    [SerializeField] private BaldoState currentBaldoState = BaldoState.Nabdo;
    [SerializeField][Range(0f, 1f)] private float baldoSpeedMultiplier = 0.6f;
    [SerializeField] private float baldoMotionDuration = 0.4f;
    [SerializeField] private float nabdoMotionDuration = 0.5f;

    [Header("Jump & Fall")]
    [SerializeField] private float jumpForce = 35f;
    [SerializeField] private float fastFallForce = 80f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    //인스펙터 창에서 가로(X)와 세로(Y) 크기를 직접 조절할 수 있도록 사각형 크기 변수(Vector2)로 변경
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1f, 2f);

    [Header("Dash")]
    [SerializeField] private float dashForce = 50f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashCooldown = 0.1f;

    // 내부 상태 및 컴포넌트 참조 변수
    protected Rigidbody2D rb;
    protected Vector2 moveInput;
    protected bool isGrounded;
    protected float dashTimeLeft;
    protected float dashCooldownTimer;
    protected float lastDirectionX = 1f; // 대시 방향 보존용 플래그
    private float baldoActionTimer = 0f;
    private Animator animator; // 애니 컴포넌트 참조용 변수

    // 입력 상태 판정 프로퍼티
    protected bool _isInputW => moveInput.y > 0.5f && currentAction != ActionState.Parrying;
    protected bool isPressingS => moveInput.y < -0.5f && currentAction != ActionState.Parrying;
    protected bool isPressingAD => moveInput.x != 0f && currentAction != ActionState.Parrying;

    // 외부 공격/스킬 스크립트나 타이머 상황 조회를 위한 프로퍼티 정의
    public BaldoState CurrentBaldoState
    {
        get => currentBaldoState;
        set => currentBaldoState = value;
    }

    // 발도/납도 진행 중(타이머 구동 중)인지 여부를 반환하는 프로퍼티
    public bool IsBaldoTransitioning => baldoActionTimer > 0f;

    // 1. 초기화
    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // 물리연산 중단 방지

        // 애니 오브젝트의 Animator 컴포넌트 자동 할당
        animator = GetComponent<Animator>();
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

        // 발도/납도 전환 타이머 실시간 마모 계산
        if (baldoActionTimer > 0f)
        {
            baldoActionTimer -= Time.deltaTime;
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

        // 애니 현재 스크립트 상태 머신 자산을 애니 파라미터에 실시간 동기화
        if (animator != null)
        {
            // 이동 중(Locomotion 상태이고 바닥에 있을 때) 파라미터 전달
            bool isMoving = (currentAction == ActionState.Locomotion && currentPosition == PositionState.Grounded);
            animator.SetBool("isMoving", isMoving);

            // [점프 애니메이션 연동 추가] 공중 상태(Airborne)인지 여부를 판단하여 애니메이터 변수에 주입
            bool isAirborne = (currentPosition == PositionState.Airborne);
            animator.SetBool("isAirborne", isAirborne);
        }
    }

    // 3. 물리연산 및 실시간 업데이트 메서드 (순수 물리 주입 공장)
    protected virtual void FixedUpdate()
    {
        // OverlapBox를 사용하여 인스펙터창에서 지정한 groundCheckSize 크기로 바닥 체크
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        currentPosition = isGrounded ? PositionState.Grounded : PositionState.Airborne;

        // 현재 발도 상태에 따른 실시간 이동 속도 보정 계산 적용
        float currentMoveSpeed = moveSpeed;
        if (currentBaldoState == BaldoState.Baldo)
        {
            currentMoveSpeed *= baldoSpeedMultiplier;
        }

        switch (currentAction)
        {
            case ActionState.Dashing:
                rb.linearVelocity = new Vector2(lastDirectionX * dashForce, 0f);
                break;

            case ActionState.FastFalling:
                rb.linearVelocity = new Vector2(moveInput.x * currentMoveSpeed, -fastFallForce);
                break;

            case ActionState.Locomotion:
                // 물리 주기 시점에 실시간으로 W(위쪽) 키 입력 확인 시 즉시 점프 주입
                float velYLoco = (currentPosition == PositionState.Grounded && _isInputW) ? jumpForce : rb.linearVelocity.y;
                rb.linearVelocity = new Vector2(moveInput.x * currentMoveSpeed, velYLoco);
                break;

            case ActionState.None:
                float targetX = (currentPosition == PositionState.Grounded) ? 0f : moveInput.x * currentMoveSpeed;
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

        // 공중 상태에서 대시 버튼 입력을 명확히 수신하되, 
        // 물리적인 대시 상태로 전이하지 않고 입력을 완전히 무효(무시) 처리합니다.
        if (currentPosition == PositionState.Airborne)
        {
            // 입력을 정상적으로 받았음을 인지하고 로그를 남긴 후, 아무런 상태 변화 없이 반환하여 무효화
            Debug.Log("공중 상태에서 대시 입력이 감지되어 무효 처리되었습니다.");
            return;
        }

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

    // 뉴 인풋 시스템 Baldo 액션(R키) 매핑 콜백 메서드
    private void OnBaldo()
    {
        // 발도 혹은 납도 모션이 재생 중인 타이머 도중에는 R키 중복 변환을 완전히 차단
        if (baldoActionTimer > 0f) return;

        // 공격 중이거나 패링 중, 스킬 사용 중일 때는 수동 납도/발도 전환 제한 (선택 사항)
        if (currentAction == ActionState.Attacking || currentAction == ActionState.SkillUsing || currentAction == ActionState.Parrying) return;

        // R키 입력 시마다 발도(Baldo) <-> 납도(Nabdo) 상태가 토글 전환됨
        if (currentBaldoState == BaldoState.Nabdo) // 원본의 오타인 BabdoState 수정을 피하기 위해 기존 명칭 구조 및 변환 타이머 규칙을 충실히 보존합니다.
        {
            currentBaldoState = BaldoState.Baldo;
            baldoActionTimer = baldoMotionDuration; // 발도 모션 시간 구동
            Debug.Log($"납도 상태에서 발도 상태로 변경되었습니다. ({baldoMotionDuration}초 동안 모션 대기)");
        }
        else
        {
            currentBaldoState = BaldoState.Nabdo;
            baldoActionTimer = nabdoMotionDuration; // 납도 모션 시간 구동
            Debug.Log($"발도 상태에서 납도 상태로 변경되었습니다. ({nabdoMotionDuration}초 동안 모션 대기)");
        }
    }

    // 자식 또는 외부 스크립트가 안전하게 발도 상태를 보장받을 수 있도록 유틸리티 메서드 구축
    /// <summary>
    /// 공격이나 스킬 사용 전 발도 상태를 보장합니다. 납도 상태일 경우 발도로 변경합니다.
    /// </summary>
    /// <returns>동작을 즉시 진행해도 되는지 여부 (발도/납도 상태 변환 모션 작동 중일 때는 false 반환)</returns>
    public bool EnsureBaldoState()
    {
        // 1. 이미 발도 중이거나 납도 중(타이머 구동 중)이라면 행동 진행 차단
        if (baldoActionTimer > 0f)
        {
            return false;
        }

        // 2. 완벽한 납도 상태라면 발도로 강제 전환하고 발도 타이머 작동 시작
        if (currentBaldoState == BaldoState.Nabdo)
        {
            currentBaldoState = BaldoState.Baldo;
            baldoActionTimer = baldoMotionDuration;
            Debug.Log($"납도 상태에서 공격/스킬이 입력되어 발도로 강제 전환됩니다. ({baldoMotionDuration}초 대기)");

            return false;
        }

        return true; // 이미 완벽히 발도가 완료된 상태라면 즉시 True를 주어 다음 행동 진행 허용
    }

    // 바닥 체크 판정 범위 사각형(DrawWireCube) 기즈모로 바꾸어, 인스펙터창 조절 값이 유니티 씬 뷰에 실시간 연동
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}