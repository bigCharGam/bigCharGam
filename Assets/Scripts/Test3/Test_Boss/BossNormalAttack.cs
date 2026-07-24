// 한국어 주석 유지를 위해 UTF-8로 작성됨
using System.Collections;
using UnityEngine;

/// <summary>
/// 보스 오브젝트에 부착하여 플레이어를 자동 감지하고 일반 공격(평타)을 수행하는 스크립트입니다.
/// </summary>
public class BossNormalAttack : MonoBehaviour
{
    [Header("=== Damage Settings ===")]
    [Tooltip("평타 타격 시 플레이어에게 입힐 데미지 수치입니다.")]
    [SerializeField] private float attackDamage = 15f;

    [Header("=== Timing Settings (초 단위) ===")]
    [Tooltip("공격 모션 시작 후 실제 타격 판정이 발동할 때까지의 선딜레이입니다.")]
    [SerializeField] private float preAttackDelay = 0.35f;

    [Tooltip("타격 판정 발동 후 공격 모션이 완전히 끝날 때까지의 후딜레이입니다.")]
    [SerializeField] private float postAttackDelay = 0.4f;

    [Tooltip("공격 완료 후 다음 공격을 실행하기까지의 쿨타임입니다.")]
    [SerializeField] private float attackCooldown = 2.0f;

    [Header("=== Auto Detect Settings (자동 감지) ===")]
    [Tooltip("자동 공격을 활성화할지 여부입니다.")]
    [SerializeField] private bool autoAttack = true;

    [Tooltip("플레이어를 감지할 사거리(반경)입니다. 이 범위 안에 들어오면 공격을 시도합니다.")]
    [SerializeField] private float detectRadius = 3.0f;

    [Tooltip("플레이어 위치를 추적할 Transform입니다. 미지정 시 Player 태그로 자동 탐색합니다.")]
    [SerializeField] private Transform playerTransform;

    [Header("=== Hitbox Settings (인스펙터 수정 가능) ===")]
    [Tooltip("공격 타격 위치의 피벗 트랜스폼입니다. 미지정 시 보스 위치를 기준으로 설정된 오프셋이 적용됩니다.")]
    [SerializeField] private Transform attackPoint;

    [Tooltip("attackPoint가 미지정된 경우 사용될 기준 위치 오프셋입니다.")]
    [SerializeField] private Vector2 attackPointOffset = new Vector2(1.5f, 0f);

    [Tooltip("타격 박스의 가로/세로 크기입니다.")]
    [SerializeField] private Vector2 attackBoxSize = new Vector2(2.5f, 2.0f);

    [Header("=== Target & Layer Settings ===")]
    [Tooltip("플레이어가 포함된 레이어 마스크를 선택해 주세요 (예: Player).")]
    [SerializeField] private LayerMask playerLayer;

    // 내부 상태 변수
    private bool isAttacking = false;
    private bool isCooldown = false;
    private Animator animator;

    // 외부 스크립트에서 상태 확인용 프로퍼티
    public bool IsAttacking => isAttacking;
    public bool IsCooldown => isCooldown;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // 플레이어 Transform 자동 탐색
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        // 자동 공격 옵션이 꺼져 있거나 이미 공격/쿨타임 중이면 스킵
        if (!autoAttack || isAttacking || isCooldown) return;

        // 플레이어 실시간 감지 및 자동 공격
        if (IsPlayerInDetectRange())
        {
            TriggerAttack();
        }
    }

    /// <summary>
    /// 플레이어가 보스의 감지 범위(detectRadius) 내에 들어왔는지 검사합니다.
    /// </summary>
    private bool IsPlayerInDetectRange()
    {
        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            return distance <= detectRadius;
        }

        // Transform을 못 찾았을 경우 OverlapCircle로 감지
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectRadius, playerLayer);
        return hit != null;
    }

    /// <summary>
    /// 외부 스크립트 또는 자동 감지에 의해 평타 공격을 실행하는 함수입니다.
    /// </summary>
    public void TriggerAttack()
    {
        if (isAttacking || isCooldown) return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // 1. [공격 시작] 애니메이터 트리거 작동
        if (animator != null)
        {
            animator.SetTrigger("isNormalAttack");
        }

        // 2. [선딜레이] 공격 모션 준비 대기
        yield return new WaitForSeconds(preAttackDelay);

        // 3. [타격 판정] 히트박스 범위 내 플레이어 탐색 및 데미지 적용
        PerformAttackHitCheck();

        // 4. [후딜레이] 모션 정지 대기
        yield return new WaitForSeconds(postAttackDelay);

        isAttacking = false;

        // 5. [쿨타임]
        isCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isCooldown = false;
    }

    /// <summary>
    /// 지정된 히트박스 범위 내의 플레이어를 감지하여 PlayerBattle.TakeDamage()를 호출합니다.
    /// </summary>
    private void PerformAttackHitCheck()
    {
        Vector2 currentAttackPos = GetAttackPosition();

        // 타격 박스 범위 오버랩 검사
        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(currentAttackPos, attackBoxSize, 0f, playerLayer);

        foreach (Collider2D playerCol in hitPlayers)
        {
            // 플레이어에 부착된 PlayerBattle 컴포넌트 탐색 및 데미지 전달
            if (playerCol.TryGetComponent<PlayerBattle>(out var playerBattle))
            {
                playerBattle.TakeDamage(attackDamage);
                Debug.Log($"<color=red>[보스 평타 타격]</color> {playerCol.name}에게 {attackDamage} 데미지 적용!");
            }
            else
            {
                // 부모 오브젝트에 PlayerBattle이 붙어있는 경우 대응
                PlayerBattle parentBattle = playerCol.GetComponentInParent<PlayerBattle>();
                if (parentBattle != null)
                {
                    parentBattle.TakeDamage(attackDamage);
                    Debug.Log($"<color=red>[보스 평타 타격]</color> {playerCol.name}(부모)에게 {attackDamage} 데미지 적용!");
                }
            }
        }
    }

    /// <summary>
    /// 보스의 바라보는 방향(Scale X)에 연동하여 타격 박스의 실시간 좌표를 계산합니다.
    /// </summary>
    private Vector2 GetAttackPosition()
    {
        if (attackPoint != null)
        {
            return attackPoint.position;
        }

        // attackPoint가 설정되지 않은 경우 보스의 Scale X 축 반전 상태를 고려하여 위치 오프셋 조절
        float directionX = transform.localScale.x >= 0 ? 1f : -1f;
        Vector2 offset = new Vector2(attackPointOffset.x * directionX, attackPointOffset.y);
        return (Vector2)transform.position + offset;
    }

    // 씬 뷰(Scene View)에서 감지 범위(노란색 원)와 타격 박스(빨간색 사각형)를 표시합니다.
    private void OnDrawGizmosSelected()
    {
        // 1. 플레이어 감지 범위 (노란색 원)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        // 2. 평타 타격 박스 (빨간색 사각형)
        Gizmos.color = Color.red;
        Vector2 drawPos = GetAttackPosition();
        Gizmos.DrawWireCube(drawPos, new Vector3(attackBoxSize.x, attackBoxSize.y, 1f));
    }
}