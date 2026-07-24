using System.Collections;
using UnityEngine;

public class BossRearAttack : MonoBehaviour
{
    [Header("Target & Layer Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack Settings")]
    [SerializeField] private float rearDetectRange = 4.0f;  // 후방 감지 거리
    [SerializeField] private float attackCooldown = 5.0f;   // 스킬 쿨타임
    [SerializeField] private float attackDamage = 35.0f;    // 강력한 데미지

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackSpeedX = 22.0f; // 수평 넉백 속도
    [SerializeField] private float knockbackSpeedY = 8.0f;  // 수직(공중) 넉백 속도
    [SerializeField] private float knockbackDuration = 0.35f; // 넉백 지속 시간

    private bool isAttacking = false;
    private bool isCooldown = false;

    private void Start()
    {
        // 타깃 자동 검색 (Player 태그 활용)
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("[BossRearAttack] 'Player' 태그를 가진 오브젝트를 찾지 못했습니다.");
            }
        }
    }

    private void Update()
    {
        if (playerTransform == null || isAttacking || isCooldown) return;

        // 플레이어가 보스 뒤에 있고, 사거리 내에 있는지 검사
        if (IsPlayerBehind() && IsPlayerInRearRange())
        {
            StartCoroutine(RearAttackRoutine());
        }
    }

    /// <summary>
    /// 플레이어가 보스의 뒤쪽에 있는지 확인 (transform.localScale.x > 0 기준 우측)
    /// </summary>
    private bool IsPlayerBehind()
    {
        float facingDirection = transform.localScale.x > 0 ? 1.0f : -1.0f;
        float directionToPlayer = playerTransform.position.x - transform.position.x;

        return (facingDirection * directionToPlayer) < 0;
    }

    /// <summary>
    /// 플레이어와의 거리 체크
    /// </summary>
    private bool IsPlayerInRearRange()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        return distance <= rearDetectRange;
    }

    /// <summary>
    /// 후방 공격 및 넉백 코루틴
    /// </summary>
    private IEnumerator RearAttackRoutine()
    {
        isAttacking = true;
        Debug.Log("<color=yellow>[보스 후방 공격] 플레이어 후방 감지! 선딜레이 시작...</color>");

        // 1. 공격 선딜레이 (필요시 animator.SetTrigger("RearAttack") 등 연동)
        yield return new WaitForSeconds(0.3f);

        // 2. 공격 시점 타격 판정
        Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, rearDetectRange, playerLayer);

        if (hitPlayer != null && IsPlayerBehind())
        {
            Debug.Log("<color=red>[보스 후방 공격] 타격 성공! 데미지 및 강력한 넉백 적용</color>");

            // 데미지 전달 (PlayerBattle의 TakeDamage 호출)
            if (hitPlayer.TryGetComponent<PlayerBattle>(out var playerBattle))
            {
                playerBattle.TakeDamage(attackDamage);
            }

            // PlayerMovement 스크립트를 건드리지 않고 보스 스크립트 내에서 넉백을 강제 유지시킴
            if (hitPlayer.TryGetComponent<Rigidbody2D>(out var playerRb))
            {
                StartCoroutine(ForceKnockbackRoutine(playerRb));
            }
        }
        else
        {
            Debug.Log("<color=gray>[보스 후방 공격] 타격 실패 (플레이어가 범위 탈출)</color>");
        }

        // 3. 공격 후딜레이
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;

        // 4. 쿨타임 시작
        isCooldown = true;
        Debug.Log($"[보스 후방 공격] 쿨타임 시작 ({attackCooldown}초)");
        yield return new WaitForSeconds(attackCooldown);
        isCooldown = false;
        Debug.Log("<color=green>[보스 후방 공격] 쿨타임 종료, 재감지 가능</color>");
    }

    /// <summary>
    /// PlayerMovement의 FixedUpdate 덮어쓰기를 무력화하기 위해 
    /// knockbackDuration 동안 지속적으로 속도를 대입하는 코루틴
    /// </summary>
    private IEnumerator ForceKnockbackRoutine(Rigidbody2D targetRb)
    {
        float timer = 0f;

        // 보스 바라보는 방향의 반대(후방)로 밀쳐내기
        float knockbackDirectionX = transform.localScale.x > 0 ? -1.0f : 1.0f;

        while (timer < knockbackDuration)
        {
            if (targetRb == null) yield break;

            // 매 FixedUpdate 프레임마다 플레이어의 FixedUpdate 덮어쓰기보다 강력하게 속도를 고정대입
            targetRb.linearVelocity = new Vector2(knockbackDirectionX * knockbackSpeedX, knockbackSpeedY);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 씬 뷰 범위 시각화
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, rearDetectRange);
    }
}