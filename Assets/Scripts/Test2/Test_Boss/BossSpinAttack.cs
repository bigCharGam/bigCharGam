using System.Collections;
using UnityEngine;

/// <summary>
/// 보스가 주변을 원 모양으로 창을 휘둘러 그 자취 내 타깃에게 데미지와 넉백을 가하는 광역 스크립트
/// </summary>
public class BossSpinAttack : MonoBehaviour
{
    [Header("🎯 감지 및 타격 설정")]
    [Tooltip("원형 판정의 중심점 (비워둘 경우 보스 본체의 위치 사용)")]
    [SerializeField] private Transform attackCenter;

    [Tooltip("회전 창 공격의 원 반지름")]
    [SerializeField] private float attackRadius = 3.5f;

    [Tooltip("플레이어 레이어")]
    [SerializeField] private LayerMask playerLayer;

    [Header("⚔️ 공격 및 넉백 수치")]
    [Tooltip("회전 베기 데미지")]
    [SerializeField] private float attackDamage = 25f;

    [Tooltip("플레이어를 밖으로 밀쳐낼 넉백 힘 (X, Y)")]
    [SerializeField] private float knockbackSpeedX = 20f;
    [SerializeField] private float knockbackSpeedY = 8f;

    [Tooltip("넉백 물리 속도가 유지될 지속 시간")]
    [SerializeField] private float knockbackDuration = 0.35f;

    [Header("⏱️ 패턴 타이머")]
    [Tooltip("스킬 재사용 쿨타임 (초)")]
    [SerializeField] private float attackCooldown = 6.0f;

    private bool isAttacking = false;
    private bool isCooldown = false;

    private void Awake()
    {
        // attackCenter가 지정되지 않았다면 자기 자신(보스)을 중심으로 설정
        if (attackCenter == null)
        {
            attackCenter = transform;
        }
    }

    /// <summary>
    /// 외부 AI나 패턴 컨트롤러에서 회전 공격을 발동시킬 때 호출하는 메서드
    /// </summary>
    public void TriggerSpinAttack()
    {
        if (isAttacking || isCooldown) return;

        StartCoroutine(SpinAttackRoutine());
    }

    private IEnumerator SpinAttackRoutine()
    {
        isAttacking = true;
        isCooldown = true;

        Debug.Log("<color=yellow>[⚔️ BOSS] 보스가 회전 창 공격을 시작합니다!</color>");

        // 1. 공격 중심점 주변의 원 영역(OverlapCircleAll) 내 플레이어 탐색
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackCenter.position, attackRadius, playerLayer);

        foreach (Collider2D playerCollider in hitPlayers)
        {
            // 데미지 처리 (PlayerBattle 또는 EnemyBase 구조 호환)
            if (playerCollider.TryGetComponent<PlayerBattle>(out var playerBattle))
            {
                playerBattle.TakeDamage(attackDamage);
            }

            // 넉백 처리 (PlayerMovement 스크립트를 건드리지 않는 코루틴 강제 대입 물리 적용)
            Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                StartCoroutine(ForceKnockbackRoutine(playerRb, playerCollider.transform.position));
            }
        }

        // 공격 액션 유지시간 (애니메이션 길이에 맞추어 조정 가능)
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;

        // 쿨타임 대기
        yield return new WaitForSeconds(attackCooldown);
        isCooldown = false;
        Debug.Log("<color=green>[✅ BOSS] 회전 창 공격 쿨타임이 완료되었습니다.</color>");
    }

    /// <summary>
    /// PlayerMovement의 FixedUpdate 덮어쓰기 현상을 우회하여 매 물리 프레임 넉백을 강제 지정하는 코루틴
    /// </summary>
    private IEnumerator ForceKnockbackRoutine(Rigidbody2D playerRb, Vector3 playerPos)
    {
        float timer = 0f;

        // 보스 중심점으로부터 플레이어가 어느 방향(좌/우)에 있는지 계산
        float directionX = (playerPos.x >= attackCenter.position.x) ? 1f : -1f;

        while (timer < knockbackDuration)
        {
            if (playerRb == null) yield break;

            // PlayerMovement.cs의 이동 입력을 무시하고 보스 바깥쪽으로 강력하게 밀쳐냄
            playerRb.linearVelocity = new Vector2(directionX * knockbackSpeedX, knockbackSpeedY);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    // 유니티 씬(Scene) 뷰에서 회전 창 공격의 원형 범위를 노란색 선으로 시각화
    private void OnDrawGizmosSelected()
    {
        Vector3 center = (attackCenter != null) ? attackCenter.position : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, attackRadius);
    }
}