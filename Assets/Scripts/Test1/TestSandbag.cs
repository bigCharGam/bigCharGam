using UnityEngine;

public class TestSandbag : CharacterBase
{
    private void Reset()
    {
        maxHealth = 100f;
    }
    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
        Debug.Log("Sandbag 시작!.");
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("Sandbag 남은 체력 : " + currentHealth);
        if (currentHealth <= 0)
        {
            Debug.Log("Sandbag 사망!.");
        }
    }
}
