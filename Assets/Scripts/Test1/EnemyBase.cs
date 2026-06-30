using UnityEngine;

public class EnemyBase : CharacterBase
{
    [Header("EnemyStats")]
    protected float test1;

    [SerializeField] protected float detectionRange;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected Transform playerTransform;
}
