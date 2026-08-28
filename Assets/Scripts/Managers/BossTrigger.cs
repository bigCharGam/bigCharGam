using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public GameObject midBossPrefab; 
    public GameObject midBossSpawnPoint;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (BattleManager.instance != null)
                BattleManager.instance.SpawnMidBoss();
            gameObject.SetActive(false);

            // 중간보스는 좌우반전해서 스폰해야함
            Instantiate(midBossPrefab, midBossSpawnPoint.transform.position, Quaternion.Euler(0, 180, 0));
            BattleManager.instance.SpawnMidBoss();
        }
    }
}
