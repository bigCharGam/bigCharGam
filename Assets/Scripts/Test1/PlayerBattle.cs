using Unity.VisualScripting;
using UnityEngine;

public class PlayerBattle : PlayerStats
{
    [Header("Battle Status")]
    public bool isParrying = false;

    // [수정] 중복 선언 방지를 위해 battleAnimator 변수 제거
    // 대신 Animator가 필요한 시점에 GetComponent로 가져오거나 
    // PlayerMovement의 animator 변수를 활용합니다.

    protected override void Start()
    {
        base.Start();
        // Start에서 컴포넌트를 가져오지 않고, 필요할 때마다 가져오거나
        // 상속 구조를 활용해 PlayerMovement의 animator를 공유합니다.
    }

    public void TakeDamage(float damage)
    {
        if (isParrying)
        {
            Debug.Log("패링 성공! 데미지를 무효화합니다.");
            return;
        }

        currentHealth -= damage;
        Debug.Log("Player 남은 체력 : " + currentHealth);

        // [수정] PlayerMovement에 있는 animator를 GetComponent로 즉시 찾아 사용합니다.
        // 이렇게 하면 참조 오류를 방지할 수 있습니다.
        Animator animator = GetComponent<Animator>();

        if (currentHealth <= 0)
        {
            Debug.Log("Player 사망!");
            if (animator != null)
            {
                animator.SetTrigger("isDead");
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetTrigger("isHit");
            }
        }
    }
}