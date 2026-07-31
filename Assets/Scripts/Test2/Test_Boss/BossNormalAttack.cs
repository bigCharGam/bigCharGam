using System.Collections;
using UnityEngine;

public class BossNormalAttack : MonoBehaviour
{
    [Header("Target & Layer Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask playerLayer;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;                       // 애니메이터 컴포넌트
    [SerializeField] private string normalAttackBoolName = "isNormalAttack"; // 애니메이터 Bool 파라미터 이름

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.5f;     // 일반 공격 사거리
    [SerializeField] private float attackCooldown = 2.0f;  // 공격 쿨타임
    [SerializeField] private float attackDamage = 15.0f;   // 기본 데미지

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackSpeedX = 10.0f;  // 수평 넉백
    [SerializeField] private float knockbackSpeedY = 4.0f;   // 수직 넉백
    [SerializeField] private float knockbackDuration = 0.2f; // 넉백 지속 시간

    private bool isAttacking = false;
    private bool isCooldown = false;

    private void Start()
    {
        // Animator 자동 검색 (인스펙터 미할당 시 본체/자식에서 찾기)
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        // 타깃 자동 검색 (Player 태그)
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("[BossNormalAttack] 'Player' 태그를 가진 오브젝트를 찾지 못했습니다.");
            }
        }
    }

    private void Update()
    {
        if (playerTransform == null || isAttacking || isCooldown) return;

        // 플레이어가 보스 전방에 있고, 사거리 내에 있는지 검사
        if (IsPlayerInFront() && IsPlayerInAttackRange())
        {
            StartCoroutine(NormalAttackRoutine());
        }
    }

    /// <summary>
    /// 플레이어가 보스의 전방에 있는지 확인
    /// </summary>
    private bool IsPlayerInFront()
    {
        float facingDirection = transform.localScale.x > 0 ? 1.0f : -1.0f;
        float directionToPlayer = playerTransform.position.x - transform.position.x;

        return (facingDirection * directionToPlayer) > 0;
    }

    /// <summary>
    /// 플레이어와의 거리 체크
    /// </summary>
    private bool IsPlayerInAttackRange()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        return distance <= attackRange;
    }

    /// <summary>
    /// 일반 공격 루틴
    /// </summary>
    private IEnumerator NormalAttackRoutine()
    {
        isAttacking = true;
        Debug.Log("<color=yellow>[보스 일반 공격] 전방 감지! 일반 공격 시작...</color>");

        // 1. 애니메이션 시작 (isNormalAttack = true)
        if (animator != null)
        {
            animator.SetBool(normalAttackBoolName, true);
        }

        // 2. 공격 선딜레이 (모션 타격 타이밍에 맞게 조절)
        yield return new WaitForSeconds(0.2f);

        // 3. 공격 시점 타격 판정
        Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);

        if (hitPlayer != null && IsPlayerInFront())
        {
            Debug.Log("<color=red>[보스 일반 공격] 타격 성공!</color>");

            // 데미지 전달
            if (hitPlayer.TryGetComponent<PlayerBattle>(out var playerBattle))
            {
                playerBattle.TakeDamage(attackDamage);
            }

            // 넉백 적용
            if (hitPlayer.TryGetComponent<Rigidbody2D>(out var playerRb))
            {
                StartCoroutine(ForceKnockbackRoutine(playerRb));
            }
        }
        else
        {
            Debug.Log("<color=gray>[보스 일반 공격] 타격 실패 (플레이어가 범위 탈출)</color>");
        }

        // 4. 공격 후딜레이
        yield return new WaitForSeconds(0.3f);

        // 5. 애니메이션 종료 (isNormalAttack = false) 및 공격 상태 해제
        if (animator != null)
        {
            animator.SetBool(normalAttackBoolName, false);
        }
        isAttacking = false;

        // 6. 쿨타임 시작
        isCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isCooldown = false;
    }

    /// <summary>
    /// 넉백 강제 유지 코루틴
    /// </summary>
    private IEnumerator ForceKnockbackRoutine(Rigidbody2D targetRb)
    {
        float timer = 0f;
        float knockbackDirectionX = transform.localScale.x > 0 ? 1.0f : -1.0f;

        while (timer < knockbackDuration)
        {
            if (targetRb == null) yield break;

            targetRb.linearVelocity = new Vector2(knockbackDirectionX * knockbackSpeedX, knockbackSpeedY);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}