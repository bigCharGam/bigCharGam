using UnityEngine;
using System.Collections;

public class TestEnemy1 : EnemyBase
{
    [Header("Skill 1 Slash")]
    [SerializeField] private GameObject skill1Hitbox;
    [SerializeField] private float skill1Damage = 10f;

    private Animator anim;
    private void Reset()
    {
        maxHealth = 150f;
        MoveSpeed = 8f;
        baseAttackPower = 12f;
    }

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();

        StartCoroutine(SkillTest());
    }
    public void UseSkill1_Slash()
    {
        anim.SetTrigger("Skill1_Slash");
    }
    private void HitboxEnable_Skill1()
    {
        skill1Hitbox.SetActive(true);
        skill1Hitbox.GetComponent<EnemyAttackHitbox>().damage = skill1Damage;
    }
    private void HitboxDisable_Skill1()
    {
        skill1Hitbox.SetActive(false);
    }
    private IEnumerator SkillTest()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            UseSkill1_Slash();
        }
    }
}
