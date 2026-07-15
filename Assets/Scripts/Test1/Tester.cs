using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tester : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Attack Timing")]
    [SerializeField] private float windupDuration = 0.5f;
    [SerializeField] private float activeDuration = 0.2f;
    [SerializeField] private float recoveryDuration = 0.5f;

    [Header("Attack Hitbox")]
    [Tooltip("캐릭터 자식으로 만들어둔 히트박스용 Collider2D (예: PolygonCollider2D, IsTrigger 체크). 평소엔 꺼두고 지속 구간에만 켜짐.")]
    [SerializeField] private Collider2D attackHitbox;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private int damage = 10;

    [Header("Trail")]
    [Tooltip("검기를 표시할 TrailRenderer. 칼날 끝 자식 오브젝트에 붙여두면 됨.")]
    [SerializeField] private TrailRenderer attackTrail;

    [Header("Gizmo Colors")]
    [SerializeField] private Color windupColor = new Color(1f, 1f, 0f, 0.4f);
    [SerializeField] private Color activeColor = new Color(1f, 0f, 0f, 0.6f);
    [SerializeField] private Color recoveryColor = new Color(0f, 0.5f, 1f, 0.4f);

    [Header("Parry")]
    [Tooltip("패링 시작 후 이 시간(초) 이내에 피격되면 퍼펙트 패리 판정")]
    [SerializeField] private float perfectParryWindow = 0.1f;

    private readonly List<Collider2D> hitResults = new List<Collider2D>();

    private InputAction attackAction;
    private bool isAttacking;

    private enum AttackPhase { None, Windup, Active, Recovery }
    private AttackPhase currentPhase = AttackPhase.None;

    private bool isParrying = false;
    private float parryStartTime = -999f;

    public float currentHealth = 200f;

    private void Awake()
    {
        if (attackHitbox != null)
            attackHitbox.enabled = false;

        if (attackTrail != null)
            attackTrail.emitting = false;

        if (inputActions != null)
            attackAction = inputActions.FindActionMap("test").FindAction("Attack");
    }

    private void OnEnable()
    {
        if (attackAction == null) return;

        attackAction.performed += OnAttackPerformed;
        inputActions.Enable();
    }

    private void OnDisable()
    {
        if (attackAction == null) return;

        attackAction.performed -= OnAttackPerformed;
        inputActions.Disable();
    }

    // PlayerInput(Send Messages) — 패리
    private void OnAParry(InputValue value)
    {
        if (value.isPressed)
        {
            isParrying = true;
            parryStartTime = Time.time;
        }
        else
        {
            isParrying = false;
        }
    }

    // PlayerInput(Send Messages) 사용 시에도 동작
    private void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;
        TryStartAttack();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        TryStartAttack();
    }

    private void TryStartAttack()
    {
        if (isAttacking) return;
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        currentPhase = AttackPhase.Windup;

        yield return new WaitForSeconds(windupDuration);

        currentPhase = AttackPhase.Active;

        if (attackHitbox != null)
            attackHitbox.enabled = true;

        if (attackTrail != null)
            attackTrail.emitting = true;

        HitClosestEnemyInRange();

        yield return new WaitForSeconds(activeDuration);

        currentPhase = AttackPhase.Recovery;

        if (attackHitbox != null)
            attackHitbox.enabled = false;

        if (attackTrail != null)
            attackTrail.emitting = false;

        yield return new WaitForSeconds(recoveryDuration);

        currentPhase = AttackPhase.None;
        isAttacking = false;
    }

    private void HitClosestEnemyInRange()
    {
        if (attackHitbox == null) return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayers);
        filter.useTriggers = true;

        hitResults.Clear();
        Physics2D.OverlapCollider(attackHitbox, filter, hitResults);

        EnemyBase closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hitResults)
        {
            if (!hit.TryGetComponent<EnemyBase>(out EnemyBase enemy)) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        if (closest == null) return;

        closest.TakeDamage(damage);
        Debug.Log($"{closest.name}에게 {damage} 데미지 (가장 가까운 적)");
    }

    private void OnDrawGizmosSelected()
    {
        if (attackHitbox == null) return;

        Color color = currentPhase switch
        {
            AttackPhase.Windup   => windupColor,
            AttackPhase.Active   => activeColor,
            AttackPhase.Recovery => recoveryColor,
            _                    => new Color(1f, 1f, 1f, 0.3f),
        };

        // bounds는 collider가 disabled일 때 (0,0,0)을 반환하므로
        // BoxCollider2D면 offset·size를 직접 읽고, 그 외 타입은 transform 위치에 작은 구로 표시
        if (attackHitbox is BoxCollider2D box)
        {
            Vector3 center = attackHitbox.transform.TransformPoint(box.offset);
            Vector3 size   = new Vector3(
                box.size.x * attackHitbox.transform.lossyScale.x,
                box.size.y * attackHitbox.transform.lossyScale.y,
                0.05f);

            Gizmos.color = color;
            Gizmos.DrawCube(center, size);
            Gizmos.color = new Color(color.r, color.g, color.b, 1f);
            Gizmos.DrawWireCube(center, size);
        }
        else if (attackHitbox is CircleCollider2D circle)
        {
            Vector3 center = attackHitbox.transform.TransformPoint(circle.offset);
            float radius   = circle.radius * Mathf.Max(
                attackHitbox.transform.lossyScale.x,
                attackHitbox.transform.lossyScale.y);

            Gizmos.color = color;
            Gizmos.DrawSphere(center, radius);
            Gizmos.color = new Color(color.r, color.g, color.b, 1f);
            Gizmos.DrawWireSphere(center, radius);
        }
        else
        {
            // PolygonCollider2D 등 — 위치만 표시
            Gizmos.color = new Color(color.r, color.g, color.b, 1f);
            Gizmos.DrawWireSphere(attackHitbox.transform.position, 1f);
        }
    }

    // parryReduction        : 일반 패리 데미지 감소 비율 (0=패리불가, 1=완전차단)
    // parryPerfectReduction : 퍼펙트 패리 데미지 감소 비율 (기본 1=완전차단)
    // isReflectable         : 퍼펙트 패리 시 적에게 반사경직을 줄지 여부
    public void TakeDamage(float damage, EnemyBase attacker = null,
                           float parryReduction = 0f, float parryPerfectReduction = 1f,
                           bool isReflectable = false)
    {
        if (isParrying && parryReduction > 0f)
        {
            float elapsed = Time.time - parryStartTime;

            if (elapsed < perfectParryWindow)
            {
                float remaining = damage * (1f - parryPerfectReduction);
                Debug.Log($"퍼펙트 패리! (패리 후 {elapsed * 1000f:F0}ms) " +
                          $"감소율 {parryPerfectReduction * 100f:F0}% → 잔여 데미지 {remaining:F1}");
                if (remaining > 0f) currentHealth -= remaining;
                if (isReflectable)
                    attacker?.OnPerfectParried();
                return;
            }

            float blocked = damage * (1f - parryReduction);
            Debug.Log($"패리 성공 (일반) 감소율 {parryReduction * 100f:F0}% → 잔여 데미지 {blocked:F1}");
            if (blocked > 0f) currentHealth -= blocked;
            return;
        }

        currentHealth -= damage;
        Debug.Log($"{damage} 데미지 받음, 남은 체력: {currentHealth}");
    }
}
