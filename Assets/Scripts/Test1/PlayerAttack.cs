using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : PlayerMovement
{
    [Header("Attack Settings")]
    [SerializeField] private float normalAttackDashForce = 12f; // 일반 공격 시 전방 전진 힘
    [SerializeField] private float bigAttackDashForce = 25f;   // 강공격 시 전방 돌진 힘

    // 공격이나 스킬 지속시간을 제어하기 위한 내부 타이머
    private float attackDurationTimer;
    private float currentAttackDuration = 0.15f;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        // ⚡ 공격/스킬 동작 제한 시간 타이머 마모 처리
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
        // ⚡ 중요: 부모의 FixedUpdate()를 먼저 실행하여 기본 물리(Grounded 체크 등)를 수행합니다.
        base.FixedUpdate();

        // 부모의 FixedUpdate()가 끝난 직후, 만약 공격 중인 상태라면 물리 속도를 강제로 제어합니다.
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

        Debug.Log("기본 공격 발동!");
        currentAction = ActionState.Attacking;
        attackDurationTimer = currentAttackDuration;
    }

    private void OnBigAttack()
    {
        if (currentAction == ActionState.Dashing || currentAction == ActionState.Attacking) return;

        Debug.Log("강공격 발동!");
        currentAction = ActionState.Attacking;
        attackDurationTimer = 0.25f;
        rb.linearVelocity = new Vector2(lastDirectionX * bigAttackDashForce, rb.linearVelocity.y);
    }

    // ⚡ [인풋 시스템 연동] 패링 (Shift + W/A/D)
    // 인스펙터의 Player Input 컴포넌트 Behavior 설정이 'Send Messages'일 경우 작동합니다.
    // 인풋 액션의 다양한 국면(Started, Canceled)을 캐치하기 위해 CallbackContext를 매개변수로 받습니다.
    private void OnParry(InputValue value)
    {
        if (currentAction == ActionState.Dashing) return;

        // PlayerInput에서 전달되는 컨텍스트를 추출
        var context = value.Get<float>();

        // 유니티 인풋 시스템에서 버튼 복합키가 성립하여 신호가 들어오는 중일 때 (Value가 0보다 큼)
        if (value.isPressed)
        {
            Debug.Log("패링 자세 돌입 (무적 활성화)!");
            isParrying = true; // 부모(PlayerBattle)의 변수를 직접 제어

            // 패링 시에는 제자리에 멈추게 하고 싶다면 속도를 0으로 리셋
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else // 키를 떼서 신호가 끊겼을 때 (Canceled 국면 대응)
        {
            Debug.Log("패링 자세 해제 (무적 종료).");
            isParrying = false;
        }
    }

    private void OnPotion()
    {
        Debug.Log("포션 아이템 소비!");
    }

    private void OnDoLoco()
    {
        Debug.Log("발도");
        Debug.Log("납도");
    }
}