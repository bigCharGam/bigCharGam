using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    
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
}
