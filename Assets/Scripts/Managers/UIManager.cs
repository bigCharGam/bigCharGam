using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private Image playerHPInnerbar;
    [SerializeField] private Image playerHPInnerbarPreview;

    [SerializeField] private Image playerMPInnerbar;
    [SerializeField] private Image bossHPInnerbar;
    [SerializeField] private Image expBar;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private float lerpSpeed = 8f;
    [SerializeField] private float lerpSpeedPotion;
    [SerializeField] private float lerpSpeedExp = 3f;


    private float playerHPTarget = 1f;
    private float playerHPPreviewTarget = 1f;
    private bool isPotionHealing = false;
    private float playerMPTarget = 1f;
    private float bossHPTarget = 1f;
    private float expTarget = 0f;

    [Header("Skill")]
    [SerializeField] private int skillsCount;
    [SerializeField] private Image[] notLearnedSkillImage;
    [SerializeField] private Image[] skillOnImage;
    [SerializeField] private Image[] skillOffImage;

    [Header("Potion")]
    [SerializeField] private Image potionImage;
    [SerializeField] private TextMeshProUGUI potionCountText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        RefreshSkillImages();
    }

    public void RefreshSkillImages()
    {
        for (int i = 0; i < skillsCount; i++)
        {
            notLearnedSkillImage[i].gameObject.SetActive(BattleManager.instance.skillLevels[i] == 0);
            skillOnImage[i].gameObject.SetActive(BattleManager.instance.skillLevels[i] > 0);
            skillOffImage[i].gameObject.SetActive(false);
        }
    }

    void Update()
    {
            // 실제 currentHealth 자체가 포션 회복 중에는 매 프레임 조금씩 오르고(PlayerBattle의 코루틴),
            // 피격 시에는 즉시 반영되므로, UI는 그 값을 쫓아가는 속도만 다르게 가져간다.
            // 포션 회복 중: lerpSpeedPotion, 그 외(피격 포함): lerpSpeed
            if (isPotionHealing)
            {
                playerHPInnerbar.fillAmount = Mathf.MoveTowards(playerHPInnerbar.fillAmount, playerHPTarget, lerpSpeedPotion * Time.deltaTime);
            }
            else
            {
                playerHPInnerbar.fillAmount = Mathf.Lerp(playerHPInnerbar.fillAmount, playerHPTarget, lerpSpeed * Time.deltaTime);
            }

            // 힐 예고 바: 회복이 끝나거나 중단되면 자동으로 숨김
            if (playerHPInnerbarPreview != null && playerHPInnerbarPreview.gameObject.activeSelf && !isPotionHealing)
            {
                playerHPInnerbarPreview.gameObject.SetActive(false);
            }

            playerMPInnerbar.fillAmount = Mathf.Lerp(playerMPInnerbar.fillAmount, playerMPTarget, lerpSpeed * Time.deltaTime);
            bossHPInnerbar.fillAmount = Mathf.Lerp(bossHPInnerbar.fillAmount, bossHPTarget, lerpSpeed * Time.deltaTime);
            expBar.fillAmount = Mathf.MoveTowards(expBar.fillAmount, expTarget, lerpSpeedExp * Time.deltaTime);
            expText.text = BattleManager.instance.skillPoint.ToString();
            potionCountText.text = BattleManager.instance.potionCount.ToString();

            // 포션 0개면 이미지 어둡게
            if (BattleManager.instance.potionCount <= 0)
            {
                potionImage.color = new Color(0.5f, 0.5f, 0.5f); // 어둡게
            }
            else
            {
                potionImage.color = Color.white; // 원래 색상
            }
    }

    public void UpdatePlayerHP(float currentHp, float maxHp)
    {
        if (playerHPInnerbar == null) return;

        playerHPTarget = NormalizeRatio(currentHp, maxHp);

        // 피격 등 일반 갱신이 들어오면 진행 중이던 포션 회복은 즉시 중단하고,
        // 이후 Update()에서 lerpSpeed로 이동하도록 한다 (즉시 스냅하지 않음).
        isPotionHealing = false;
    }

    // 포션 회복 시작: 예고 바 표시 + 회복량/시간에 비례한 UI 추적 속도 계산.
    // 실제 currentHealth 증가는 PlayerBattle의 코루틴이 매 프레임 UpdatePlayerHPHealTick으로 갱신한다.
    public void BeginPotionHeal(float currentHp, float maxHp, float healAmount, float duration)
    {
        if (playerHPInnerbar == null) return;

        isPotionHealing = true;

        if (maxHp > 0f && duration > 0f)
        {
            lerpSpeedPotion = (healAmount / maxHp) / duration;
        }

        if (playerHPInnerbarPreview != null)
        {
            playerHPPreviewTarget = NormalizeRatio(currentHp + healAmount, maxHp);
            playerHPInnerbarPreview.fillAmount = playerHPPreviewTarget;
            playerHPInnerbarPreview.gameObject.SetActive(true);
        }
    }

    // 포션 회복 중 매 프레임 호출: 실제로 늘어난 체력만큼 목표치만 갱신 (isPotionHealing 유지)
    public void UpdatePlayerHPHealTick(float currentHp, float maxHp)
    {
        if (playerHPInnerbar == null) return;
        playerHPTarget = NormalizeRatio(currentHp, maxHp);
    }

    public void UpdatePlayerMP(float currentMp, float maxMp)
    {
        if (playerMPInnerbar == null) return;
        playerMPTarget = NormalizeRatio(currentMp, maxMp);
    }

    public void UpdateBossHP(float currentHp, float maxHp)
    {
        if (bossHPInnerbar == null) return;
        bossHPTarget = NormalizeRatio(currentHp, maxHp);
    }
    public void UpdateExp(float currentExp, float maxExp)
    {
        if (expBar == null) return;
        expTarget = NormalizeRatio(currentExp, maxExp);
    }

    public void ResetExpBar()
    {
        if (expBar == null) return;
        expBar.fillAmount = 0f;
        expTarget = 0f;
    }

    public IEnumerator WaitForExpBar()
    {
        if (expBar == null) yield break;

        while (Mathf.Abs(expBar.fillAmount - expTarget) > 0.01f)
            yield return null;

        expBar.fillAmount = expTarget;
    }

    private float NormalizeRatio(float current, float max)
    {
        if (max <= 0f) return 0f;
        return Mathf.Clamp01(current / max);
    }

    public void SetSkillImage(int index, bool on)
    {
        bool learned = BattleManager.instance.skillLevels[index] > 0;
        notLearnedSkillImage[index].gameObject.SetActive(!learned);
        skillOnImage[index].gameObject.SetActive(on && learned);
        skillOffImage[index].gameObject.SetActive(!on && learned);
    }
}
