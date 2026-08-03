using UnityEngine;
using UnityEngine.UI;

public class BossHP : MonoBehaviour
{
    [Header("보스 대상 정보")]
    [SerializeField] private GameObject bossObject; // 대상 보스 오브젝트 (미지정 시 자기 자신 자동 할당)

    [Header("체력 설정")]
    [SerializeField] private float maxHealth = 100f; // 최대 체력
    [SerializeField] private float currentHealth;   // 현재 체력

    [Header("UI 생성 설정")]
    [SerializeField] private GameObject hpSliderPrefab; // 생성할 HP바 Slider 프리팹
    private Slider hpSlider;

    private void Awake()
    {
        // 보스 대상을 인스펙터에서 직접 지정하지 않았다면, 이 스크립트가 붙은 오브젝트를 보스로 설정
        if (bossObject == null)
        {
            bossObject = this.gameObject;
        }

        // 씬 내의 Canvas 오브젝트 탐색
        Canvas targetCanvas = FindFirstObjectByType<Canvas>();

        if (targetCanvas != null && hpSliderPrefab != null)
        {
            // Canvas 하위에 HP바 UI 생성
            GameObject uiObject = Instantiate(hpSliderPrefab, targetCanvas.transform);

            // 생성된 UI를 Canvas 자식(Sibling) 목록의 맨 마지막으로 이동시켜 화면 최상단에 레이어링
            uiObject.transform.SetAsLastSibling();

            hpSlider = uiObject.GetComponent<Slider>();
        }
        else if (targetCanvas == null)
        {
            Debug.Log("BossHP: 씬 내에 Canvas가 존재하지 않습니다.");
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
    }

    private void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHPUI();

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

        // 보스 사망 시 생성했던 UI 제거
        if (hpSlider != null)
        {
            Destroy(hpSlider.gameObject);
        }

        // 대상 보스 오브젝트 파괴
        if (bossObject != null)
        {
            Destroy(bossObject);
        }
    }
}