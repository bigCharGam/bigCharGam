using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BossHitbox : MonoBehaviour
{
    [Header("보스 대상 정보")]
    [SerializeField] private GameObject bossObject; // 대상 보스 오브젝트 (미지정 시 자기 자신 자동 할당)

    [Header("체력 설정")]
    [SerializeField] private float maxHealth = 100f; // 최대 체력
    [SerializeField] private float currentHealth;   // 현재 체력

    [Header("전용 피격 박스(Hitbox) 설정")]
    [SerializeField] private Vector2 hitboxSize = new Vector2(3f, 3f); // 인스펙터에서 조절할 전용 히트박스 크기
    [SerializeField] private Vector2 hitboxOffset = Vector2.zero;     // 히트박스 오프셋 위치
    [SerializeField] private LayerMask playerLayer;                    // 플레이어 레이어 판정

    [Header("피격 및 데미지 설정")]
    [SerializeField] private float normalAttackDamage = 10f; // 기본 공격(Space / 좌클릭) 데미지
    [SerializeField] private float bigAttackDamage = 25f;    // 강공격(Ctrl / 우클릭) 데미지

    [Header("UI 생성 설정")]
    [SerializeField] private GameObject hpSliderPrefab; // 생성할 HP바 Slider 프리팹
    private Slider hpSlider;

    private bool isPlayerInHitbox = false;
    private PlayerInput playerInput;
    private BoxCollider2D generatedHitboxCollider;

    private void Awake()
    {
        if (bossObject == null)
        {
            bossObject = this.gameObject;
        }

        // 1. 전용 히트박스 자식 오브젝트 생성 및 콜라이더 설정
        CreateCustomHitbox();

        // 2. UI 생성 및 최상단 레이어링
        Canvas targetCanvas = FindFirstObjectByType<Canvas>();

        if (targetCanvas != null && hpSliderPrefab != null)
        {
            GameObject uiObject = Instantiate(hpSliderPrefab, targetCanvas.transform);
            uiObject.transform.SetAsLastSibling();
            hpSlider = uiObject.GetComponent<Slider>();
        }
        else if (targetCanvas == null)
        {
            Debug.Log("BossHitbox: 씬 내에 Canvas가 존재하지 않습니다.");
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHPUI();
    }

    private void OnValidate()
    {
        if (hpSlider != null)
        {
            UpdateHPUI();
        }

        // 에디터 수정 시 자식 히트박스 크기 즉시 반영
        if (generatedHitboxCollider != null)
        {
            generatedHitboxCollider.size = hitboxSize;
            generatedHitboxCollider.offset = hitboxOffset;
        }
    }

    // 전용 히트박스 자식 오브젝트 생성 메서드
    private void CreateCustomHitbox()
    {
        GameObject hitboxObj = new GameObject("Boss_Custom_Hitbox");
        hitboxObj.transform.SetParent(transform);
        hitboxObj.transform.localPosition = Vector3.zero;

        // 히트박스 감지기 컴포넌트 추가
        HitboxDetector detector = hitboxObj.AddComponent<HitboxDetector>();
        detector.Init(this);

        // 트리거 콜라이더 설정
        generatedHitboxCollider = hitboxObj.AddComponent<BoxCollider2D>();
        generatedHitboxCollider.isTrigger = true;
        generatedHitboxCollider.size = hitboxSize;
        generatedHitboxCollider.offset = hitboxOffset;
    }

    private void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
    }

    // 히트박스 전용 감지 메서드 (자식 감지기에서 호출)
    public void OnPlayerEnterHitbox(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            isPlayerInHitbox = true;
            playerInput = other.GetComponent<PlayerInput>();

            if (playerInput != null)
            {
                playerInput.actions["Attack"].started += OnPlayerAttack;
                playerInput.actions["BigAttack"].started += OnPlayerBigAttack;
            }
        }
    }

    public void OnPlayerExitHitbox(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            isPlayerInHitbox = false;

            if (playerInput != null)
            {
                playerInput.actions["Attack"].started -= OnPlayerAttack;
                playerInput.actions["BigAttack"].started -= OnPlayerBigAttack;
                playerInput = null;
            }
        }
    }

    private void OnPlayerAttack(InputAction.CallbackContext context)
    {
        if (isPlayerInHitbox)
        {
            TakeDamage(normalAttackDamage);
        }
    }

    private void OnPlayerBigAttack(InputAction.CallbackContext context)
    {
        if (isPlayerInHitbox)
        {
            TakeDamage(bigAttackDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHPUI();

        Debug.Log($"보스 피격! 받은 데미지: {damage}, 남은 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHPUI();
    }

    private void Die()
    {
        Debug.Log("보스가 처치되었습니다.");

        if (playerInput != null)
        {
            playerInput.actions["Attack"].started -= OnPlayerAttack;
            playerInput.actions["BigAttack"].started -= OnPlayerBigAttack;
        }

        if (hpSlider != null)
        {
            Destroy(hpSlider.gameObject);
        }

        if (bossObject != null)
        {
            Destroy(bossObject);
        }
    }

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.actions["Attack"].started -= OnPlayerAttack;
            playerInput.actions["BigAttack"].started -= OnPlayerBigAttack;
        }
    }

    // 씬 뷰에서 전용 피격 박스 영역 가시화 (빨간색)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position + (Vector3)hitboxOffset;
        Gizmos.DrawWireCube(center, hitboxSize);
    }
}

// 자식 히트박스 오브젝트용 충돌 감지 클래스
public class HitboxDetector : MonoBehaviour
{
    private BossHitbox mainScript;

    public void Init(BossHitbox script)
    {
        mainScript = script;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (mainScript != null)
        {
            mainScript.OnPlayerEnterHitbox(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (mainScript != null)
        {
            mainScript.OnPlayerExitHitbox(other);
        }
    }
}