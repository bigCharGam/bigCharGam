using UnityEngine;
using System.Collections;

public enum EnemyState
{
    Patrol, // Waypoints 따라 순찰 상태
    Idle, // 모든 Waypoint 순찰 후 대기 상태 (순찰 반복이 false인 경우)
    Battle, // 감지범위 내 플레이어 발견 시 전투 상태
}

// 순찰 지점: 위치와 도착 후 대기 시간
[System.Serializable]
public struct Waypoint
{
    public Transform transform;
    public float waitTime;
}

public class EnemyBase : CharacterBaseStats

{
    [Header("EnemyBase")]
    [SerializeField] protected float detectionRange;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected Waypoint[] waypoints;
    [SerializeField] protected bool patrolLoop = true;
    [SerializeField] protected Transform playerTransform;

    protected EnemyState currentState = EnemyState.Patrol;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected float distanceToPlayer;
    private int currentWaypointIndex = 0;
    private bool isWating = false;

    protected void MoveToTarget(Transform target, float moveSpeed)
    {
        if (target == null) return;
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);
    }

    override protected void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {
        if (playerTransform != null)
            distanceToPlayer = Mathf.Abs(transform.position.x - playerTransform.position.x);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Patrol:
                HandlePatrol();
                break;
            case EnemyState.Battle:
                HandleBattle();
                break;
        }
    }

    private bool PlayerDetect()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (player != null)
        {
            currentState = EnemyState.Battle;
            anim.SetBool("isMoving", true); //추후 걷기 뛰기 모션 분리
        }
        return player != null;
    }

    protected virtual void HandleIdle()
    {
        if (PlayerDetect()) return;
    }

    private void HandlePatrol()
    {
        if (waypoints.Length == 0)
        {
            currentState = EnemyState.Idle;
            return;
        }
        if (isWating) return;

        if (PlayerDetect()) return;

        Transform waypoint = waypoints[currentWaypointIndex].transform;
        MoveToTarget(waypoint, moveSpeed);

        if (Mathf.Abs(transform.position.x - waypoint.position.x) < 0.1f)
        {
            isWating = true;
            StartCoroutine(WaitAtWaypoint(waypoints[currentWaypointIndex].waitTime));

            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                if (patrolLoop)
                {
                    currentWaypointIndex = 0;
                }
                else
                {
                    currentState = EnemyState.Idle;
                }
            }
        }
    }
    IEnumerator WaitAtWaypoint(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        isWating = false;
    }

    protected virtual void HandleBattle()
    {

    }
}
