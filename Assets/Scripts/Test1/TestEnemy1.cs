using UnityEngine;
using System.Collections;

public class TestEnemy1 : EnemyBase
{
    [Header("Skill 1 Slash")]
    [SerializeField] private GameObject skill1Hitbox;
    [SerializeField] private float skill1Damage;
    [SerializeField] private float skill1MinUseRange;

    [Header("Battle")]
    [SerializeField] private float runSpeed;

    private bool isAttacking = false;
    private int selectedSkillIndex = -1;

    private void Reset()
    {
        maxHealth = 150f;
        moveSpeed = 8f;
        skill1Damage = 10f;
        skill1MinUseRange = 2f;
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

        if (selectedSkillIndex == -1)
            selectedSkillIndex = Random.Range(0, 1);

        float requiredRange = GetMinSkillRange(selectedSkillIndex);

        if (distanceToPlayer > requiredRange)
        {
            MoveToTarget(playerTransform, runSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            isAttacking = true;
            UseSkill(selectedSkillIndex);
            selectedSkillIndex = -1;
        }
    }

    private float GetMinSkillRange(int index)
    {
        switch (index)
        {
            case 0: return skill1MinUseRange;
            default: return 2f;
        }
    }

    private void UseSkill(int index)
    {
        switch (index)
        {
            case 0: anim.SetTrigger("Skill1_Slash"); break;
        }
    }

    // Animation Event에서 호출
    private void OnAttackEnd()
    {
        isAttacking = false;
    }

    // Animation Event에서 호출
    private void HitboxEnable_Skill1()
    {
        skill1Hitbox.SetActive(true);
        skill1Hitbox.GetComponent<EnemyAttackHitbox>().damage = skill1Damage;
    }

    // Animation Event에서 호출
    private void HitboxDisable_Skill1()
    {
        skill1Hitbox.SetActive(false);
    }
}
