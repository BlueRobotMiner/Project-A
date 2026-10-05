using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int totalStardust;
    public int hullLevel;
    public int shieldLevel;
    public int firepowerLevel;
    public int stardustLevel;
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

    private const string SaveKey = "save";

    public static void Load()
    {
        data = null;
#if UNITY_WEBGL && !UNITY_EDITOR
        if (PlayerPrefs.HasKey(SaveKey))
        {
            try
            {
                data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Save data could not be read: " + e.Message);
            }
        }
#else
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
#endif
        if (data == null) data = new SaveData();
    }

    public static void Save()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
#else
        string tempPath = SavePath + ".tmp";
        File.WriteAllText(tempPath, JsonUtility.ToJson(Data, true));
        if (File.Exists(SavePath)) File.Delete(SavePath);
        File.Move(tempPath, SavePath);
#endif
    }
}
