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
}