using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }
    public List<EnemyBase> spawnedEnemies = new List<EnemyBase>();
    public List<AreaSpawner> areaSpawners = new List<AreaSpawner>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DeleteAllEnemies()
    {
        foreach (EnemyBase enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        spawnedEnemies.Clear();
    }
    public void ResetAreaSpawners(int index)
    {
        foreach (var spawner in areaSpawners)
        {
            if (spawner != null)
            {
                spawner.gameObject.SetActive(false);
            }
        }
        for (int i = index + 1; i < areaSpawners.Count; i++)
        {
            if (areaSpawners[i] != null)
            {
                areaSpawners[i].gameObject.SetActive(true);
            }
        }
    }
}
