using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossHorseRush : MonoBehaviour
{
    [Header("=== 감지 및 이동 세팅 ===")]
    [SerializeField] private Vector2 detectBoxSize = new Vector2(10.0f, 4.0f);
    [SerializeField] private Vector2 detectBoxOffset = Vector2.zero;
    [SerializeField] private float rushSpeed = 15.0f;
    [SerializeField] private float rushDuration = 1.0f;
    [SerializeField] private float rushCooldown = 3.0f;

    [Header("=== 타깃 설정 ===")]
    [SerializeField] private Transform playerTransform;

    private Rigidbody2D rb;
    private Collider2D bossCollider;
    private Collider2D playerCollider;

    private bool isRushing = false;
    private bool isCooldown = false;

    private int bossLayer;
    private int playerLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<Collider2D>();
        bossLayer = gameObject.layer;
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        if (playerTransform != null)
        {
            playerLayer = playerTransform.gameObject.layer;
            playerCollider = playerTransform.GetComponent<Collider2D>();
        }
    }

    private void Update()
    {
        if (playerTransform == null || isRushing || isCooldown) return;

        if (IsPlayerInDetectBox())
        {
            StartCoroutine(RushRoutine());
        }
    }

    private bool IsPlayerInDetectBox()
    {
        Vector2 center = (Vector2)transform.position + detectBoxOffset;
        Vector2 playerPos = playerTransform.position;

        float halfWidth = detectBoxSize.x * 0.5f;
        float halfHeight = detectBoxSize.y * 0.5f;

        bool inX = (playerPos.x >= center.x - halfWidth) && (playerPos.x <= center.x + halfWidth);
        bool inY = (playerPos.y >= center.y - halfHeight) && (playerPos.y <= center.y + halfHeight);

        return inX && inY;
    }

    private IEnumerator RushRoutine()
    {
        isRushing = true;

        // 1. 돌진 시작: 주인공 레이어와의 물리 충돌 무시 (바닥 지형 충돌은 유지)
        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, true);

        // 2. 방향 계산 및 Scale X 반전
        float directionX = (playerTransform.position.x - transform.position.x) > 0 ? 1.0f : -1.0f;
        Vector3 currentScale = transform.localScale;
        if ((directionX > 0 && currentScale.x < 0) || (directionX < 0 && currentScale.x > 0))
        {
            currentScale.x *= -1f;
            transform.localScale = currentScale;
        }

        // 3. 지정된 시간 동안 돌진
        float timer = 0f;
        while (timer < rushDuration)
        {
            rb.linearVelocity = new Vector2(directionX * rushSpeed, rb.linearVelocity.y);
            timer += Time.deltaTime;
            yield return null;
        }

        // 4. 돌진 종료 및 감속 정지
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 5. ★ [핵심] 보스와 주인공의 콜라이더가 여전히 겹쳐 있다면 겹침이 해제될 때까지 대기
        if (bossCollider != null && playerCollider != null)
        {
            while (bossCollider.BoundsIsOverlapped(playerCollider))
            {
                yield return null; // 겹쳐있는 동안 프레임 대기
            }
        }

        // 6. 완전히 벗어났을 때 주인공과의 충돌 재활성화
        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, false);

        isRushing = false;

        // 7. 쿨타임 대기
        isCooldown = true;
        yield return new WaitForSeconds(rushCooldown);
        isCooldown = false;
    }

    private void OnDisable()
    {
        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 center = (Vector2)transform.position + detectBoxOffset;
        Gizmos.DrawWireCube(center, detectBoxSize);
    }
}

// 겹침 판정을 돕는 확장 메서드
public static class ColliderExtensions
{
    public static bool BoundsIsOverlapped(this Collider2D col1, Collider2D col2)
    {
        return col1.bounds.Intersects(col2.bounds);
    }
}