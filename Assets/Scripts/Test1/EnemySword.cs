using UnityEngine;
using System.Collections;

// 버그잡기
public enum EnemySwordBattleState
{
    SkillSelctAndGo,
    SkillUsing,
    SkillEndIdle,
    Parry,
    OnHit,
}

public class EnemySword : EnemyBase
{
    [Header("Skills")]
    [SerializeField] private SkillDataMelee[] skills;

    [Header("Battle")]
    [SerializeField] private float runSpeed;
    [SerializeField] private float backStepSpeed;
    [SerializeField] private float backStepJumpForce;
    [SerializeField] private float tooCloseRange; //너무 가까울시 backwalk
    [SerializeField] private int selectedSkillIndex = -1;
    public ParticleSystem parryEffect1;
    public ParticleSystem parryEffect2;
    private float endIdleTime = 0f;
    private float endIdleTimeElapsed = 0f;

    [Header("Debug")]
    public int skillDisplay = -1;
    [SerializeField] private bool parryAble = false; // 추후 not Serialize
    [SerializeField] private EnemySwordBattleState state = EnemySwordBattleState.SkillSelctAndGo;

    private void Reset()
    {
        maxHealth = 150f;
        moveSpeed = 8f;
        backStepSpeed = 30f;
        detectionRange = 10f;
        runSpeed = 12f;
    }

    protected override void Start()
    {
        base.Start();

        for (int i = 0; i < skills.Length; i++)
        {
            skills[i].weightNow = skills[i].weightInit;
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void HandleBattle()
    {
        switch (state)
        {
            case EnemySwordBattleState.SkillSelctAndGo:
                HandleSkillSelectAndGo();
                break;
            case EnemySwordBattleState.SkillUsing:
                HandleSkillUsing();
                break;
            case EnemySwordBattleState.SkillEndIdle:
                HandleSkillEndIdle();
                break;
            case EnemySwordBattleState.Parry:
                HandleParry();
                break;
            case EnemySwordBattleState.OnHit:
                break;
        }
    }

    private void HandleSkillSelectAndGo()
    {
        if (skills == null || skills.Length == 0) return;

        // minRange ~ maxRange 사이에 있는 스킬 중에서 랜덤으로 선택
        if (selectedSkillIndex == -1)
        {
            int weightSum = 0;
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i].minRange <= distanceToPlayer && skills[i].maxRange >= distanceToPlayer)
                {
                    weightSum += skills[i].weightNow;
                }
            }
            int r = Random.Range(0, weightSum);
            int a = 0;
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i].minRange <= distanceToPlayer && skills[i].maxRange >= distanceToPlayer)
                {
                    a += skills[i].weightNow;
                    if (r < a)
                    {
                        selectedSkillIndex = i;
                        break;
                    }
                }
            }

            // 백스텝 사거리 안에서 다른 공격 시 백스텝 확률 증가
            if (selectedSkillIndex != -1 && skills[2].weightNow < 100 && distanceToPlayer < skills[2].maxRange)
            {
                skills[2].weightNow += 10;
            }
        }

        // 실제 사용 거리(UseRange) 까지 달려가서 사용
        float requiredRange = skills[selectedSkillIndex].UseRange;

        if (distanceToPlayer > requiredRange)
        {
            anim.SetInteger("moveLevel", 2);
            MoveToTarget(playerTransform, runSpeed);
        }
        else
        {
            anim.SetInteger("moveLevel", 0);
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            hitReactImmune = skills[selectedSkillIndex].isImmune;
            anim.SetTrigger("skill_" + selectedSkillIndex);

            state = EnemySwordBattleState.SkillUsing;
        }
    }
    private void HandleSkillUsing()
    {
        return;
    }
    private void HandleSkillEndIdle()
    {
        endIdleTimeElapsed += Time.deltaTime;
        if (endIdleTimeElapsed > 0.5f)
        {
            parryAble = true;
        }
        if (distanceToPlayer < tooCloseRange) // 너무 가까우면 Back Walk
        {
            anim.SetInteger("moveLevel", 1);
            rb.linearVelocity = new Vector2(-0.5f * moveSpeed, rb.linearVelocity.y);
        }
        if (endIdleTimeElapsed >= endIdleTime)
        {
            parryAble = false;
            selectedSkillIndex = -1;
            state = EnemySwordBattleState.SkillSelctAndGo;
        }
    }
    private void HandleParry()
    {
        return;
    }

    public override void TakeDamage(float damage)
    {
        if (parryAble)
        {
            // 패리 성공 → 데미지X, 히트리액트X
            parryAble = false;
            state = EnemySwordBattleState.Parry;
            anim.SetTrigger("parry");
            parryEffect1.Play();
            parryEffect2.Play();
            return;
        }

        base.TakeDamage(damage);
    }

    // 피격 시 히트박스 끄고 스킬 종료
    protected override void OnHitStart()
    {
        base.OnHitStart();
        state = EnemySwordBattleState.OnHit;
        hitReactImmune = false;

        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i].hitbox != null)
                skills[i].hitbox.SetActive(false);
        }
    }

    // 직접 호출하지 않고 Animation Event에서 호출하는 함수들

    //모든 스킬 종료 시
    private void OnAttackEnd()
    {
        hitReactImmune = false;
        state = EnemySwordBattleState.SkillEndIdle;
        endIdleTime = Random.Range(0.1f, 1.5f);
        endIdleTimeElapsed = 0f;
    }

    // 피격, 패리
    protected override void OnHitEnd()
    {
        base.OnHitEnd();
        state = EnemySwordBattleState.SkillSelctAndGo;
        selectedSkillIndex = -1;
    }
    private void OnParryEnd()
    {
        state = EnemySwordBattleState.SkillSelctAndGo;
        selectedSkillIndex = -1;
    }

    //공격 스킬 공용
    private void SetHitbox(int index, bool active)
    {
        if (index < 0 || index >= skills.Length) return;
        skills[index].hitbox.SetActive(active);
        if (active)
        {
            var hitboxComp = skills[index].hitbox.GetComponent<EnemyAttackHitbox>();
            hitboxComp.damage = skills[index].damage;
            hitboxComp.parryReduction = skills[index].parryReduction;
            hitboxComp.parryPerfectReduction = skills[index].parryPerfectReduction;
            hitboxComp.isReflectable = skills[index].isReflectable;
        }
    }

    //스킬0
    private void HitboxEnable_0()
    {
        SetHitbox(0, true);
    }
    private void HitboxDisable_0()
    {
        SetHitbox(0, false);
    }

    //스킬1
    private void HitboxEnable_1()
    {
        SetHitbox(1, true);
    }
    private void HitboxDisable_1()
    {
        SetHitbox(1, false);
    }

    //스킬2
    public void BackStepStart()
    {
        rb.linearVelocity = new Vector2(transform.localScale.x * -backStepSpeed, backStepJumpForce);
    }
    public void BackStepEnd()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        skills[2].weightNow = skills[2].weightInit;
    }
}
