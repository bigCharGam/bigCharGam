using UnityEngine;
using UnityEngine.InputSystem;

// [RequireComponent(typeof(Rigidbody2D))]     // 어트리뷰트) RequireComponen 인스펙터 창의 컴포넌트에 Rigidbody2d가 없으면 자동 추가
public class PlayerStats : CharacterBaseStats
{
    [Header("PlayerStats")]
    public float maxStamina;
    public float currentStamina;
    public float staminaRegenRate;
    public float staminaRegenDelay;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private Transform groundCheck;     // 형식) Transform, 여기선 실시간 위치 좌표
    [SerializeField] private LayerMask groundLayer;     // 형식) LayerMask, 특정 레이어의 오브젝트만 감지
    [SerializeField] private float groundCheckRadius = 0.1f;

    private Rigidbody2D rb;     // 형식) Rigidbody2D, 2D 물리 컴포넌트 클래스
    private Vector2 moveInput;      // 형식) Vector2, 2차원 좌표 값 구조체
    private bool isGrounded;

    private void Reset()        // 함수) Reset, 인스펙터 창에서 기본 값 자동 입력
    {
        maxHealth = 100f;
        moveSpeed = 10f;
        baseAttackPower = 10f;
        maxStamina = 100f;
        staminaRegenRate = 5f;
        staminaRegenDelay = 2f;
    }

    protected override void Start()     // 키워드) 오버라이드, 부모 내용 변경 및 추가
    {
        base.Start();
        currentStamina = maxStamina;
        rb = GetComponent<Rigidbody2D>();       // 변수) rb, GetComponent 캐싱 용도
                                                // 메서드) GetComponent, 해당 오브젝트의 컴포넌트 주솟값 색인
                                                // 제너릭) <>, 형식 종류에 맞게 파라미터 형식 변경
    }

    protected override void Update()
    {

        base.Update();
        /// [기능 요약] 
        // 키워드) base, 내 부모 클래스를 가리켜 인스턴스를 찾도록 함

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        /// [기능 요약] 캐릭터의 위치를 바닥인지 공중인지 바닥 체크
        // 클래스.메서드) Physics2D.OverlapCircle, 가상의 원을 그려 원 안에 겹치는(overlap) 오브젝트 확인
        // 변수) groundCheck.position, 원의 중심점
    }

    private void OnMove(InputValue value)
    /// [기능 요약] 뉴 인풋 시스템, 이벤트 기반 프로그래밍
    // 매개변수/지역변수) value, -
    {
        moveInput = value.Get<Vector2>();
        /// [기능 요약] 키보드의 입력 방향 값을 Vector2 형식으로 가져와 저장
        // 전역변수) moveInput, -

        
        if (moveInput.y > 0.5f && isGrounded)
        /// [기능 요약] '위쪽 방향키 입력'과 '바닥 상태 조건' 체크
        
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        /// [기능 요약] 조건을 만족하면 위로 점프 물리 주입
        // 빌트인 전역변수) rb.linearVelocity, 캐릭터의 실시간 물리 속도 값
    }

    private void FixedUpdate()
    /// [기능 요약] 유니티 물리 프레임 주기(초당 50번)에 맞춰 실시간 캐릭터 좌우 이동 속도 주입
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        // 전역변수) moveInput.x, OnMove 함수에서 저장된 플레이어의 실시간 좌우 입력 값 (-1 ~ 1)
        // 변수) rb.linearVelocity.y, 중력이나 점프에 의해 변화하는 현재 캐릭터의 상하 물리 속도 값
    }
}
