using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (BattleManager.instance != null)
                BattleManager.instance.SpawnMidBoss();
            gameObject.SetActive(false);
        }
    }
}
