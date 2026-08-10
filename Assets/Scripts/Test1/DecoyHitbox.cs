using UnityEngine;

// Skill3 백스텝 시 원래 자리에 남겨두는 미끼 히트박스, 적 공격에 맞으면 wasHit이 true가 됨
public class DecoyHitbox : MonoBehaviour
{
    public bool wasHit { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<EnemyAttackHitbox>() != null)
        {
            wasHit = true;
        }
    }
}
