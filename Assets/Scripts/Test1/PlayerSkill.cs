using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct skillData
{
    public string skillName;
    public GameObject hitboxDetection;
    public GameObject hitboxAttack;
    public float damageMultiplier;
    public float cooldown;
    public bool isCooldown;
    public float usingStamina;
}

public struct DamageGraph
{
    public float time;
    public float damage;
}

public class PlayerSkill : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerStats playerStats;
    [SerializeField] private skillData[] skills;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private QTEIndicator qteIndicatorPrefab;

    [Header("Skill1")]
    [SerializeField] private SkillTimeDamageGraph s1TDGraph;

    private List<EnemyBase> detectedEnemies = new List<EnemyBase>();
    private EnemyBase closestEnemy;
    private QTEIndicator currentQteIndicator;
    private int QTEStack = 0;
    private int S1Damage;

    [Header("Skill2")]
    [SerializeField] private float s2MinDistance;
    [SerializeField] private float s2RushSpeed = 10f;
    [SerializeField] private float s2AttackDistance = 1f; // 적 앞에서 멈추는 거리
    [SerializeField] private SkillTimeDamageGraph s2TDGraph;

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
    
    private float s1Time = 0f;
    private bool s1Waiting = false;
    private bool s2Waiting = false;
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
    private SkillTimeDamageGraph Skill2Graph => s2TDGraph != null ? s2TDGraph : s1TDGraph;
    private PlayerMovement playerMovement;
    
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
        if (skills[index].isCooldown || s1Waiting || s2Waiting) return false; // 스킬 쿨타임 중이거나 QTE 진행 중이면 무시
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


        s1Waiting = true;
        StartCoroutine(Skill1());
        StartCoroutine(Skill1Damage());
        StartCoroutine(Cooldown(0));
    }
    private void OnSkill2()
    {
        if (!SkillStarter(1)) return;

        s2Waiting = true;
        StartCoroutine(Skill2());
        StartCoroutine(Cooldown(1));
    }
    private void OnSkill3()
    {
        SkillStarter(2);
    }
    private void OnQTE()
    {
        if (s2Waiting)
        {
            OnSkill2QTE();
            return;
        }
        if (s1Time < s1TDGraph.damageGraph[0].time) return; // QTE 입력이 너무 빠른 경우 무시
        playerInput.SwitchCurrentActionMap("Player");
        Debug.Log(s1Time);

        currentQteIndicator?.SkipToStage4(); // 버튼 누르면 페이드아웃

        // effect
        if (s1Time > s1TDGraph.damageGraph[2].time && s1Time < s1TDGraph.damageGraph[3].time) 
        {
            Instantiate(s1EffectPerfect, s1EffectSpawnPoint.transform.position, s1EffectPerfect.transform.rotation);
        }
        else
        {
            Instantiate(s1Effect, s1EffectSpawnPoint.transform.position, s1Effect.transform.rotation);
        }

        if (closestEnemy == null)
        {
            s1Waiting = false;
            playerMovement?.EndSkillUsing();
            return;
        }

        PlayerSkillHitbox skillHitbox = Instantiate(skills[0].hitboxAttack, closestEnemy.transform.position, Quaternion.identity).GetComponent<PlayerSkillHitbox>();
        skillHitbox.damage = S1Damage * skills[0].damageMultiplier; // 물리 시뮬레이션보다 대입이 빨라서 히트박스가 충분히 damage를 받음
        s1Waiting = false;
        playerMovement?.EndSkillUsing();
    }

    private void OnSkill2QTE()
    {
        if (s2Resolved || s2TargetIndex >= s2Targets.Count) return;
        float elapsed = Time.time - s2StartTime;
        if (elapsed < s2EarliestInput) return; // 너무 이른 입력 무시

        bool isPerfect = elapsed >= s2PerfectStart && elapsed <= s2PerfectEnd;
        EnemyBase target = s2Targets[s2TargetIndex];
        s2Indicators[s2TargetIndex]?.SkipToStage4();

        PlayerSkillHitbox skillHitbox = Instantiate(skills[1].hitboxAttack, target.transform.position, Quaternion.identity).GetComponent<PlayerSkillHitbox>();
        skillHitbox.damage = EvaluateDamage(Skill2Graph, elapsed - s2CurrentSpawnTime) * skills[1].damageMultiplier;
        s2Resolved = true;

        // effect
        if (s1Time > s1TDGraph.damageGraph[2].time && s1Time < s1TDGraph.damageGraph[3].time) 
        {
            Instantiate(s1EffectPerfect, s1EffectSpawnPoint.transform.position, s1EffectPerfect.transform.rotation);
        }
        else
        {
            Instantiate(s1Effect, s1EffectSpawnPoint.transform.position, s1Effect.transform.rotation);
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
            s2Waiting = false;
            playerMovement?.EndSkillUsing();
            playerInput.SwitchCurrentActionMap("Player");
            yield break;
        }

        int n = s2Targets.Count;
        float indicatorLead = QTEIndicator.ComputeLeadTime(Skill2Graph); // 스폰~퍼펙트까지 걸리는 고정 시간
        float perfectWindow = QTEIndicator.ComputePerfectWindow(Skill2Graph);
        float earlyMargin = Skill2Graph.damageGraph[0].time;

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
        s2Waiting = false;
        playerMovement?.EndSkillUsing();
        playerInput.SwitchCurrentActionMap("Player");
    }

    // 한 번 시작하면 마지막 공격지점까지 멈추지 않는 연속 등속 돌진
    private IEnumerator Skill2Rush(float direction, float rushStart, float rushDuration)
    {
        while (Time.time - s2StartTime < rushStart) yield return null;
        float endTime = rushStart + rushDuration;
        while (s2Waiting && Time.time - s2StartTime < endTime)
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
            if (!s2Waiting) yield break;
            s2Indicators[i] = Instantiate(qteIndicatorPrefab, s2Targets[i].transform.position, Quaternion.identity);
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

            currentQteIndicator = Instantiate(qteIndicatorPrefab, closestEnemy.transform.position, Quaternion.identity);
        }

        // 제한시간 내 QTE 입력 없으면 실패 처리
        yield return new WaitForSeconds(s1TDGraph.damageGraph[s1TDGraph.damageGraph.Length - 1].time);
        if (s1Waiting)
        {
            Debug.Log("Skill1 QTE Failed");
            s1Waiting = false;
            playerMovement?.EndSkillUsing();
            playerInput.SwitchCurrentActionMap("Player");
        }
    }
    private IEnumerator Skill1Damage()
    {
        s1Time = 0f;
        while (s1Time < s1TDGraph.damageGraph[s1TDGraph.damageGraph.Length - 1].time)
        {
            s1Time += Time.deltaTime;
            if (s1Time < s1TDGraph.damageGraph[0].time)
            {
                S1Damage = 0;
            }
            else if (s1Time < s1TDGraph.damageGraph[1].time)
            {
                S1Damage = Mathf.RoundToInt(Mathf.Lerp(s1TDGraph.damageGraph[0].damage, s1TDGraph.damageGraph[1].damage, (s1Time - s1TDGraph.damageGraph[0].time) / (s1TDGraph.damageGraph[1].time - s1TDGraph.damageGraph[0].time)));
            }
            else if (s1Time < s1TDGraph.damageGraph[2].time)
            {
                S1Damage = Mathf.RoundToInt(Mathf.Lerp(s1TDGraph.damageGraph[1].damage, s1TDGraph.damageGraph[2].damage, (s1Time - s1TDGraph.damageGraph[1].time) / (s1TDGraph.damageGraph[2].time - s1TDGraph.damageGraph[1].time)));
            }
            else if (s1Time < s1TDGraph.damageGraph[3].time)
            {
                S1Damage = Mathf.RoundToInt(Mathf.Lerp(s1TDGraph.damageGraph[2].damage, s1TDGraph.damageGraph[3].damage, (s1Time - s1TDGraph.damageGraph[2].time) / (s1TDGraph.damageGraph[3].time - s1TDGraph.damageGraph[2].time)));
            }
            else if (s1Time < s1TDGraph.damageGraph[4].time)
            {
                S1Damage = Mathf.RoundToInt(Mathf.Lerp(s1TDGraph.damageGraph[3].damage, s1TDGraph.damageGraph[4].damage, (s1Time - s1TDGraph.damageGraph[3].time) / (s1TDGraph.damageGraph[4].time - s1TDGraph.damageGraph[3].time)));
            }
            else if (s1Time < s1TDGraph.damageGraph[5].time)
            {
                S1Damage = Mathf.RoundToInt(Mathf.Lerp(s1TDGraph.damageGraph[4].damage, s1TDGraph.damageGraph[5].damage, (s1Time - s1TDGraph.damageGraph[4].time) / (s1TDGraph.damageGraph[5].time - s1TDGraph.damageGraph[4].time)));
            }
            else
            {
                S1Damage = 0;
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
}
