using UnityEngine;

public class PlayerBattle : PlayerStats
{
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("Player 남은 체력 : " + currentHealth);
        if (currentHealth <= 0)
        {
            Debug.Log("Player 사망!.");
        }
    }
}
