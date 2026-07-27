using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private Image playerHPInnerbar;
    [SerializeField] private Image playerMPInnerbar;
    [SerializeField] private Image bossHPInnerbar;
    [SerializeField] private float lerpSpeed = 8f;

    private float playerHPTarget = 1f;
    private float playerMPTarget = 1f;
    private float bossHPTarget = 1f;

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
        if (playerHPInnerbar != null)
            playerHPInnerbar.fillAmount = Mathf.Lerp(playerHPInnerbar.fillAmount, playerHPTarget, lerpSpeed * Time.deltaTime);

        if (playerMPInnerbar != null)
            playerMPInnerbar.fillAmount = Mathf.Lerp(playerMPInnerbar.fillAmount, playerMPTarget, lerpSpeed * Time.deltaTime);

        if (bossHPInnerbar != null)
            bossHPInnerbar.fillAmount = Mathf.Lerp(bossHPInnerbar.fillAmount, bossHPTarget, lerpSpeed * Time.deltaTime);
    }

    public void updatePlayerHP(float currentHp, float maxHp)
    {
        if (playerHPInnerbar == null) return;
        playerHPTarget = NormalizeRatio(currentHp, maxHp);
    }

    public void updatePlayerMP(float currentMp, float maxMp)
    {
        if (playerMPInnerbar == null) return;
        playerMPTarget = NormalizeRatio(currentMp, maxMp);
    }

    public void updateBossHP(float currentHp, float maxHp)
    {
        if (bossHPInnerbar == null) return;
        Debug.Log($"Updating Boss HP: Current HP = {currentHp}, Max HP = {maxHp}");
        bossHPTarget = NormalizeRatio(currentHp, maxHp);
    }

    private float NormalizeRatio(float current, float max)
    {
        if (max <= 0f) return 0f;
        return Mathf.Clamp01(current / max);
    }
}
