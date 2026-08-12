using UnityEngine;
using TMPro;

public class SkillUIManager : MonoBehaviour
{
    [SerializeField] private Transform contentTransform;
    [SerializeField] private GameObject skillUiLearnedPrefab;
    [SerializeField] private GameObject skillUiNotLearnedPrefab;
    [SerializeField] private PlayerSkillData[] playerSkills;

    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillInfoText;
    [SerializeField] private RectTransform expBar;
    [SerializeField] private Transform expBarPoint;
    [SerializeField] private GameObject learnButton;
    [SerializeField] private GameObject noExp;
    [SerializeField] private GameObject learned;
    

    private Vector3 originalExpBarPosition;
    private Quaternion originalExpBarRotation;
    private int selectedSkillIndex;

    private void Awake()
    {
        originalExpBarPosition = expBar.position;
        originalExpBarRotation = expBar.rotation;
    }

    private void Start()
    {
        RefreshSkillList();

        if (playerSkills.Length > 0)
        {
            ShowSkillInfo(playerSkills[0], BattleManager.instance.skillLevels[0] > 0);
        }
    }

    private void RefreshSkillList()
    {
        for (int i = contentTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(contentTransform.GetChild(i).gameObject);
        }

        for (int i = 0; i < playerSkills.Length; i++)
        {
            PlayerSkillData skillData = playerSkills[i];
            bool isLearned = BattleManager.instance.skillLevels[i] > 0;

            GameObject skillUiPrefab = isLearned ? skillUiLearnedPrefab : skillUiNotLearnedPrefab;
            GameObject skillUiInstance = Instantiate(skillUiPrefab, contentTransform);
            SkillUiElement skillUiElement = skillUiInstance.GetComponent<SkillUiElement>();
            skillUiElement.SetSkillData(skillData, isLearned, ShowSkillInfo);
        }
    }

    private void OnEnable()
    {
        expBar.position = expBarPoint.position;
        expBar.rotation = expBarPoint.rotation;
    }

    private void OnDisable()
    {
        if (expBar == null) return;
        expBar.position = originalExpBarPosition;
        expBar.rotation = originalExpBarRotation;
    }

    private void ShowSkillInfo(PlayerSkillData skillData, bool isLearned)
    {
        selectedSkillIndex = System.Array.IndexOf(playerSkills, skillData);
        skillNameText.text = skillData.skillName;
        skillInfoText.text = skillData.skillDescription;
        UpdateLearnState(isLearned);
    }

    private void UpdateLearnState(bool isLearned)
    {
        bool canLearn = !isLearned && BattleManager.instance.skillPoint > 0;
        bool hasNoExp = !isLearned && BattleManager.instance.skillPoint <= 0;

        learnButton.SetActive(canLearn);
        learned.SetActive(isLearned);
        noExp.SetActive(hasNoExp);
    }

    public void AddSkill()
    {
        if (BattleManager.instance.skillPoint <= 0)
            return;

        BattleManager.instance.skillLevels[selectedSkillIndex]++;
        BattleManager.instance.skillPoint--;

        RefreshSkillList();
        ShowSkillInfo(playerSkills[selectedSkillIndex], true);
        UIManager.instance.RefreshSkillImages();
    }
}
