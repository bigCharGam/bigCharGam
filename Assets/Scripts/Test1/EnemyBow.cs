using UnityEngine;
using System.Collections;

[System.Serializable]
public struct SkillDataBow
{
    public float damage;
    public float shootPower;
    public float minUseRange;
}

public class EnemyBow : EnemyBase
{
    [Header("Skills")]
    [SerializeField] private SkillDataBow[] skills;

    [Header("Battle")]
    [SerializeField] private float runSpeed;
    [SerializeField] private float backStepSpeed;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;

    private bool isAttacking = false;
    private int selectedSkillIndex = -1;
    private float pendingDamage;
    private float pendingShootPower;

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
            if (p < 20)
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
            pendingDamage = skills[selectedSkillIndex].damage;
            pendingShootPower = skills[selectedSkillIndex].shootPower;
            isAttacking = true;
            anim.SetInteger("moveLevel", 0);
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            anim.SetTrigger("skill_" + selectedSkillIndex);
        }
    }

    // 직접 호출하지 않고 Animation Event에서 호출하는 함수들
    private void ArrowStart()
    {
        if (arrowPrefab == null || arrowSpawnPoint == null) return;

        float direction = transform.localScale.x >= 0 ? 1f : -1f;
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript == null) return;
        arrowScript.Shoot(pendingDamage, pendingShootPower, direction);
    }
    private void OnAttackEnd()
    {
        isAttacking = false;
        selectedSkillIndex = -1;
    }
}
