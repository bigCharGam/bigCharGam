using UnityEngine;
using System.Collections;

[System.Serializable]
public struct SkillData
{
    public string animTrigger;
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

    private bool isAttacking = false;
    private int selectedSkillIndex = -1;

    private void Reset()
    {
        maxHealth = 150f;
        moveSpeed = 8f;
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

        if (selectedSkillIndex == -1)
            selectedSkillIndex = Random.Range(0, skills.Length);

        float requiredRange = skills[selectedSkillIndex].minUseRange;

        if (distanceToPlayer > requiredRange)
        {
            MoveToTarget(playerTransform, runSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            isAttacking = true;
            anim.SetTrigger(skills[selectedSkillIndex].animTrigger);
            selectedSkillIndex = -1;
        }
    }

    // Animation Event에서 호출 — 이름 형식: HitboxEnable_{스킬인덱스}
    private void HitboxEnable_0()
    {
        SetHitbox(0, true);
    }

    private void HitboxDisable_0()
    {
        SetHitbox(0, false);
    }

    private void SetHitbox(int index, bool active)
    {
        if (index < 0 || index >= skills.Length) return;
        skills[index].hitbox.SetActive(active);
        if (active)
            skills[index].hitbox.GetComponent<EnemyAttackHitbox>().damage = skills[index].damage;
    }

    // Animation Event에서 호출
    private void OnAttackEnd()
    {
        isAttacking = false;
    }
}
