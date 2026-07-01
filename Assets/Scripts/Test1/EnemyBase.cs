using UnityEngine;
using System.Collections;

public enum EnemyState
{
    Idle,
    Patrol,
    Battle,
}

[System.Serializable] //인스펙터에서 잘 보이게?
public struct Waypoint
{
    public Transform transform;
    public float waitTime;
}

public class EnemyBase : CharacterBase
{
    [Header("EnemyBase")]
    [SerializeField] protected float detectionRange;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected Waypoint[] waypoints;
    [SerializeField] protected bool patrolLoop = true;

    protected EnemyState currentState = EnemyState.Patrol;
    protected Animator anim;
    [SerializeField] protected Transform playerTransform;
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
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        if (playerTransform != null)
            distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

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
            anim.SetBool("isMoving", true);
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
        MoveToTarget(waypoint, MoveSpeed);

        if (Vector2.Distance(transform.position, waypoint.position) < 0.1f)
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
