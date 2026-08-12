using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public GameObject skillUIManager;
    
    public void ToggleSkillUIManager()
    {
        if (skillUIManager != null)
        {
            skillUIManager.SetActive(!skillUIManager.activeSelf);
        }
    }
    public void AddExp100()
    {
        BattleManager.instance.AddExp(100);
    }
}
