using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 체력 감지 및 사망 처리 스크립트
/// </summary>
public class PlayerDeathHandler : MonoBehaviour
{
    [Header("컴포넌트 참조")]
    [SerializeField] private PlayerBattle playerBattle;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Animator animator;

    [Header("사망 애니메이션 트리거 명")]
    [SerializeField] private string deathAnimationTrigger = "Die";

    private bool isDead = false;

    // ⭐ [추가] GameOver.cs가 폴링해서 확인할 수 있도록 공개
    public bool IsDead => isDead;

    private void Awake()
    {
        // 컴포넌트 자동 탐색 및 할당
        if (playerBattle == null) playerBattle = GetComponent<PlayerBattle>();
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // 이미 사망 처리되었거나 체력 데이터가 없으면 패스
        if (isDead || playerBattle == null) return;

        // PlayerBattle의 currentHealth가 0 이하가 되는 순간 사망 후처리 실행
        if (playerBattle.currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    /// <summary>
    /// 플레이어 사망 시 키 입력 및 상태 차단
    /// (애니메이션 트리거는 PlayerBattle.TakeDamage()에서 이미 "isDead"로 처리되므로 여기서는 중복 호출하지 않음)
    /// </summary>
    private void HandleDeath()
    {
        isDead = true;
        Debug.Log("<color=red>[💀 PLAYER DIED]</color> 플레이어의 체력이 0이 되어 모든 조작이 차단됩니다.");

        // 1. 유니티 Input System의 모든 키 입력 수신 비활성화
        if (playerInput != null)
        {
            //playerInput.DeactivateInput();
        }

        // 2. 물리 이동 및 잔여 속도 완전히 정지 (Rigidbody2D 멈춤)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            //rb.linearVelocity = Vector2.zero;
        }

        // 참고: animator.SetTrigger("isDead")는 PlayerBattle.TakeDamage()에서 이미 실행됩니다.
        // 여기서 deathAnimationTrigger를 또 쏘면 같은 죽음에 트리거가 두 번 걸리니 주의하세요.
    }

    /// <summary>
    /// (필요시) 부활 또는 리스타트 시 입력을 다시 켜는 메서드
    /// </summary>
    public void RevivePlayer()
    {
        isDead = false;

        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }

        Debug.Log("<color=green>[✨ PLAYER REVIVED]</color> 플레이어가 부활하여 키 입력을 다시 받습니다.");
    }
}