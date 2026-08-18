using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public float playerHP;
    public float playerMaxHP;
    public float posX, posY, posZ;
    public string currentSceneName;
    public List<string> unlockedItems = new List<string>();
    public string lastBonfireName = ""; // 빈 문자열이면 화톳불 미사용(기본 스폰)

    // ---- 레벨 / 경험치 ----
    public int playerLevel;
    public int playerExp;

    // ---- 스킬 ----
    // BattleManager.instance.skillLevels 배열을 그대로 옮겨 담는 용도.
    // 인덱스는 SkillUIManager의 playerSkills 배열/BattleManager의 skillLevels와 동일한 순서여야 함.
    public List<int> skillLevels = new List<int>();

    // 아직 안 쓴(스킬 찍는 데 쓸 수 있는) 스킬 포인트
    public int skillPoint;
}