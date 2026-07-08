using System.Collections;
using UnityEditor;
using UnityEngine;

// EnemyBase를 상속받아 타격 판정과 체력 시스템을 그대로 물려받습니다.
public class BreakableTree : EnemyBase
{
    [Header("Tree Settings")]
    public Sprite brokenSprite; // 부서진 나무 이미지

    private SpriteRenderer spriteRenderer;
    private Collider2D treeCollider;

    protected override void Start()
    {
        // base.Start()를 호출하지 않기
        // 나무는 Animator나 Rigidbody가 필요 없고, 플레이어를 추적할 필요도 없기 때문

        spriteRenderer = GetComponent<SpriteRenderer>();
        treeCollider = GetComponent<Collider2D>();

        // CharacterBaseStats에 정의된 체력 변수(currentHealth)를 1로 고정하여 한 방에 부서지게 설정
        currentHealth = 1f;
    }

    protected override void Update()
    {
        // base.Update()를 지움으로써 나무가 Patrol, Idle, Battle 상태를 오가며 
        // 움직이거나 NullReference 에러를 뿜는 것을 완벽하게 차단합니다.
    }

    // EnemyBase의 DieCoroutine을 나무만의 방식으로 덮어씁니다(Override)
    protected override IEnumerator DieCoroutine()
    {
        // 몬스터처럼 파괴(Destroy)되지 않고, 이미지 교체와 물리 충돌만 꺼줍니다.
        if (brokenSprite != null)
        {
            spriteRenderer.sprite = brokenSprite;
        }

        if (treeCollider != null)
        {
            treeCollider.enabled = false;
        }

        // 추가적인 연출(먼지 파티클 생성, 효과음 재생 등)을 여기에 넣을 수 있습니다.
        Debug.Log("나무가 공격받아 부서졌습니다!");

        yield return null; // 코루틴 형식 유지를 위한 빈 반환
    }

    #if UNITY_EDITOR

// BreakableTree 클래스 전용 인스펙터를 새로 그리겠다는 선언입니다.
[CustomEditor(typeof(BreakableTree))]
public class BreakableTreeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 인스펙터에서 숨기고 싶은 부모 클래스의 '변수명'들을 적기
        // (CharacterBaseStats나 EnemyBase에 선언된 실제 변수명을 적으시면 됩니다)
        string[] hiddenFields = new string[]
        {
            "detectionRange",
            "playerLayer",
            "waypoints",
            "patrolLoop",
            // 아래는 CharacterBaseStats에 있을 법한 변수명 예시입니다. 실제 이름에 맞게 수정하세요.
            "baseAttackPower",
            "moveSpeed",

        };

        // 숨길 변수들(hiddenFields)만 제외하고 기본 인스펙터를 정상적으로 그려라!
        DrawPropertiesExcluding(serializedObject, hiddenFields);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
}

