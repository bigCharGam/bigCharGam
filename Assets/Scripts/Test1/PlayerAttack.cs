using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : PlayerMovement
{
    private enum ParryDirection { None, W, A, D }

    // 자동으로 발도 후 수행할 공격의 종류를 예약하기 위한 Enum
    private enum PendingAttackType { None, Normal, Big }

    [Header("Battle Status")]
    [SerializeField] private ParryDirection currentParryDirection = ParryDirection.None;

    // 발도 전환 타이머가 끝난 뒤 즉시 공격을 실행하기 위한 상태 변수
    private PendingAttackType reservedAttack = PendingAttackType.None;
    private bool wasTransitioningLastFrame = false;

    [Header("Attack Settings")]
    [SerializeField] private float normalAttackDashForce = 12f;
    [SerializeField] private float bigAttackDashForce = 25f;

    [Header("Attack Delay Settings (선딜 / 후딜)")]
    [SerializeField] private float lightPreDelay = 0.15f;   // 작은 공격 선딜
    [SerializeField] private float lightPostDelay = 0.2f;   // 작은 공격 후딜
    [SerializeField] private float heavyPreDelay = 0.35f;   // 큰 공격 선딜
    [SerializeField] private float heavyPostDelay = 0.4f;   // 큰 공격 후딜

    [Header("Attack Duration Settings (돌진 및 판정 지속시간)")]
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
    private float attackCooldownTimer; // 현재 남은 쿨타임을 계산할 타이머 변수
    private bool isAttackingRoutineActive = false; // 코루틴 중복 실행 방지용 플래그

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

        // 실시간으로 부모의 발도/납도 타이머 종료 시점을 감지하여 예약된 공격 실행
        HandleReservedAttack();

        // 공격 돌진 타이머 관리 (FixedUpdate에서의 속도 제어용)
        if (attackDurationTimer > 0)
        {
            attackDurationTimer -= Time.deltaTime;
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

        // 공격 액션 상태이고, 돌진 지속 타이머가 남아있을 때만 앞으로 전진 처리
        if (currentAction == ActionState.Attacking && attackDurationTimer > 0)
        {
            float currentDashForce = (attackDurationTimer > normalAttackDuration) ? bigAttackDashForce : normalAttackDashForce;
            rb.linearVelocity = new Vector2(lastDirectionX * currentDashForce, rb.linearVelocity.y);
        }
    }

    // 발도 완료 프레임을 실시간 감지하여 예약 공격을 분기 처리하는 유틸리티 메서드
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
                    StartCoroutine(AttackRoutine(isHeavy: false));
                }
                else if (reservedAttack == PendingAttackType.Big)
                {
                    StartCoroutine(AttackRoutine(isHeavy: true));
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
            // 현재 발도 모션 중이거나 납도 모션 중인 경우 패링 작동 제한
            if (IsBaldoTransitioning) return;

            // 공격, 대시, 스킬 중에는 패링 불가 (코루틴 실행 중 상태도 포함)
            if (currentAction == ActionState.Attacking ||
                currentAction == ActionState.Dashing ||
                currentAction == ActionState.SkillUsing ||
                isAttackingRoutineActive) return;

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

    // --- 공격 입력 함수 ---
    private void OnAttack()
    {
        // 대시 중이거나 이미 공격 중일 때, 그리고 쿨타임이 남아있을 때는 공격 불가
        if (currentAction == ActionState.Dashing || currentAction == ActionState.Attacking || isAttackingRoutineActive || attackCooldownTimer > 0) return;

        // 부모(PlayerMovement)에 구현된 발도 검증 로직 실행
        if (!EnsureBaldoState())
        {
            // 전환 타이머 작동으로 차단된 경우, 일반 공격을 예약 상자에 주입
            reservedAttack = PendingAttackType.Normal;
            return;
        }

        StartCoroutine(AttackRoutine(isHeavy: false));
    }

    private void OnBigAttack()
    {
        // 대시 중이거나 이미 공격 중일 때, 그리고 쿨타임이 남아있을 때는 공격 불가 조건 추가
        if (currentAction == ActionState.Dashing || currentAction == ActionState.Attacking || isAttackingRoutineActive || attackCooldownTimer > 0) return;

        // 부모(PlayerMovement)에 구현된 발도 검증 로직 실행
        if (!EnsureBaldoState())
        {
            // 전환 타이머 작동으로 차단된 경우, 강공격을 예약 상자에 주입
            reservedAttack = PendingAttackType.Big;
            return;
        }

        StartCoroutine(AttackRoutine(isHeavy: true));
    }

    // --- [핵심 기능] 선딜 -> 공격/돌진 -> 후딜 제어 코루틴 ---
    private IEnumerator AttackRoutine(bool isHeavy)
    {
        isAttackingRoutineActive = true;
        currentAction = ActionState.Attacking;

        // 1. 공격 스펙 설정 분기
        float preDelay = isHeavy ? heavyPreDelay : lightPreDelay;
        float postDelay = isHeavy ? heavyPostDelay : lightPostDelay;
        float duration = isHeavy ? bigAttackDuration : normalAttackDuration;
        float cooldown = isHeavy ? bigAttackCooldown : normalAttackCooldown;
        int damage = isHeavy ? bigAttackDamage : normalAttackDamage;
        string triggerName = isHeavy ? "isBigAttack" : "isSmallAttack";

        // 2. [선딜 시작] 선딜 모션 재생 트리거가 있다면 여기서 실행할 수 있습니다.
        if (animator != null)
        {
            // 공격을 준비하는 모션이나 선행 트리거가 필요할 때 호출
            animator.SetTrigger(triggerName);
        }

        // 선딜레이 시간만큼 대기
        yield return new WaitForSeconds(preDelay);

        // 3. [실제 공격 판정 및 돌진] 선딜이 끝난 직후 딜러와 대미지 연산 처리
        attackDurationTimer = duration; // FixedUpdate에서 대시 연산을 실행하도록 타이머 설정
        CheckAttackHit(damage);

        // 돌진 및 공격 판정 시간만큼 대기
        yield return new WaitForSeconds(duration);

        // 4. [후딜 시작] 공격 판정 프로세스가 끝나고 제자리 복귀 및 경직 대기
        yield return new WaitForSeconds(postDelay);

        // 5. [행동 가능 상태 복귀 및 쿨타임 적용]
        currentAction = isPressingAD ? ActionState.Locomotion : ActionState.None;
        attackCooldownTimer = cooldown; // 후딜이 끝난 시점부터 순수 쿨타임 작동 시작
        isAttackingRoutineActive = false;
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