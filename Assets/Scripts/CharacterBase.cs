using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    [Header("BaseStats")]
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float MoveSpeed;
    [SerializeField] protected float baseAttackPower;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        
    }
}
