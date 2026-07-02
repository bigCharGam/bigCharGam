using UnityEngine;
using System.Collections;

[System.Serializable]
public struct SkillData
{
    public GameObject hitbox;
    public float damage;
    public float minUseRange;
}

public class TestEnemy1 : EnemyBase
{
    [Header("Skills")]
    [SerializeField] private SkillData[] skills;

    [Header("Battle")]
    [SerializeField] private float runSpeed;
    [SerializeField] private float backStepSpeed;

    private bool isAttacking = false;
    private int selectedSkillIndex = -1;

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
        anim = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void HandleBattle()
    {
        if (isAttacking) return;
        if (skills == null || skills.Length == 0) return;

        // 어떤 스킬 사용할지 결정
        // 임시) 랜덤으로 스킬 사용
        if (selectedSkillIndex == -1)
        {
            int p = Random.Range(0, 100);
            if (p < 30)
            {
                selectedSkillIndex = 1;
            }
            else
            {
                selectedSkillIndex = 0;
            }
        }

        float requiredRange = skills[selectedSkillIndex].minUseRange;

        // 스킬별 사거리까지 달려가서 사용
        if (distanceToPlayer > requiredRange)
        {
            anim.SetInteger("moveLevel", 2);
            MoveToTarget(playerTransform, runSpeed);
        }
        else
        {
            anim.SetInteger("moveLevel", 0);
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            isAttacking = true;
            anim.SetTrigger("skill_" + selectedSkillIndex);
            selectedSkillIndex = -1;
        }
    }

    // 직접 호출하지 않고 Animation Event에서 호출하는 함수들
    private void HitboxEnable_0()
    {
        SetHitbox(0, true);
    }
    private void HitboxDisable_0()
    {
        SetHitbox(0, false);
    }
    private void HitboxEnable_1()
    {
        SetHitbox(1, true);
    }
    private void HitboxDisable_1()
    {
        SetHitbox(1, false);
    }
    private void SetHitbox(int index, bool active)
    {
        if (index < 0 || index >= skills.Length) return;
        skills[index].hitbox.SetActive(active);
        if (active)
        {
            skills[index].hitbox.GetComponent<EnemyAttackHitbox>().damage = skills[index].damage;
        }
    }
    private void OnAttackEnd()
    {
        isAttacking = false;
    }
    public void BackStepStart()
    {
        anim.SetInteger("moveLevel", 2);
        rb.linearVelocity = new Vector2(-backStepSpeed, rb.linearVelocity.y);
    }
    public void BackStepEnd()
    {
        anim.SetInteger("moveLevel", 0);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
}
