using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public EnemyBase enemyPrefab;
    public Transform spawnPoint;
    public Waypoint[] waypoints;
}

public class AreaSpawner : MonoBehaviour
{
    [Header("AreaSpawner")]
    [SerializeField] private EnemySpawnData[] enemySpawnDataArray;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SpawnEnemies();
            gameObject.SetActive(false);
        }
    }

    private void SpawnEnemies()
    {
        foreach (EnemySpawnData spawnData in enemySpawnDataArray)
        {
            if (spawnData.enemyPrefab != null && spawnData.spawnPoint != null)
            {
                EnemyBase spawnedEnemy = Instantiate(spawnData.enemyPrefab, spawnData.spawnPoint.position, Quaternion.identity);
                if (spawnData.waypoints != null && spawnData.waypoints.Length > 0)
                {
                    spawnedEnemy.SetWaypoints(spawnData.waypoints);
                }
            }
        }
    }
}
