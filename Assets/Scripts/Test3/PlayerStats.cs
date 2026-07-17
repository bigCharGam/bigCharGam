using UnityEngine;

public class PlayerStats : CharacterBaseStats
{
    [Header("PlayerStats")]
    public float maxStamina;
    public float currentStamina;
    public float staminaRegenRate;
    public float staminaRegenDelay;

    private void Reset()        // 함수) Reset, 인스펙터 창에서 기본 값 자동 입력
    {
        maxHealth = 100f;
        moveSpeed = 10f;
        baseAttackPower = 10f;
        maxStamina = 100f;
        staminaRegenRate = 5f;
        staminaRegenDelay = 2f;
    }

    protected override void Start()     // 키워드) 오버라이드, 부모 내용 변경 및 추가
    {
        base.Start();       /// [기능 요약] 체력 초기화
        currentStamina = maxStamina;
    }

    protected override void Update()
    {

    }
   
}
