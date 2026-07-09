using UnityEngine;
using System.Collections;

public class EnemyAttackHitbox : MonoBehaviour
{
    public float damage;
    public bool isParryable = false;
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            col.GetComponent<PlayerBattle>().TakeDamage(damage);
        }
        //나중에 경직 코드 추가할것
    }
}
