using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct SkillData
{
    public string skillName;
    public GameObject hitboxDetection;
    public GameObject hitboxAttack;
    public float damageMultiplier;
    public float cooldown;
    public bool isCooldown;
    public float usingStamina;
    public Vector2 QTERotateRange; // QTEIndicator의 회전 범위 (min, max)
}

public struct DamageGraph
{
    public float time;
    public float damage;
}

public enum SkillState
{
    Idle,
    Skill1,
    Skill2,
    Skill3,
}

public class PlayerSkill : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerStats playerStats;
    [SerializeField] private SkillData[] skills;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private QTEIndicator qteIndicatorPrefab;
    [SerializeField] private SkillState skillState = SkillState.Idle;
    [SerializeField] private SkillTimeDamageGraph graph;
    private int skillDamage; // QTE중 실시간으로 계산되는 스킬 데미지

    [Header("Skill1")]
    private List<EnemyBase> detectedEnemies = new List<EnemyBase>();
    private EnemyBase closestEnemy;
    private QTEIndicator currentQteIndicator;
    private int QTEStack = 0;

    [Header("Skill2")]
    [SerializeField] private float s2MinDistance;
    [SerializeField] private float s2RushSpeed = 10f;
    [SerializeField] private float s2AttackDistance = 1f; // 적 앞에서 멈추는 거리

    [Header("Skill3")]
    [SerializeField] private float backStepSpeed = 10f;
    [SerializeField] private float backStepJumpForce = 3f;
    [SerializeField] private float backStepDuration = 0.3f;
    [SerializeField] private GameObject qteSpawnPoint;
    [SerializeField] private GameObject hitboxSpawnPoint; // 다시 앞으로 가는 시간
    [SerializeField] private float reDashTime = 0.15f;
    [SerializeField] private float s3AttackDistance = 1f; // 적 앞에서 멈추는 거리
    [SerializeField] private float s3MaxMoveDistance = 3f; // 돌진 이동 가능한 최대 거리
    [SerializeField] private GameObject DropingHitboxPrefab;
    [SerializeField] private GameObject DropingHitboxSpawnPoint;
    [SerializeField] private float avoidDamageBonus = 1.5f; // 회피 성공 시 데미지 증가 배율


    [Header("Effect")]
    //effect test
    public GameObject s1Effect;
    public GameObject s1EffectPerfect;
    public GameObject s1EffectSpawnPoint;
    public GameObject s2Effect;
    public GameObject s2EffectPerfect;
    public GameObject s2EffectSpawnPoint;
    public GameObject s3Effect;
    public GameObject s3EffectPerfect;
    public GameObject s3EffectSpawnPoint;
    
    private float elapsedTime = 0f;
    private int s2TargetIndex;
    private bool s2Resolved;
    private float s2StartTime;
    private float s2PerfectStart;
    private float s2PerfectEnd;
    private float s2EarliestInput;
    private float s2CurrentSpawnTime;
    private List<EnemyBase> s2Targets = new List<EnemyBase>();
    private QTEIndicator[] s2Indicators;
    private Rigidbody2D playerRb;
    private PlayerMovement playerMovement;
    private DecoyHitbox currentDecoyHitbox;
    
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerStats = GetComponent<PlayerStats>();
        playerRb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private bool SkillStarter(int index)
    {
        if (BattleManager.instance.skillLevels[index] == 0) return false; // 스킬 레벨이 0이면 무시
        if (skills[index].isCooldown || skillState != SkillState.Idle) return false; // 스킬 쿨타임 중이거나 QTE 진행 중이면 무시
        if (playerStats == null || playerStats.currentStamina < skills[index].usingStamina) return false;

        playerStats.currentStamina -= skills[index].usingStamina;
        UIManager.instance?.UpdatePlayerMP(playerStats.currentStamina, playerStats.maxStamina);
        UIManager.instance?.SetSkillImage(index, false); // 스킬 쿨타임 도는 이미지

        playerMovement?.BeginSkillUsing();
        playerInput.SwitchCurrentActionMap("OnSkill");
        return true;
    }
    private void OnSkill1()
    {
        if (!SkillStarter(0)) return;


        skillState = SkillState.Skill1;
        StartCoroutine(Skill1());
        StartCoroutine(SkillDamageCal());
        StartCoroutine(Cooldown(0));
    }
    private void OnSkill2()
    {
        if (!SkillStarter(1)) return;

        skillState = SkillState.Skill2;
        StartCoroutine(Skill2());
        StartCoroutine(Cooldown(1));
    }
    private void OnSkill3()
    {
        if (!SkillStarter(2)) return;

        skillState = SkillState.Skill3;
        StartCoroutine(Skill3());
        StartCoroutine(SkillDamageCal());
        StartCoroutine(Cooldown(2));
    }
    private void OnQTE()
    {
        switch (skillState)
        {
            case SkillState.Skill1:
                QTESkill1();
                break;
            case SkillState.Skill2:
                QTESkill2();
                break;
            case SkillState.Skill3:
                QTESkill3();
                break;
        }
    }

    private void QTESkill1()
    {
        if (elapsedTime < graph.damageGraph[0].time) return; // QTE 입력이 너무 빠른 경우 무시
        playerInput.SwitchCurrentActionMap("Player");
        Debug.Log(elapsedTime);

        currentQteIndicator?.SkipToStage4(); // 버튼 누르면 페이드아웃

        // effect
        if (elapsedTime > graph.damageGraph[2].time && elapsedTime < graph.damageGraph[3].time) 
        {
            Instantiate(s1EffectPerfect, s1EffectSpawnPoint.transform.position, s1EffectPerfect.transform.rotation);
        }
        else
        {
            Instantiate(s1Effect, s1EffectSpawnPoint.transform.position, s1Effect.transform.rotation);
        }

        if (closestEnemy == null)
        {
            skillState = SkillState.Idle;
            playerMovement?.EndSkillUsing();
            return;
        }

        PlayerSkillHitbox skillHitbox = Instantiate(skills[0].hitboxAttack, closestEnemy.transform.position, Quaternion.identity).GetComponent<PlayerSkillHitbox>();
        skillHitbox.damage = skillDamage * skills[0].damageMultiplier; // 물리 시뮬레이션보다 대입이 빨라서 히트박스가 충분히 damage를 받음
        skillState = SkillState.Idle;
        playerMovement?.EndSkillUsing();
    }
    private void QTESkill2()
    {
        if (s2Resolved || s2TargetIndex >= s2Targets.Count) return;
        float elapsed = Time.time - s2StartTime;
        if (elapsed < s2EarliestInput) return; // 너무 이른 입력 무시

        bool isPerfect = elapsed >= s2PerfectStart && elapsed <= s2PerfectEnd;
        EnemyBase target = s2Targets[s2TargetIndex];
        s2Indicators[s2TargetIndex]?.SkipToStage4();

        PlayerSkillHitbox skillHitbox = Instantiate(skills[1].hitboxAttack, target.transform.position, Quaternion.identity).GetComponent<PlayerSkillHitbox>();
        skillHitbox.damage = EvaluateDamage(graph, elapsed - s2CurrentSpawnTime) * skills[1].damageMultiplier;
        s2Resolved = true;

        // effect
        if (elapsedTime > graph.damageGraph[2].time && elapsedTime < graph.damageGraph[3].time) 
        {
            Instantiate(s2EffectPerfect, s2EffectSpawnPoint.transform.position, s2EffectPerfect.transform.rotation);
        }
        else
        {
            Instantiate(s2Effect, s2EffectSpawnPoint.transform.position, s2Effect.transform.rotation);
        }
    }
    private void QTESkill3()
    {
        if (elapsedTime < graph.damageGraph[0].time) return; // QTE 입력이 너무 빠른 경우 무시
        playerInput.SwitchCurrentActionMap("Player");

        currentQteIndicator?.SkipToStage4(); // 버튼 누르면 페이드아웃

        StartCoroutine(Skill3DashToEnemy());
    }

    // 가장 가까운 적 앞까지 0.1초간 이동(최대 이동거리 제한)한 뒤 기존 이펙트/히트박스를 발생시킴
    private IEnumerator Skill3DashToEnemy()
    {
        EnemyBase target = FindClosestEnemy(skills[2].hitboxDetection);

        float moveDist = 0f;
        if (target != null)
        {
            float dir = Mathf.Sign(target.transform.position.x - transform.position.x);
            float desiredX = target.transform.position.x - dir * s3AttackDistance; // 적 앞에서 멈출 위치
            moveDist = Mathf.Clamp(desiredX - transform.position.x, -s3MaxMoveDistance, s3MaxMoveDistance);
        }
        float velocityX = moveDist / reDashTime;

        float timer = 0f;
        while (timer < reDashTime)
        {
            if (playerRb != null)
            {
                playerRb.linearVelocity = new Vector2(velocityX, playerRb.linearVelocity.y);
            }
            timer += Time.deltaTime;
            yield return null;
        }
        if (playerRb != null) playerRb.linearVelocity = new Vector2(0f, playerRb.linearVelocity.y);

        // effect
        if (elapsedTime > graph.damageGraph[2].time && elapsedTime < graph.damageGraph[3].time) 
        {
            Instantiate(s3EffectPerfect, s3EffectSpawnPoint.transform.position, RandomizedEffectRotation(s3EffectPerfect.transform.rotation, skills[2].QTERotateRange));
        }
        else
        {
            Instantiate(s3Effect, s3EffectSpawnPoint.transform.position, RandomizedEffectRotation(s3Effect.transform.rotation, skills[2].QTERotateRange));
        }

        float damageMultiplier = skills[2].damageMultiplier;
        if (currentDecoyHitbox != null)
        {
            if (currentDecoyHitbox.wasHit) damageMultiplier *= avoidDamageBonus; // 미끼가 대신 맞았다면 회피 보너스 적용
            Destroy(currentDecoyHitbox.gameObject);
            currentDecoyHitbox = null;
        }

        PlayerSkillHitbox skillHitbox = Instantiate(skills[2].hitboxAttack, hitboxSpawnPoint.transform.position, Quaternion.identity).GetComponent<PlayerSkillHitbox>();
        skillHitbox.damage = skillDamage * damageMultiplier; // 물리 시뮬레이션보다 대입이 빨라서 히트박스가 충분히 damage를 받음
        skillState = SkillState.Idle;
        playerMovement?.EndSkillUsing();
    }

    // QTEIndicator의 회전 범위 내에서 랜덤한 회전값을 반환
    private Quaternion RandomizedEffectRotation(Quaternion baseRotation, Vector2 angleRange)
    {
        float randomAngle = Random.Range(angleRange.x, angleRange.y);
        return baseRotation * Quaternion.Euler(0f, 0f, randomAngle);
    }

    private EnemyBase FindClosestEnemy(GameObject detectionArea)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(detectionArea.transform.position, detectionArea.transform.localScale, 0f, enemyLayer);
        EnemyBase closest = null;
        float closestDistSqr = float.MaxValue;
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<EnemyBase>(out EnemyBase enemy))
            {
                float distSqr = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closest = enemy;
                    closestDistSqr = distSqr;
                }
            }
        }
        return closest;
    }

    private IEnumerator Skill1()
    {
        detectedEnemies.Clear();
        Collider2D[] hits = Physics2D.OverlapBoxAll(skills[0].hitboxDetection.transform.position, skills[0].hitboxDetection.transform.localScale, 0f, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyBase>(out EnemyBase enemy))
            {
                detectedEnemies.Add(enemy);
            }

        }
        // 가장 가까운 적에게 QTE
        if (detectedEnemies.Count > 0)
        {
            closestEnemy = detectedEnemies[0];
            float closestDistance = (closestEnemy.transform.position - transform.position).sqrMagnitude;

            foreach (EnemyBase enemy in detectedEnemies)
            {
                float distance = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestEnemy = enemy;
                    closestDistance = distance;
                }
            }

            currentQteIndicator = Instantiate(qteIndicatorPrefab, closestEnemy.transform.position, RandomizedEffectRotation(qteIndicatorPrefab.transform.rotation, skills[0].QTERotateRange));
        }

        // 제한시간 내 QTE 입력 없으면 실패 처리
        yield return new WaitForSeconds(graph.damageGraph[graph.damageGraph.Length - 1].time);
        if (skillState == SkillState.Skill1)
        {
            Debug.Log("Skill1 QTE Failed");
            skillState = SkillState.Idle;
            playerMovement?.EndSkillUsing();
            playerInput.SwitchCurrentActionMap("Player");
        }
    }

    private IEnumerator Skill2()
    {
        s2Targets.Clear();
        Collider2D[] hits = Physics2D.OverlapBoxAll(skills[1].hitboxDetection.transform.position, skills[1].hitboxDetection.transform.localScale, 0f, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<EnemyBase>(out EnemyBase enemy) && !s2Targets.Contains(enemy))
            {
                s2Targets.Add(enemy);
            }
        }
        s2Targets.Sort((a, b) =>
            ((a.transform.position - transform.position).sqrMagnitude).CompareTo((b.transform.position - transform.position).sqrMagnitude));

        List<EnemyBase> spacedTargets = new List<EnemyBase>();
        float minTargetDistanceSqr = s2MinDistance * s2MinDistance;
        foreach (EnemyBase candidate in s2Targets)
        {
            bool isFarEnough = true;
            foreach (EnemyBase selectedTarget in spacedTargets)
            {
                if ((candidate.transform.position - selectedTarget.transform.position).sqrMagnitude <= minTargetDistanceSqr)
                {
                    isFarEnough = false;
                    break;
                }
            }

            if (isFarEnough)
            {
                spacedTargets.Add(candidate);
                if (spacedTargets.Count == 3) break;
            }
        }
        s2Targets = spacedTargets;

        if (s2Targets.Count == 0)
        {
            skillState = SkillState.Idle;
            playerMovement?.EndSkillUsing();
            playerInput.SwitchCurrentActionMap("Player");
            yield break;
        }

        int n = s2Targets.Count;
        float indicatorLead = QTEIndicator.ComputeLeadTime(graph); // 스폰~퍼펙트까지 걸리는 고정 시간
        float perfectWindow = QTEIndicator.ComputePerfectWindow(graph);
        float earlyMargin = graph.damageGraph[0].time;

        float dir = Mathf.Sign(s2Targets[n - 1].transform.position.x - transform.position.x);
        float[] travelTime = new float[n];
        float[] perfectTime = new float[n];
        float[] spawnTime = new float[n];
        for (int i = 0; i < n; i++)
        {
            float attackDist = Mathf.Max(0f, Mathf.Abs(s2Targets[i].transform.position.x - transform.position.x) - s2AttackDistance);
            travelTime[i] = attackDist / Mathf.Max(0.01f, s2RushSpeed);
        }
        // 첫 퍼펙트 시각은 indicatorLead로 고정하고, 돌진 시작 시점을 역산한다
        float rushStart = Mathf.Max(0f, indicatorLead - travelTime[0]);
        for (int i = 0; i < n; i++)
        {
            perfectTime[i] = rushStart + travelTime[i]; // 연속 등속 돌진이므로 누적 거리로 계산
            spawnTime[i] = perfectTime[i] - indicatorLead;
        }

        s2Indicators = new QTEIndicator[n];
        s2StartTime = Time.time;
        StartCoroutine(Skill2Rush(dir, rushStart, travelTime[n - 1]));
        StartCoroutine(Skill2IndicatorSpawner(spawnTime));

        for (s2TargetIndex = 0; s2TargetIndex < n; s2TargetIndex++)
        {
            s2PerfectStart = perfectTime[s2TargetIndex] - perfectWindow / 2f;
            s2PerfectEnd = perfectTime[s2TargetIndex] + perfectWindow / 2f;
            s2EarliestInput = spawnTime[s2TargetIndex] + earlyMargin;
            s2CurrentSpawnTime = spawnTime[s2TargetIndex];
            float judgeDeadline = s2PerfectEnd + earlyMargin;

            s2Resolved = false;
            while (!s2Resolved && Time.time - s2StartTime < judgeDeadline)
                yield return null;

            if (!s2Resolved)
            {
                Debug.Log("Skill2 target missed: " + s2TargetIndex);
                s2Indicators[s2TargetIndex]?.SkipToStage4();
            }
        }

        StopRush();
        skillState = SkillState.Idle;
        playerMovement?.EndSkillUsing();
        playerInput.SwitchCurrentActionMap("Player");
    }

    // 한 번 시작하면 마지막 공격지점까지 멈추지 않는 연속 등속 돌진
    private IEnumerator Skill2Rush(float direction, float rushStart, float rushDuration)
    {
        while (Time.time - s2StartTime < rushStart) yield return null;
        float endTime = rushStart + rushDuration;
        while (skillState == SkillState.Skill2 && Time.time - s2StartTime < endTime)
        {
            if (playerRb != null) 
            {
                playerRb.linearVelocity = new Vector2(direction * s2RushSpeed, playerRb.linearVelocity.y);
            }
            yield return null;
        }
        StopRush();
    }

    private IEnumerator Skill2IndicatorSpawner(float[] spawnTime)
    {
        for (int i = 0; i < spawnTime.Length; i++)
        {
            while (Time.time - s2StartTime < spawnTime[i]) yield return null;
            if (skillState != SkillState.Skill2) yield break;
            s2Indicators[i] = Instantiate(qteIndicatorPrefab, s2Targets[i].transform.position, RandomizedEffectRotation(qteIndicatorPrefab.transform.rotation, skills[1].QTERotateRange));
        }
    }

    private void StopRush()
    {
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
    }

    private float EvaluateDamage(SkillTimeDamageGraph graph, float time)
    {
        for (int i = 1; i < graph.damageGraph.Length; i++)
        {
            if (time < graph.damageGraph[i].time)
            {
                SkillTimeDamageGraph.DamageGraph previous = graph.damageGraph[i - 1];
                SkillTimeDamageGraph.DamageGraph next = graph.damageGraph[i];
                return Mathf.Lerp(previous.damage, next.damage, (time - previous.time) / (next.time - previous.time));
            }
        }
        return 0f;
    }
    
    private IEnumerator SkillDamageCal()
    {
        elapsedTime = 0f;
        while (elapsedTime < graph.damageGraph[graph.damageGraph.Length - 1].time)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime < graph.damageGraph[0].time)
            {
                skillDamage = 0;
            }
            else if (elapsedTime < graph.damageGraph[1].time)
            {
                skillDamage = Mathf.RoundToInt(Mathf.Lerp(graph.damageGraph[0].damage, graph.damageGraph[1].damage, (elapsedTime - graph.damageGraph[0].time) / (graph.damageGraph[1].time - graph.damageGraph[0].time)));
            }
            else if (elapsedTime < graph.damageGraph[2].time)
            {
                skillDamage = Mathf.RoundToInt(Mathf.Lerp(graph.damageGraph[1].damage, graph.damageGraph[2].damage, (elapsedTime - graph.damageGraph[1].time) / (graph.damageGraph[2].time - graph.damageGraph[1].time)));
            }
            else if (elapsedTime < graph.damageGraph[3].time)
            {
                skillDamage = Mathf.RoundToInt(Mathf.Lerp(graph.damageGraph[2].damage, graph.damageGraph[3].damage, (elapsedTime - graph.damageGraph[2].time) / (graph.damageGraph[3].time - graph.damageGraph[2].time)));
            }
            else if (elapsedTime < graph.damageGraph[4].time)
            {
                skillDamage = Mathf.RoundToInt(Mathf.Lerp(graph.damageGraph[3].damage, graph.damageGraph[4].damage, (elapsedTime - graph.damageGraph[3].time) / (graph.damageGraph[4].time - graph.damageGraph[3].time)));
            }
            else if (elapsedTime < graph.damageGraph[5].time)
            {
                skillDamage = Mathf.RoundToInt(Mathf.Lerp(graph.damageGraph[4].damage, graph.damageGraph[5].damage, (elapsedTime - graph.damageGraph[4].time) / (graph.damageGraph[5].time - graph.damageGraph[4].time)));
            }
            else
            {
                skillDamage = 0;
            }
            yield return null;
        }
    }
    private IEnumerator Cooldown(int skillIndex)
    {
        skills[skillIndex].isCooldown = true;
        yield return new WaitForSeconds(skills[skillIndex].cooldown);
        skills[skillIndex].isCooldown = false;
        UIManager.instance?.SetSkillImage(skillIndex, true); // 스킬 사용 가능 이미지
    }

    private IEnumerator Skill3()
    {
        playerMovement?.BeginSkillUsing();
        currentDecoyHitbox = Instantiate(DropingHitboxPrefab, DropingHitboxSpawnPoint.transform.position, Quaternion.identity).GetComponent<DecoyHitbox>(); // 뒤로 빠지기 전, 원래 자리에 미끼 히트박스를 드롭
        playerRb.linearVelocity = new Vector2(-transform.localScale.x * backStepSpeed, backStepJumpForce);
        currentQteIndicator = Instantiate(qteIndicatorPrefab, qteSpawnPoint.transform.position, RandomizedEffectRotation(qteIndicatorPrefab.transform.rotation, skills[2].QTERotateRange));
        yield return new WaitForSeconds(backStepDuration);
        playerRb.linearVelocity = new Vector2(0f, playerRb.linearVelocity.y);

        // 제한시간 내 QTE 입력 없으면 실패 처리
        yield return new WaitForSeconds(graph.damageGraph[graph.damageGraph.Length - 1].time);
        if (skillState == SkillState.Skill3)
        {
            Debug.Log("Skill3 QTE Failed");
            skillState = SkillState.Idle;
            playerMovement?.EndSkillUsing();
            playerInput.SwitchCurrentActionMap("Player");
            if (currentDecoyHitbox != null)
            {
                Destroy(currentDecoyHitbox.gameObject);
                currentDecoyHitbox = null;
            }
        }
    }
}
