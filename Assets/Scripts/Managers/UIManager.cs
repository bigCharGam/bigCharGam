using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private Image playerHPInnerbar;
    [SerializeField] private Image playerMPInnerbar;
    [SerializeField] private Image bossHPInnerbar;
    [SerializeField] private float lerpSpeed = 8f;

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

    public void updatePlayerHP(float currentHp, float maxHp)
    {
        if (playerHPInnerbar == null) return;
        float target = NormalizeRatio(currentHp, maxHp);
        playerHPInnerbar.fillAmount = Mathf.Lerp(playerHPInnerbar.fillAmount, target, lerpSpeed * Time.deltaTime);
    }

    public void updatePlayerMP(float currentMp, float maxMp)
    {
        if (playerMPInnerbar == null) return;
        float target = NormalizeRatio(currentMp, maxMp);
        playerMPInnerbar.fillAmount = Mathf.Lerp(playerMPInnerbar.fillAmount, target, lerpSpeed * Time.deltaTime);
    }

    public void updateBossHP(float currentHp, float maxHp)
    {
        if (bossHPInnerbar == null) return;
        float target = NormalizeRatio(currentHp, maxHp);
        bossHPInnerbar.fillAmount = Mathf.Lerp(bossHPInnerbar.fillAmount, target, lerpSpeed * Time.deltaTime);
    }

    private float NormalizeRatio(float current, float max)
    {
        if (max <= 0f) return 0f;
        return Mathf.Clamp01(current / max);
    }
}
