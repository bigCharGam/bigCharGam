using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [Header("Exp")]
    public int exp = 0;
    public int expLevel = 0;
    public int skillPoint = 0;
    public int[] expTable =
    {
        100,
105,
110,
116,
122,
128,
134,
141,
155,
163,
171,
180,
    };
    [SerializeField] private float expLevelUpDelay = 0.5f;

    private int pendingExp;
    private bool isProcessingExp;
    
    public GameObject midBossPrefab; 
    public GameObject midBossSpawnPoint;
    public GameObject bossHPBar;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SpawnMidBoss()
    {
        // 중간보스는 좌우반전해서 스폰해야함
        Instantiate(midBossPrefab, midBossSpawnPoint.transform.position, Quaternion.Euler(0, 180, 0));
        bossHPBar.SetActive(true);
    }

    public void AddExp(int amount)
    {
        if (amount <= 0 || expTable.Length == 0)
            return;

        pendingExp += amount;

        if (!isProcessingExp)
            StartCoroutine(ProcessExp());
    }

    private IEnumerator ProcessExp()
    {
        isProcessingExp = true;

        while (pendingExp > 0)
        {
            if (expLevel >= expTable.Length)
            {
                exp = expTable[expTable.Length - 1];
                pendingExp = 0;
                UIManager.instance.UpdateExp(exp, expTable[expTable.Length - 1]);
                break;
            }

            int requiredExp = expTable[expLevel];
            int expToAdd = Mathf.Min(pendingExp, requiredExp - exp);
            exp += expToAdd;
            pendingExp -= expToAdd;
            UIManager.instance.UpdateExp(exp, requiredExp);

            yield return UIManager.instance.WaitForExpBar();

            if (exp < requiredExp)
                break;

            yield return new WaitForSeconds(expLevelUpDelay);

            exp = 0;
            expLevel++;
            skillPoint++;
            UIManager.instance.ResetExpBar();
        }

        isProcessingExp = false;
    }
}
