using UnityEngine;

public class CharacterBaseStats : MonoBehaviour
{
    [Header("BaseStats")]
    [SerializeField] protected float maxHealth;
    [SerializeField] public float currentHealth;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float baseAttackPower;

    protected virtual void Start()      // 키워드) virtual, 자식 오버라이드 허용
    {
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        
    }
}
