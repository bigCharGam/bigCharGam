// 한국어 주석 유지를 위해 UTF-8로 작성됨
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerBattle : PlayerStats
{
    private Coroutine healCoroutine;

    [Header("Parry System (Collider Drag & Drop)")]
    [SerializeField] protected Collider2D outerCollider;     // 1번 바깥쪽 패리 감지 콜라이더 (드래그 앤 드롭)
    [SerializeField] protected Collider2D innerCollider;     // 2번 안쪽 본체 피격 콜라이더 (드래그 앤 드롭)
    [SerializeField] protected LayerMask enemyAttackLayer;     // ⭐ 인스펙터 드롭다운에서 EnemyAttack 레이어를 체크하여 감지합니다.

    private Vector2 firstContactPoint;  // 1번 영역 진입 최초 좌표
    private bool hasFirstContact = false;

    [Header("Parry Detection Tuning")]
    [Tooltip("바깥 콜라이더가 한두 프레임 겹침을 놓쳐도 진입 기록을 바로 지우지 않고 버텨주는 유예 시간(초)")]
    [SerializeField] private float contactLossGracePeriod = 0.12f;
    private float contactLossTimer = 0f;

    [Header("Battle Status")]
    // [수정] 아래 isParrying 변수는 직접 쓰기보다, PlayerMovement의 currentAction 상태를 참조하는 방식으로 일원화합니다.
    public bool isParrying => (GetComponent<PlayerMovement>() != null && GetComponent<PlayerMovement>().GetCurrentAction() == PlayerMovement.ActionState.Parrying);

    // 상속 구조를 통해 하위(Movement, Attack)에서 currentAction에 접근할 수 있으므로, 
    // 이를 활용하기 위해 PlayerMovement의 선언 정보를 가져와 검사합니다.
    protected PlayerMovement.ActionState currentActionState => GetComponent<PlayerMovement>() != null ? GetComponent<PlayerMovement>().CurrentAction : PlayerMovement.ActionState.None;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        // 실시간으로 플레이어가 패링 상태일 때만 바깥쪽 영역(1번)에 들어오는 적 공격을 감시합니다.
        if (isParrying && outerCollider != null)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;
            Collider2D[] results = new Collider2D[5];

            // 바깥쪽 콜라이더 영역 겹침 감지
            int count = outerCollider.Overlap(filter, results);

            bool anyAttackInOuter = false;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    // ⭐ [개선] 하드코딩된 태그나 이름 검사 대신, 인스펙터 드롭다운에서 설정한 레이어마스크 비트 연산으로 필터링합니다.
                    if (results[i] != null && ((1 << results[i].gameObject.layer) & enemyAttackLayer) != 0)
                    {
                        anyAttackInOuter = true;

                        // 최초 진입 시점의 접점 좌표만 딱 한 번 고정 잠금합니다.
                        if (!hasFirstContact)
                        {
                            firstContactPoint = results[i].bounds.ClosestPoint(outerCollider.transform.position);
                            hasFirstContact = true;
                            Debug.Log($"<color=cyan>[1번 바깥 영역 감지]</color> 진입 좌표: {firstContactPoint}");
                        }
                        break;
                    }
                }
            }

            if (anyAttackInOuter)
            {
                // 이번 프레임에 정상적으로 감지됐으면 유예 타이머 리셋
                contactLossTimer = 0f;
            }
            else if (hasFirstContact)
            {
                // ⭐ [완화] 한두 프레임 놓쳤다고 바로 기록을 지우지 않고, 유예 시간 동안은 firstContactPoint를 유지합니다.
                // (터널링/물리 흔들림으로 인한 순간적인 오버랩 유실이 패링 실패로 이어지는 것을 방지)
                contactLossTimer += Time.deltaTime;
                if (contactLossTimer >= contactLossGracePeriod)
                {
                    hasFirstContact = false;
                    contactLossTimer = 0f;
                }
            }
        }
        else
        {
            // 패링 상태 자체를 벗어난 경우는 즉시 초기화 (이건 흔들림이 아니라 명확한 상태 전환이므로 유예 불필요)
            hasFirstContact = false;
            contactLossTimer = 0f;
        }
    }

    public virtual void TakeDamage(float damage, EnemyBase attacker = null, float parryReduction = 0f, float parryPerfectReduction = 1f, bool isReflectable = false)
    {
        var movement = GetComponent<PlayerMovement>();
        var attackScript = GetComponent<PlayerAttack>();

        // ⭐ [임시 추가] 아래 조건(궤적/방향) 판정 로직은 그대로 두되,
        // 패링 판정이 간헐적으로 안 되는 문제 때문에 패링 자세(Parrying)만 취하고 있으면
        // 조건 검사 없이 무조건 패리 성공(무적) 처리합니다. (임시 조치)
        if (movement != null && movement.GetCurrentAction() == PlayerMovement.ActionState.Parrying)
        {
            Debug.Log("<color=purple>[⚔️ PARRY - 임시 무조건 성공]</color> 조건 검사 없이 패리 처리되었습니다!");
            hasFirstContact = false;
            contactLossTimer = 0f;
            return;
        }

        // 1. 플레이어가 현재 패링 자세를 취하고 있고, 바깥 영역 진입 좌표가 기록되어 있을 때
        if (movement != null && movement.GetCurrentAction() == PlayerMovement.ActionState.Parrying && attackScript != null && hasFirstContact && innerCollider != null)
        {
            // 현재 안쪽 영역(2번)에 겹쳐있는 적 공격 콜라이더의 위치를 구합니다.
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;
            Collider2D[] results = new Collider2D[5];
            int count = innerCollider.Overlap(filter, results);

            Vector2 secondContactPoint = (Vector2)innerCollider.transform.position;

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    // ⭐ [개선] 안쪽 피격 시점 조건문도 드롭다운 레이어마스크 검사로 통일합니다.
                    if (results[i] != null && ((1 << results[i].gameObject.layer) & enemyAttackLayer) != 0)
                    {
                        secondContactPoint = results[i].bounds.ClosestPoint(innerCollider.transform.position);
                        break;
                    }
                }
            }

            // 궤적 벡터 계산: 1번 바깥 진입점 -> 2번 안쪽 본체 도달점
            Vector2 attackVector = secondContactPoint - firstContactPoint;

            bool isParrySuccess = false;
            PlayerAttack.ParryDirection userDir = attackScript.GetCurrentParryDirection();

            // Y축 변화량이 아래쪽으로 매우 크면 위에서 내려찍는 공격으로 간주
            if (attackVector.y < -0.7f && Mathf.Abs(attackVector.y) > Mathf.Abs(attackVector.x))
            {
                if (userDir == PlayerAttack.ParryDirection.W) // 상단 방어
                {
                    isParrySuccess = true;
                    Debug.Log("<color=purple>[⚔️ PERFECT PARRY - TOP]</color> 플레이어가 위에서 떨어지는 공격 궤적을 완벽히 쳐냈습니다!");
                }
            }
            else
            {
                // X축 변화량을 통해 좌/우 진입 궤적 판별
                if (attackVector.x > 0f) // 왼쪽에서 들어와 오른쪽으로 진행하는 궤적
                {
                    if (userDir == PlayerAttack.ParryDirection.A) // 좌측 방어
                    {
                        isParrySuccess = true;
                        Debug.Log("<color=purple>[⚔️ PERFECT PARRY - LEFT]</color> 플레이어가 왼쪽에서 날아오는 공격 궤적을 방어했습니다!");
                    }
                }
                else if (attackVector.x < 0f) // 오른쪽에서 들어와 왼쪽으로 진행하는 궤적
                {
                    if (userDir == PlayerAttack.ParryDirection.D) // 우측 방어
                    {
                        isParrySuccess = true;
                        Debug.Log("<color=purple>[⚔️ PERFECT PARRY - RIGHT]</color> 플레이어가 오른쪽에서 날아오는 공격 궤적을 방어했습니다!");
                    }
                }
            }

            // 패리 처리 후 진입 정보 리셋
            hasFirstContact = false;
            contactLossTimer = 0f;

            if (isParrySuccess)
            {
                // [애니메이션 연동] 패리 성공 애니메이션 트리거
                Animator anim = GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger("isParrySuccess");
                }

                // ⭐ [추가 권장] 안쪽 영역에 겹쳐서 타격을 준 적 투사체/공격체를 정리하는 트리거 신호를 보냅니다.
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (results[i] != null && ((1 << results[i].gameObject.layer) & enemyAttackLayer) != 0)
                        {
                            results[i].gameObject.SendMessage("OnParried", SendMessageOptions.DontRequireReceiver);
                            break;
                        }
                    }
                }
                return;
            }
            else
            {
                Debug.Log("<color=yellow>[⚠️ PARRY FAILED]</color> 패링 자세는 취했으나 궤적이 어긋나 방어막이 뚫렸습니다!");
            }
        }

        // 2. 패리 상태가 아니거나 패리에 실패했을 때만 실제 대미지 적용
        // 포션 회복 중이었다면 그 즉시 중단한다 (그 시점까지 회복된 체력에서 바로 데미지 적용)
        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
            healCoroutine = null;
        }

        currentHealth -= damage;
        UIManager.instance.UpdatePlayerHP(currentHealth, maxHealth);
        //Debug.Log("Player 남은 체력 : " + currentHealth);

        hasFirstContact = false;
        contactLossTimer = 0f;

        Animator animator = GetComponent<Animator>();

        if (currentHealth <= 0)
        {
            Debug.Log("Player 사망!");
            if (animator != null)
            {
                animator.SetTrigger("isDead");
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetTrigger("isHit");
            }
        }
    }

    // 포션 등에 의한 회복을 duration에 걸쳐 실제 currentHealth 자체에 서서히 반영한다.
    // 회복 도중 TakeDamage가 들어오면 코루틴이 즉시 중단되어, 그 시점까지 회복된 값에서 바로 데미지가 적용된다.
    public void StartPotionHeal(float healAmount, float duration)
    {
        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
            healCoroutine = null;
        }

        float clampedHeal = Mathf.Min(healAmount, maxHealth - currentHealth);
        if (clampedHeal <= 0f || duration <= 0f)
        {
            currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
            UIManager.instance.UpdatePlayerHP(currentHealth, maxHealth);
            return;
        }

        UIManager.instance.BeginPotionHeal(currentHealth, maxHealth, clampedHeal, duration);
        healCoroutine = StartCoroutine(HealOverTime(clampedHeal, duration));
    }

    private IEnumerator HealOverTime(float healAmount, float duration)
    {
        float healed = 0f;
        float healPerSecond = healAmount / duration;

        while (healed < healAmount)
        {
            float step = Mathf.Min(healPerSecond * Time.deltaTime, healAmount - healed);
            healed += step;
            currentHealth = Mathf.Min(currentHealth + step, maxHealth);
            UIManager.instance.UpdatePlayerHPHealTick(currentHealth, maxHealth);
            yield return null;
        }

        UIManager.instance.UpdatePlayerHP(currentHealth, maxHealth);
        healCoroutine = null;
    }
}