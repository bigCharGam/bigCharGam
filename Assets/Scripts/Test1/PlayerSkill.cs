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
    [SerializeField] SkillTimeDamageGraph s1TDGraph;

    private List<EnemyBase> detectedEnemies = new List<EnemyBase>();
    private EnemyBase closestEnemy;
    private QTEIndicator currentQteIndicator;
    private int QTEStack = 0;
    private int S1Damage;

    //effect test
    public GameObject s1Effect;
    public GameObject s1EffectPerfect;
    public GameObject skill1EffectSpawnPoint;
    private float s1Time = 0f;
    private bool s1Waiting = false;
    
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerStats = GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnSkill1()
    {
        if (skills[0].isCooldown || s1Waiting) return; // 스킬 쿨타임 중이거나 QTE 진행 중이면 무시
        if (playerStats == null || playerStats.currentStamina < skills[0].usingStamina) return;

        playerStats.currentStamina -= skills[0].usingStamina;
        UIManager.instance?.UpdatePlayerMP(playerStats.currentStamina, playerStats.maxStamina);
        UIManager.instance?.SetSkillImage(0, false); // 스킬 쿨타임 도는 이미지

        playerInput.SwitchCurrentActionMap("OnSkill");
        s1Waiting = true;

        StartCoroutine(Skill1());
        StartCoroutine(Skill1Damage());
        StartCoroutine(Cooldown(0));
    }
    private void OnQTE()
    {
        if (s1Time < s1TDGraph.damageGraph[0].time) return; // QTE 입력이 너무 빠른 경우 무시
        playerInput.SwitchCurrentActionMap("Player");
        Debug.Log(s1Time);

        currentQteIndicator?.SkipToStage4(); // 버튼 누르면 페이드아웃

        if (s1Time > s1TDGraph.damageGraph[2].time && s1Time < s1TDGraph.damageGraph[3].time) 
        {
            Instantiate(s1EffectPerfect, skill1EffectSpawnPoint.transform.position, s1EffectPerfect.transform.rotation);
        }
        else
        {
            Instantiate(s1Effect, skill1EffectSpawnPoint.transform.position, s1Effect.transform.rotation);
        }
        if (closestEnemy == null)
        {
            s1Waiting = false;
            return;
        }

        PlayerSkillHitbox skillHitbox = Instantiate(skills[0].hitboxAttack, closestEnemy.transform.position, Quaternion.identity).GetComponent<PlayerSkillHitbox>();
        skillHitbox.damage = S1Damage * skills[0].damageMultiplier; // 물리 시뮬레이션보다 대입이 빨라서 히트박스가 충분히 damage를 받음
        s1Waiting = false;
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
