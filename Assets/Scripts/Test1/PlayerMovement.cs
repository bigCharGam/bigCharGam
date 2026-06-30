using UnityEngine;
using UnityEngine.InputSystem;

// [RequireComponent(typeof(Rigidbody2D))]
// 어트리뷰트) RequireComponen 인스펙터 창의 컴포넌트에 Rigidbody2d가 없으면 자동 추가

public class PlayerMovement : PlayerStats
{
    public enum PlayerState
    {
        Idle,
        Move,
        Jump,
        Dash
    }
    [Header("Current State")]
    [SerializeField] private PlayerState currentState = PlayerState.Idle;
        // 변수) 현재 플레이어의 실시간 상태 (한개)
    
    [Header("Jump & Fall")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float fastFallForce = 20f; // 변수) S키로 하강할 때 아래로 내리꽂을 힘의 크기
    [SerializeField] private Transform groundCheck;     // 형식) Transform, 여기선 실시간 위치 좌표
    [SerializeField] private LayerMask groundLayer;     // 형식) LayerMask, 특정 레이어의 오브젝트만 감지
    [SerializeField] private float groundCheckRadius = 0.1f;

    [Header("Dash")]
    [SerializeField] private float dashForce = 25f;       // 변수) 대시할 때 순간적으로 주입할 속도의 크기
    [SerializeField] private float dashDuration = 0.2f;    // 변수) 대시가 지속될 시간 (초 단위)
    [SerializeField] private float dashCooldown = 1f;      // 변수) 대시 재사용 대기 시간 (초 단위)
    [SerializeField] private float doubleTapTime = 0.25f;  // 변수) 더블 탭으로 인정할 두 클릭 사이의 최대 시간 간격

    private Rigidbody2D rb;     // 형식) Rigidbody2D, 2D 물리 컴포넌트 클래스
    private Vector2 moveInput;      // 형식) Vector2, 2차원 좌표 값 구조체
    private bool isGrounded;

    // 대시 제어용 상태 변수들
    private float dashTimeLeft;     // 변수) 대시 남은 시간 타이머
    private float dashCooldownTimer;// 변수) 대시 쿨타임 타이머
    private float lastInputTimeX;   // 변수) 마지막으로 좌우 입력을 가했던 실시간 시점 저장
    private float lastDirectionX;   // 변수) 바로 직전에 입력했던 좌우 방향 저장 (-1 또는 1)

    // 1. 초기화
    protected override void Start()     // 키워드) 오버라이드, 부모 내용 변경 및 추가
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();       // 변수) rb, GetComponent 캐싱 용도
                                                // 메서드) GetComponent, 해당 오브젝트의 컴포넌트 주솟값 색인
                                                // 제너릭) <>, 형식 종류에 맞게 파라미터 형식 변경
    }

    // 2. 프레임
    protected override void Update()
    {
        base.Update();
            /// [기능 요약] 스테미너 초기화
            // 키워드) base, 내 부모 클래스를 가리켜 인스턴스를 찾도록 함

        // [타이머 관리]: 대시 쿨타임이 남아있다면 실시간으로 시간을 차감
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
            // 전역변수) Time.deltaTime, 이전 프레임에서 현재 프레임까지 걸린 실제 연산 시간 (초)
        }

        // [상태 제어]: 대시 중일 때 타이머를 계산하고, 시간이 다 되면 상태를 변경
        if (currentState == PlayerState.Dash)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                // 대시가 끝난 순간 바닥에 있다면 Move나 Idle로, 공중이라면 Jump 상태로 돌려놓음
                SetState(isGrounded ? (moveInput.x != 0f ? PlayerState.Move : PlayerState.Idle) : PlayerState.Jump);
            }
        }
    }

    // 3. 물리연산
    private void FixedUpdate()
        /// [기능 요약] 실시간 캐릭터 좌우 이동 속도 주입
        // 함수) FixedUpdate, 고정 프레임 속도로 업데이트

    {
        // 1) 물리 감시
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        /// [기능 요약] 캐릭터의 위치를 바닥인지 공중인지 바닥 체크
        // 클래스.메서드) Physics2D.OverlapCircle, 가상의 원을 그려 원 안에 겹치는(overlap) 오브젝트 확인
        // 변수) groundCheck.position, 원의 중심점


        // 2) 상태별 물리 분기 (switch문을 활용해 완벽하게 통로 분리)
        switch (currentState)
        {
            case PlayerState.Dash:
                // 대시 상태일 때는 다른 입력을 무시하고 오직 대시 물리만 주입
                rb.linearVelocity = new Vector2(lastDirectionX * dashForce, 0f);
                break;

            default:
                // 일반 상태(Idle, Move, Jump)일 때의 통합 물리 이동 및 하강 처리
                rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

                // 공중 급속 강하 제어
                if (!isGrounded && moveInput.y < -0.5f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fastFallForce);
                    SetState(PlayerState.Jump);
                }
                // 실시간 상태 업데이트 보정
                else if (isGrounded)
                {
                    SetState(moveInput.x != 0f ? PlayerState.Move : PlayerState.Idle);
                }
                else
                {
                    SetState(PlayerState.Jump);
                }
                break;
        }
    }

    // 4. 외부 신호
    private void OnMove(InputValue value)
        /// [기능 요약] 뉴 인풋 시스템, 이벤트 기반 프로그래밍
        // 매개변수/지역변수) value, -
    {
        Vector2 previousMoveInput = moveInput;
            // 지역변수) previousMoveInput, 이번 입력 직전의 이동 상태를 잠시 보관

        moveInput = value.Get<Vector2>();
        /// [기능 요약] 키보드의 입력 방향 값을 Vector2 형식으로 가져와 저장
        // 전역변수) moveInput, -

        // [대시 상태 차단]: 대시 중에는 새로운 방향키 조작이나 더블 탭 판정을 일절 연산하지 않고 차단
        if (currentState == PlayerState.Dash)
        {
            return;
        }

        // [더블 탭 대시 판정]: 키를 '누르는 순간'에만 트리거링 연산 수행 (이전 입력이 0이었고, 현재 입력이 발생했을 때)
        if (previousMoveInput.x == 0f && moveInput.x != 0f)
        {
            // 조건) 방금 누른 방향이 직전 방향과 같고,
            //      마지막 입력 후 지나간 시간이 doubleTapTime(0.25초) 이내이며,
            //      쿨타임이 끝났는가?
            if (moveInput.x == lastDirectionX && (Time.time - lastInputTimeX) <= doubleTapTime && dashCooldownTimer <= 0)
            {
                dashTimeLeft = dashDuration;
                dashCooldownTimer = dashCooldown;
                SetState(PlayerState.Dash); // 상태변경
            }
            else
            {
                // 조건을 만족하지 못했다면 현재 누른 방향과 시간을 다음 더블 탭 판정을 위해 상자에 기록함
                lastDirectionX = moveInput.x;
            }
            lastInputTimeX = Time.time; // 빌트인 전역변수) Time.time, 게임이 켜진 후 흘러온 총 실시간 누적 시간
        }

        if (moveInput.y > 0.5f && isGrounded)
        {
            /// [기능 요약] '위쪽 방향키 입력'과 '바닥 상태 조건' 체크

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                /// [기능 요약] 조건을 만족하면 위로 점프 물리 주입
                // 빌트인 전역변수) rb.linearVelocity, 캐릭터의 X, Y 실시간 물리 속도 값

            SetState(PlayerState.Jump);
        }

        if (moveInput.y < -0.5f && !isGrounded)
            /// [기능 요약] '아래쪽 방향키(S) 입력'과 '공중에 떠 있는 상태 조건' 체크
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fastFallForce);
                /// [기능 요약] 조건을 만족하면 아래로 즉시 내리꽂는 강제 하강 물리 주입
                // 빌트인 전역변수) rb.linearVelocity.y, 가로 속력은 보존한 상태로 세로 속력만 즉시 음수 기획 값으로 치환
         
            SetState(PlayerState.Jump);
        }
    }

    // =================================================================
    // C영역: 내부 관리용 헬퍼 유틸리티 메서드
    // =================================================================
    /// [기능 요약] 안전하게 상태를 전이(Transition)시키고 인스펙터 시각화 및 디버깅을 통합 제어하는 방
    private void SetState(PlayerState newState)
    {
        if (currentState == newState) return; // 이미 같은 상태면 연산 생략

        currentState = newState;

        // 💡 나중에 여기에 "상태별 애니메이션 트리거"를 한 줄만 적어주면 애니메이션 꼬임도 100% 예방됩니다!
        // 예: anim.SetInteger("State", (int)currentState);
    }
}
