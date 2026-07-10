using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : PlayerMovement
{
    private enum ParryDirection { None, W, A, D }

    // [기능 추가] 자동으로 발도 후 수행할 공격의 종류를 예약하기 위한 Enum
    private enum PendingAttackType { None, Normal, Big }

    [Header("Battle Status")]
    [SerializeField] private ParryDirection currentParryDirection = ParryDirection.None;

    // [기능 추가] 발도 전환 타이머가 끝난 뒤 즉시 공격을 실행하기 위한 상태 변수
    private PendingAttackType reservedAttack = PendingAttackType.None;
    private bool wasTransitioningLastFrame = false;

    [Header("Attack Settings")]
    [SerializeField] private float normalAttackDashForce = 12f;
    [SerializeField] private float bigAttackDashForce = 25f;

    [Header("Attack Duration Settings")]
    [SerializeField] private float normalAttackDuration = 0.15f;
    [SerializeField] private float bigAttackDuration = 0.25f;

    [Header("Attack Cooldown Settings")]
    [SerializeField] private float normalAttackCooldown = 0.2f;
    [SerializeField] private float bigAttackCooldown = 0.5f;

    [Header("Damage Settings")]
    [SerializeField] private int normalAttackDamage = 10;
    [SerializeField] private int bigAttackDamage = 20;

    [Header("Attack Range Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackSize = new Vector2(2f, 1f);
    [SerializeField] private LayerMask enemyLayers;

    private float attackDurationTimer;
    private float attackCooldownTimer; // 현재 남은 쿨타임을 계산할 타이머 변수 추가

    protected override void Start() => base.Start();

    protected override void Update()
    {
        base.Update();

        // 패링 중 Shift 키 해제 확인
        if (currentAction == ActionState.Parrying)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && !keyboard.leftShiftKey.isPressed && !keyboard.rightShiftKey.isPressed)
            {
                ForceExitParry();
            }
        }

        // [기능 추가] 실시간으로 부모의 발도/납도 타이머 종료 시점을 감지하여 예약된 공격 실행
        HandleReservedAttack();

        // 공격 타이머 관리
        if (attackDurationTimer > 0)
        {
            attackDurationTimer -= Time.deltaTime;
            if (attackDurationTimer <= 0)
            {
                if (currentAction == ActionState.Attacking || currentAction == ActionState.SkillUsing)
                {
                    currentAction = isPressingAD ? ActionState.Locomotion : ActionState.None;
                }
            }
        }

        // 쿨타임 타이머 관리
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (currentAction == ActionState.Attacking)
        {
            float currentDashForce = (attackDurationTimer > normalAttackDuration) ? bigAttackDashForce : normalAttackDashForce;
            rb.linearVelocity = new Vector2(lastDirectionX * currentDashForce, rb.linearVelocity.y);
        }
    }

    // [기능 추가] 발도 완료 프레임을 실시간 감지하여 예약 공격을 분기 처리하는 유틸리티 메서드
    private void HandleReservedAttack()
    {
        // 전 프레임에는 발도/납도 중이었는데, 이번 프레임에 타이머가 끝난 경우 (발도 완료 순간)
        if (wasTransitioningLastFrame && !IsBaldoTransitioning)
        {
            // 현재 상태가 완벽히 발도 완료(Baldo)이고 예약된 공격 자산이 존재할 때
            if (CurrentBaldoState == BaldoState.Baldo && reservedAttack != PendingAttackType.None)
            {
                Debug.Log($"발도 모션이 완료되어 예약된 공격({reservedAttack})을 수행합니다.");

                if (reservedAttack == PendingAttackType.Normal)
                {
                    ExecuteNormalAttackLogic();
                }
                else if (reservedAttack == PendingAttackType.Big)
                {
                    ExecuteBigAttackLogic();
                }
            }

            // 예약 자산 초기화
            reservedAttack = PendingAttackType.None;
        }

        // 다음 프레임 비교용 상태 최신화 보존
        wasTransitioningLastFrame = IsBaldoTransitioning;
    }

    // --- 패링 로직 ---
    private void HandleParryInput(ParryDirection direction, bool isPressed)
    {
        if (isPressed)
        {
            // [기능 추가] 현재 발도 모션 중이거나 납도 모션 중인 경우 패링 작동 제한
            if (IsBaldoTransitioning) return;

            // 공격, 대시, 스킬 중에는 패링 불가
            if (currentAction == ActionState.Attacking ||
                currentAction == ActionState.Dashing ||
                currentAction == ActionState.SkillUsing) return;

            // 이미 패링 중이라도 방향이 다르면 새 방향으로 갱신
            if (currentAction == ActionState.Parrying)
            {
                if (currentParryDirection == direction) return;

                // 방향이 다르면 즉시 교체
                currentParryDirection = direction;
                Debug.Log($"패링 방향 교체: {direction}");
                return;
            }

            // 패링 시작
            currentAction = ActionState.Parrying;
            currentParryDirection = direction;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            Debug.Log($"패링 자세 돌입: {direction}");
        }
        else
        {
            // 내가 뗀 버튼이 현재 패링 중인 방향과 일치할 때만 해제
            if (currentAction == ActionState.Parrying && currentParryDirection == direction)
            {
                Debug.Log($"패링 자세 해제: {direction}");
                ForceExitParry();
            }
        }
    }

    private void OnWParry(InputValue value) => HandleParryInput(ParryDirection.W, value.isPressed);
    private void OnAParry(InputValue value) => HandleParryInput(ParryDirection.A, value.isPressed);
    private void OnDParry(InputValue value) => HandleParryInput(ParryDirection.D, value.isPressed);

    private void ForceExitParry()
    {
        currentAction = ActionState.None;
        currentParryDirection = ParryDirection.None;
    }

    // --- 공격 로직 ---
    private void OnAttack()
    {
        // 대시 중이거나 이미 공격 중일 때, 그리고 쿨타임이 남아있을 때는 공격 불가
        if (currentAction == ActionState.Dashing || currentAction == ActionState.Attacking || attackCooldownTimer > 0) return;

        // 부모(PlayerMovement)에 구현된 발도 검증 로직 실행
        if (!EnsureBaldoState())
        {
            // [기능 추가] 전환 타이머 작동으로 차단된 경우, 일반 공격을 예약 상자에 주입
            reservedAttack = PendingAttackType.Normal;
            return;
        }

        ExecuteNormalAttackLogic();
    }

    private void OnBigAttack()
    {
        // 대시 중이거나 이미 공격 중일 때, 그리고 쿨타임이 남아있을 때는 공격 불가 조건 추가
        if (currentAction == ActionState.Dashing || currentAction == ActionState.Attacking || attackCooldownTimer > 0) return;

        // 부모(PlayerMovement)에 구현된 발도 검증 로직 실행
        if (!EnsureBaldoState())
        {
            // [기능 추가] 전환 타이머 작동으로 차단된 경우, 강공격을 예약 상자에 주입
            reservedAttack = PendingAttackType.Big;
            return;
        }

        ExecuteBigAttackLogic();
    }

    // [기능 추가] 중복을 줄이고 예약 시점에 재호출하기 위해 순수 공격 실행 단락을 가독성 있게 메소드로 분리
    private void ExecuteNormalAttackLogic()
    {
        currentAction = ActionState.Attacking;
        attackDurationTimer = normalAttackDuration;

        // 공격이 끝난 시점부터 쿨타임이 돌도록 [공격 지속 시간 + 쿨타임]으로 설정
        attackCooldownTimer = normalAttackDuration + normalAttackCooldown;


        // [애니메이션 추가] 일반 공격 트리거 발동
        // (PlayerMovement에서 animator를 protected로 선언했으므로 접근 가능)
        if (animator != null)
        {
            animator.SetTrigger("isSmallAttack");
        }

        // 일반 공격 데미지 적용
        CheckAttackHit(normalAttackDamage);
    }

    private void ExecuteBigAttackLogic()
    {
        currentAction = ActionState.Attacking;
        attackDurationTimer = bigAttackDuration;

        // 공격이 끝난 시점부터 쿨타임이 돌도록 [공격 지속 시간 + 쿨타임]으로 설정
        attackCooldownTimer = bigAttackDuration + bigAttackCooldown;

        // [애니메이션 추가] 강공격 트리거 발동
        if (animator != null)
        {
            animator.SetTrigger("isBigAttack");
        }

        // 강공격 데미지 적용
        CheckAttackHit(bigAttackDamage);
    }

    private void CheckAttackHit(int damage)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackSize, 0f, enemyLayers);

        Debug.Log($"공격 범위 내 감지된 적 수: {hitEnemies.Length} | 적용 데미지: {damage}");

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<EnemyBase>(out var enemyComponent))
            {
                enemyComponent.TakeDamage(damage);
                Debug.Log($"{enemy.name}에게 {damage} 데미지를 입혔습니다!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackSize.x, attackSize.y, 1));
    }
}