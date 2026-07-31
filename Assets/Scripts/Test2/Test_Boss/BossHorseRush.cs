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

    [Header("=== 애니메이션 설정 ===")]
    [SerializeField] private Animator anim; // 인스펙터에서 직접 할당하거나 Awake에서 가져옵니다.
    [SerializeField] private string rushBoolName = "IsRushing"; // 애니메이터 파라미터 이름

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

        // Animator가 할당되지 않았다면 자식이나 본인에게서 컴포넌트를 찾습니다.
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerCollider = playerObj.GetComponent<Collider2D>();
                playerLayer = playerObj.layer;
            }
        }
    }

    private void Update()
    {
        if (isRushing || isCooldown || playerTransform == null) return;

        // 플레이어가 감지 범위 내에 들어왔는지 체크
        if (IsPlayerInDetectBox())
        {
            StartCoroutine(RushRoutine());
        }
    }

    private IEnumerator RushRoutine()
    {
        isRushing = true;

        // 1. 돌진 방향 설정
        float directionX = (playerTransform.position.x > transform.position.x) ? 1.0f : -1.0f;

        // 스프라이트 좌우 반전 처리 (필요시)
        if (directionX > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // 2. 플레이어와 충돌 무시 (통과 처리)
        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, true);

        // ★ [핵심] 돌진 애니메이션 재생 시작
        if (anim != null)
        {
            anim.SetBool(rushBoolName, true);
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

        // ★ [핵심] 돌진 애니메이션 종료 (Idle 상태 등으로 복구)
        if (anim != null)
        {
            anim.SetBool(rushBoolName, false);
        }

        // 5. 보스와 주인공의 콜라이더가 여전히 겹쳐 있다면 겹침이 해제될 때까지 대기
        if (bossCollider != null && playerCollider != null)
        {
            while (bossCollider.bounds.Intersects(playerCollider.bounds))
            {
                yield return null;
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

    private bool IsPlayerInDetectBox()
    {
        Vector2 boxCenter = (Vector2)transform.position + detectBoxOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, detectBoxSize, 0f);
        foreach (var hit in hits)
        {
            if (hit.transform == playerTransform) return true;
        }
        return false;
    }

    private void OnDisable()
    {
        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, false);
        if (anim != null)
        {
            anim.SetBool(rushBoolName, false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 boxCenter = (Vector2)transform.position + detectBoxOffset;
        Gizmos.DrawWireCube(boxCenter, detectBoxSize);
    }
}