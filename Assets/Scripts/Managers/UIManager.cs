using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private Image playerHPInnerbar;
    [SerializeField] private Image playerMPInnerbar;
    [SerializeField] private Image bossHPInnerbar;
    [SerializeField] private Image expBar;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private float lerpSpeed = 8f;
    [SerializeField] private float lerpSpeedExp = 3f;


    private float playerHPTarget = 1f;
    private float playerMPTarget = 1f;
    private float bossHPTarget = 1f;
    private float expTarget = 0f;

    [Header("Skill")]
    [SerializeField] private Image[] skillOnImage;
    [SerializeField] private Image[] skillOffImage;

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

    void Update()
    {
            playerHPInnerbar.fillAmount = Mathf.Lerp(playerHPInnerbar.fillAmount, playerHPTarget, lerpSpeed * Time.deltaTime);
            playerMPInnerbar.fillAmount = Mathf.Lerp(playerMPInnerbar.fillAmount, playerMPTarget, lerpSpeed * Time.deltaTime);
            bossHPInnerbar.fillAmount = Mathf.Lerp(bossHPInnerbar.fillAmount, bossHPTarget, lerpSpeed * Time.deltaTime);
            expBar.fillAmount = Mathf.MoveTowards(expBar.fillAmount, expTarget, lerpSpeedExp * Time.deltaTime);
            expText.text = BattleManager.instance.expLevel.ToString();
    }

    public void UpdatePlayerHP(float currentHp, float maxHp)
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
        skillOnImage[index].gameObject.SetActive(on);
        skillOffImage[index].gameObject.SetActive(!on);
    }
}
