using UnityEngine;

/// <summary>
/// 플레이어에게 부착되어 보스와 콜라이더가 겹쳐있을 때 초당 데미지(DPS)를 지속해서 입히는 컴포넌트입니다.
/// 패링 자세 중일 때는 피해를 받지 않습니다.
/// </summary>
public class BossOverlapDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("초당 플레이어에게 입힐 지속 데미지 수치입니다.")]
    [SerializeField] private float damagePerSecond = 10f;

    [Header("Layer / Target Settings")]
    [Tooltip("보스가 속한 레이어 마스크를 인스펙터에서 선택해 주세요 (예: BossH).")]
    [SerializeField] private LayerMask bossLayer;

    private PlayerBattle playerBattle;
    private Collider2D playerCollider;

    // 💡 최적화: 매 프레임 new 생성 방지를 위한 재사용 변수 및 배열
    private ContactFilter2D contactFilter;
    private readonly Collider2D[] overlapResults = new Collider2D[5];

    private void Awake()
    {
        playerBattle = GetComponent<PlayerBattle>();
        playerCollider = GetComponent<Collider2D>();

        // ContactFilter 미리 초기화 (매 프레임 new 할당 방지)
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(bossLayer);
        contactFilter.useTriggers = true; // 필요에 따라 Trigger 감지 여부 조절
    }

    private void Update()
    {
        if (playerBattle == null || playerCollider == null) return;

        // 플레이어가 현재 패링 자세라면 비비기 데미지 무시
        if (playerBattle.isParrying) return;

        // 미리 구성된 필터와 배열을 재사용하여 Overlap 수행 (GC 발생 최소화)
        int count = playerCollider.Overlap(contactFilter, overlapResults);

        if (count > 0)
        {
            float damageThisFrame = damagePerSecond * Time.deltaTime;
            playerBattle.TakeDamage(damageThisFrame);
        }
    }
}