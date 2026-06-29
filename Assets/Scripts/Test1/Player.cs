using UnityEngine;

public class Player : CharacterBase
{
    [Header("PlayerStats")]
    public float maxStamina;
    public float currentStamina;
    public float staminaRegenRate;
    public float staminaRegenDelay;

    private void Reset()
    {
        maxHealth = 100f;
        MoveSpeed = 10f;
        baseAttackPower = 10f;
        maxStamina = 100f;
        staminaRegenRate = 5f;
        staminaRegenDelay = 2f;
    }

    protected override void Start()
    {
        base.Start();
        currentStamina = maxStamina;
    }
}
