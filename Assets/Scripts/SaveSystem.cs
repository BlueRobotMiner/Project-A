using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int totalStardust;
}

public static class SaveSystem
{
    private static SaveData data;

    private static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, "save.json"); }
    }

    public static SaveData Data
    {
        get
        {
            if (data == null) Load();
            return data;
        }
    }

    public static void Load()
    {
        data = null;
        if (File.Exists(SavePath))
        {
            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Save file could not be read: " + e.Message);
            }
        }
        if (data == null) data = new SaveData();
    }

    public static void Save()
    {
        string tempPath = SavePath + ".tmp";
        File.WriteAllText(tempPath, JsonUtility.ToJson(Data, true));
        if (File.Exists(SavePath)) File.Delete(SavePath);
        File.Move(tempPath, SavePath);
    }
}
