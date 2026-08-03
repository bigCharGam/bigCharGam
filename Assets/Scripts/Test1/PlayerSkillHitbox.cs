using UnityEngine;

public class PlayerSkillHitbox : MonoBehaviour
{
    public float damage;
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        EnemyBase enemy = col.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"Hit Enemy: {enemy.name}, Damage: {damage}");
        }
        Destroy(gameObject);
    }
}
