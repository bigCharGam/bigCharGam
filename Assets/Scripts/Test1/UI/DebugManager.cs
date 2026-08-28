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
    // 플레이어 x좌표를 이동
    public void GotoX(int x)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[DebugManager] Player를 찾을 수 없습니다.");
            return;
        }

        Vector3 pos = player.transform.position;
        pos.x = x;
        player.transform.position = pos;
    }
}
