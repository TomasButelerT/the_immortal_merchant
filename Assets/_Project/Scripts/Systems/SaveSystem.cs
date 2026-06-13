using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PermanentProgressData
{
    public int gold;
    public int corruptionLevel;
    public int damageUpgradeLevel;
    public int damageBonus;
    public int healthUpgradeLevel;
    public int maxHealthBonus;
    public int moveSpeedUpgradeLevel;
    public float moveSpeedBonus;
    public string lastRunSummary;
}

public static class SaveSystem
{
    private const string FileName = "prototype_progress.json";

    public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static void Save(PermanentProgressData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static PermanentProgressData Load()
    {
        if (!File.Exists(SavePath))
        {
            return null;
        }

        try
        {
            return JsonUtility.FromJson<PermanentProgressData>(File.ReadAllText(SavePath));
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load save file: {exception.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }
}
