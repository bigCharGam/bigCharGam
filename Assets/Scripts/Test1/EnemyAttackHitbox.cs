using UnityEngine;
using System.Collections;

public class EnemyAttackHitbox : MonoBehaviour
{
    [HideInInspector]
    public float damage;
    [Tooltip("일반 패리 시 데미지 감소 비율 (0=패리불가, 0.5=절반차단, 1=완전차단)")]
    public float parryReduction = 0f;
    [Tooltip("퍼펙트 패리 시 데미지 감소 비율 (기본 1=완전차단)")]
    public float parryPerfectReduction = 1f;
    [Tooltip("퍼펙트 패리 시 적에게 반사경직(reflect) 모션을 줄지 여부")]
    public bool isReflectable = false;

    private EnemyBase ownerEnemy;

    private void Awake()
    {
        ownerEnemy = GetComponentInParent<EnemyBase>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            col.GetComponent<Tester>()?.TakeDamage(
                damage, ownerEnemy, parryReduction, parryPerfectReduction, isReflectable);
        }
    }
}
