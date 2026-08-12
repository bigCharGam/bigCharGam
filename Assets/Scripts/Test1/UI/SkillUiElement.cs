using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class PlayerSkillData
{
    public string skillName;
    public int learnCost;
    [TextArea] public string skillDescription;
}
public class SkillUiElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI learnCostText;

    private PlayerSkillData skillData;
    private bool isLearned;
    private Action<PlayerSkillData, bool> onClick;

    private void Awake()
    {
        Button button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        button.onClick.AddListener(HandleClick);
    }

    public void SetSkillData(PlayerSkillData skillData, bool isLearned, Action<PlayerSkillData, bool> onClick)
    {
        this.skillData = skillData;
        this.isLearned = isLearned;
        this.onClick = onClick;

        skillNameText.text = skillData.skillName;
        if (isLearned && learnCostText != null)
        {
            learnCostText.text = skillData.learnCost.ToString();
        }
    }

    private void HandleClick()
    {
        onClick?.Invoke(skillData, isLearned);
    }
}
