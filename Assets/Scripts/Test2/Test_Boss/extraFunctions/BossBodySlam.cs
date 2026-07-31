using System.Collections;
using UnityEngine;

public class BossBodySlam : MonoBehaviour
{
    [Header("몸통박치기 세팅")]
    [SerializeField] private float knockbackSpeedX = 20f;   // 수평으로 날려버리는 속도
    [SerializeField] private float knockbackSpeedY = 10f;   // 위로 띄우는 속도
    [SerializeField] private float knockbackDuration = 0.35f; // 넉백 지속 시간
    [SerializeField] private float damage = 20f;            // 피해량

    [Header("타깃 레이어")]
    [SerializeField] private LayerMask playerLayer;

    // Trigger 충돌 감지 (보스 또는 타격 콜라이더가 Is Trigger가 켜져 있을 때)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsPlayerLayer(collision.gameObject.layer))
        {
            ApplyKnockback(collision.gameObject);
        }
    }

    // 일반 Collision 충돌 감지 (물리 콜라이더끼리 부딪힐 때)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsPlayerLayer(collision.gameObject.layer))
        {
            ApplyKnockback(collision.gameObject);
        }
    }

    private void ApplyKnockback(GameObject playerObj)
    {
        // 1. 플레이어 데미지 처리
        if (playerObj.TryGetComponent<PlayerBattle>(out var playerBattle))
        {
            playerBattle.TakeDamage(damage);
        }

        // 2. 플레이어의 Rigidbody2D 가져와 지속 넉백 코루틴 실행
        if (playerObj.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
        {
            // 보스 기준 플레이어의 방향 계산 (-1: 왼쪽, 1: 오른쪽)
            float direction = playerObj.transform.position.x >= transform.position.x ? 1f : -1f;

            // PlayerMovement.cs의 FixedUpdate 덮어쓰기를 뚫는 넉백 코루틴 실행
            StartCoroutine(ForceKnockbackRoutine(playerRb, direction));
        }
    }

    /// <summary>
    /// knockbackDuration 동안 매 물리 프레임(FixedUpdate)마다 X, Y축 속도를 대입하여
    /// 플레이어 이동 입력을 뚫고 밀쳐냅니다.
    /// </summary>
    private IEnumerator ForceKnockbackRoutine(Rigidbody2D targetRb, float direction)
    {
        float timer = 0f;

        while (timer < knockbackDuration)
        {
            if (targetRb == null) yield break;

            // 매 FixedUpdate 프레임마다 물리 속도를 강제로 대입
            targetRb.linearVelocity = new Vector2(direction * knockbackSpeedX, knockbackSpeedY);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private bool IsPlayerLayer(int layer)
    {
        return (playerLayer.value & (1 << layer)) != 0;
    }
}