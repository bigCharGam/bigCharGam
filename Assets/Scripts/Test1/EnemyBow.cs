using UnityEngine;
using System.Collections;

public class EnemyBow : EnemyBase
{
    [Header("Skills")]
    [SerializeField] private SkillDataBow[] skills;

    [Header("Battle")]
    [SerializeField] private float runSpeed;
    [SerializeField] private float backStepSpeed;
    [SerializeField] private float backStepJumpForce;
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
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();

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
        if (isAttacking) return;
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
            pendingDamage = skills[selectedSkillIndex].damage;
            pendingShootPower = skills[selectedSkillIndex].shootPower;

            isAttacking = true;
            anim.SetInteger("moveLevel", 0);
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            anim.SetTrigger("skill_" + selectedSkillIndex);
        }
    }

    // 직접 호출하지 않고 Animation Event에서 호출하는 함수들

    // 모든 스킬 종료 시 공통
    private void OnAttackEnd()
    {
        isAttacking = false;
        selectedSkillIndex = -1;
    }

    // 스킬0, 스킬1
    private void ArrowStart()
    {
        if (arrowPrefab == null || arrowSpawnPoint == null) return;

        float direction = transform.localScale.x >= 0 ? 1f : -1f;
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript == null) return;
        arrowScript.Shoot(pendingDamage, pendingShootPower, direction);
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

    //스킬3
    public void RunStart()
    {
        StartCoroutine(RunStartCoroutine());
    }

    private IEnumerator RunStartCoroutine()
    {
        if (playerTransform == null) yield break;

        float fleeDirection = transform.position.x > playerTransform.position.x ? 1f : -1f;
        transform.localScale = new Vector3(fleeDirection, 1f, 1f);

        float distance = Random.Range(10f, 15f);
        float startX = transform.position.x;

        while (Mathf.Abs(transform.position.x - startX) < distance)
        {
            rb.linearVelocity = new Vector2(fleeDirection * runSpeed, rb.linearVelocity.y);
            yield return null;
        }

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 도망 후 다시 플레이어 바라보기
        float facePlayer = playerTransform.position.x > transform.position.x ? 1f : -1f;
        transform.localScale = new Vector3(facePlayer, 1f, 1f);

        OnAttackEnd();
    }

    //스킬4
    public void BackWalkStart()
    {
        StartCoroutine(MoveBackwardForDistance(moveSpeed, Random.Range(5f, 10f)));

    }
    private IEnumerator MoveBackwardForDistance(float speed, float distance)
    {
        float direction = transform.localScale.x >= 0 ? -1f : 1f;
        float startX = transform.position.x;
        while (Mathf.Abs(transform.position.x - startX) < distance)
        {
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
            yield return null;
        }
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        OnAttackEnd();
    }
}
