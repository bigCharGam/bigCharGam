using UnityEngine;

public class TestEnemy1 : EnemyBase
{
    private void Reset()
    {
        maxHealth = 150f;
        moveSpeed = 8f;
        baseAttackPower = 12f;
    }

    protected override void Start()
    {
        base.Start();
    }
}
