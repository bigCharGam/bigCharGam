using UnityEngine;
using System.Collections;

public enum EnemyState
{
    Patrol, // Waypoints 따라 순찰 상태
    Idle, // 모든 Waypoint 순찰 후 대기 상태 (순찰 반복이 false인 경우)
    Battle, // 감지범위 내 플레이어 발견 시 전투 상태
}

// 순찰 지점 구조체 { 위치, 도착 후 대기 시간 }
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
    [SerializeField] protected bool patrolLoop = true; // 순찰 반복 여부
    protected Transform playerTransform;

    protected EnemyState currentState = EnemyState.Patrol;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected float distanceToPlayer;
    private int currentWaypointIndex = 0;
    private bool isWating = false;

    protected bool isDead = false;

    // 목표 위치로 이동하는 함수
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

        // 플레이어랑 씬이 달라 인스펙터에서 드래그해서 넣을 수 없고 코드로 찾아야함
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    protected override void Update()
    {
        if (isDead) return;

        // 임시) 항상 플레이어와 거리재기
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

    // Patrol, Idle 상태에서 플레이어 감지 함수
    private bool PlayerDetect()
    {
        // 추후 앞뒤 다르게
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (player != null)
        {
            currentState = EnemyState.Battle;
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
        anim.SetInteger("moveLevel", 1); 

        if (Mathf.Abs(transform.position.x - waypoint.position.x) < 0.1f)
        {
            isWating = true;
            anim.SetInteger("moveLevel", 0);
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
        anim.SetInteger("moveLevel", 1);
    }

    protected virtual void HandleBattle()
    {

    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(DieCoroutine());
        }
    }

    protected IEnumerator DieCoroutine()
    {
        anim.SetTrigger("die");
        
        yield return new WaitForSeconds(0.8f); //죽고나서 물리판정 지속시간
        rb.simulated = false;
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
