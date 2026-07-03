using Unity.VisualScripting;
using UnityEngine;

public class PlayerBattle : PlayerStats
{
    [Header("Battle Status")]
    // 패링 중인지 여부를 저장하는 전역 변수 (true일 때 패링 성공 가능)
    public bool isParrying = false;

    public void TakeDamage(float damage)
    {
        // 패링 중일 경우 데미지를 입지 않고 리턴 (함수 종료)
        if (isParrying)
        {
            Debug.Log("패링 성공! 데미지를 무효화합니다.");

            // 여기에 패링 성공 이펙트나 사운드, 혹은 적 경직 신호를 넣으면 됩니다.

            return;
        }

        // 패링 중이 아닐 때만 데미지 계산
        currentHealth -= damage;
        Debug.Log("Player 남은 체력 : " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Player 사망!.");
        }
    }
}