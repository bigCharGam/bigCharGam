using UnityEngine;
using System.IO;

public static class SaveManager
{
    static string savePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public static GameSaveData Load()
    {
        if (!File.Exists(savePath)) return null;
        string json = File.ReadAllText(savePath);
        return JsonUtility.FromJson<GameSaveData>(json);
    }

    public static bool HasSave() => File.Exists(savePath);
}