using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : PlayerMovement
{
    private enum ParryDirection { None, Up, Left, Right }
    private ParryDirection currentParryDirection = ParryDirection.None;

    [Header("Attack Settings")]
    [SerializeField] private float normalAttackDashForce = 12f;
    [SerializeField] private float bigAttackDashForce = 25f;

    [Header("Attack Range Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackSize = new Vector2(2f, 1f);
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private int attackDamage = 10;

    private float attackDurationTimer;
    private float currentAttackDuration = 0.15f;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (isParrying)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && !keyboard.leftShiftKey.isPressed && !keyboard.rightShiftKey.isPressed)
            {
                ForceExitParry();
            }
        }

        if (attackDurationTimer > 0)
        {
            attackDurationTimer -= Time.deltaTime;
            if (attackDurationTimer <= 0)
            {
                if (currentAction == ActionState.Attacking || currentAction == ActionState.SkillUsing)
                {
                    currentAction = isPressingAD ? ActionState.Locomotion : ActionState.None;
                }
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        switch (currentAction)
        {
            case ActionState.Attacking:
                rb.linearVelocity = new Vector2(lastDirectionX * normalAttackDashForce, rb.linearVelocity.y);
                break;
            case ActionState.SkillUsing:
                break;
        }
    }

    private void OnAttack()
    {
        if (currentAction == ActionState.Dashing || currentAction == ActionState.Attacking) return;

        currentAction = ActionState.Attacking;
        attackDurationTimer = currentAttackDuration;

        // 공격 판정 호출
        CheckAttackHit();
    }

    private void CheckAttackHit()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackSize, 0f, enemyLayers);
        Debug.Log($"공격 범위 내 감지된 적 수: {hitEnemies.Length}");
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<EnemyBase>(out var enemyComponent))
            {
                enemyComponent.TakeDamage(attackDamage);
                Debug.Log($"{enemy.name}에게 {attackDamage} 데미지를 입혔습니다!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackSize.x, attackSize.y, 1));
    }

    private void OnBigAttack()
    {
        if (currentAction == ActionState.Dashing || currentAction == ActionState.Attacking) return;
        currentAction = ActionState.Attacking;
        attackDurationTimer = 0.25f;
        rb.linearVelocity = new Vector2(lastDirectionX * bigAttackDashForce, rb.linearVelocity.y);
        CheckAttackHit();
    }

    private void OnWParry(InputValue value)
    {
        if (currentAction == ActionState.Dashing) return;
        if (value.isPressed) EnterParry(ParryDirection.Up, "상단 패링 자세 돌입!");
        else ExitParry(ParryDirection.Up, "상단 패링 자세 해제.");
    }

    private void OnAParry(InputValue value)
    {
        if (currentAction == ActionState.Dashing) return;
        if (value.isPressed) EnterParry(ParryDirection.Left, "좌측 패링 자세 돌입!");
        else ExitParry(ParryDirection.Left, "좌측 패링 자세 해제.");
    }

    private void OnDParry(InputValue value)
    {
        if (currentAction == ActionState.Dashing) return;
        if (value.isPressed) EnterParry(ParryDirection.Right, "우측 패링 자세 돌입!");
        else ExitParry(ParryDirection.Right, "우측 패링 자세 해제.");
    }

    private void EnterParry(ParryDirection direction, string logMessage)
    {
        isParrying = true;
        currentParryDirection = direction;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void ExitParry(ParryDirection direction, string logMessage)
    {
        if (currentParryDirection == direction)
        {
            isParrying = false;
            currentParryDirection = ParryDirection.None;
        }
    }

    private void ForceExitParry()
    {
        isParrying = false;
        currentParryDirection = ParryDirection.None;
    }

    private void OnPotion() { }
    private void OnDoLoco() { }
}