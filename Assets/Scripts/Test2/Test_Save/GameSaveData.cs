using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public float playerHP;
    public float playerMaxHP;
    public float posX, posY, posZ;
    public string currentSceneName;
    public List<string> unlockedItems = new List<string>();
    public string lastBonfireID;
}