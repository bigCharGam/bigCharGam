using UnityEngine;
using UnityEngine.InputSystem; // ⚡ 필수 확인

public class PlayerAttack : PlayerMovement
{
    private enum ParryDirection { None, Up, Left, Right }
    private ParryDirection currentParryDirection = ParryDirection.None;

    [Header("Attack Settings")]
    [SerializeField] private float normalAttackDashForce = 12f;
    [SerializeField] private float bigAttackDashForce = 25f;

    private float attackDurationTimer;
    private float currentAttackDuration = 0.15f;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        // ⚡ [핵심 수정] Shift 키를 완전히 떼었을 때 실시간 감지하여 패리 해제
        // 왼쪽 Shift나 오른쪽 Shift 중 아무것도 누르고 있지 않다면 강제 종료
        if (isParrying)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && !keyboard.leftShiftKey.isPressed && !keyboard.rightShiftKey.isPressed)
            {
                Debug.Log("실시간 Shift 해제 감지: 패링 종료.");
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
        Debug.Log("기본 공격.");
        currentAction = ActionState.Attacking;
        attackDurationTimer = currentAttackDuration;
    }

    private void OnBigAttack()
    {
        if (currentAction == ActionState.Dashing || currentAction == ActionState.Attacking) return;
        Debug.Log("강한 공격.");
        currentAction = ActionState.Attacking;
        attackDurationTimer = 0.25f;
        rb.linearVelocity = new Vector2(lastDirectionX * bigAttackDashForce, rb.linearVelocity.y);
    }

    // ⚡ [인풋 시스템 연동] 위쪽 패링 (Shift + W)
    private void OnWParry(InputValue value)
    {
        if (currentAction == ActionState.Dashing) return;

        if (value.isPressed)
        {
            EnterParry(ParryDirection.Up, "상단 패링 자세 돌입!");
        }
        else
        {
            ExitParry(ParryDirection.Up, "상단 패링 자세 해제.");
        }
    }

    // ⚡ [인풋 시스템 연동] 왼쪽 패링 (Shift + A)
    private void OnAParry(InputValue value)
    {
        if (currentAction == ActionState.Dashing) return;

        if (value.isPressed)
        {
            EnterParry(ParryDirection.Left, "좌측 패링 자세 돌입!");
        }
        else
        {
            ExitParry(ParryDirection.Left, "좌측 패링 자세 해제.");
        }
    }

    // ⚡ [인풋 시스템 연동] 오른쪽 패링 (Shift + D)
    private void OnDParry(InputValue value)
    {
        if (currentAction == ActionState.Dashing) return;

        if (value.isPressed)
        {
            EnterParry(ParryDirection.Right, "우측 패링 자세 돌입!");
        }
        else
        {
            ExitParry(ParryDirection.Right, "우측 패링 자세 해제.");
        }
    }

    private void EnterParry(ParryDirection direction, string logMessage)
    {
        Debug.Log(logMessage);
        isParrying = true;
        currentParryDirection = direction;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void ExitParry(ParryDirection direction, string logMessage)
    {
        if (currentParryDirection == direction)
        {
            Debug.Log(logMessage);
            isParrying = false;
            currentParryDirection = ParryDirection.None;
        }
    }

    // Shift 키 해제 시 예외 없이 패리를 푸는 강제 해제 메서드
    private void ForceExitParry()
    {
        isParrying = false;
        currentParryDirection = ParryDirection.None;
    }

    private void OnPotion() { }
    private void OnDoLoco() { }
}