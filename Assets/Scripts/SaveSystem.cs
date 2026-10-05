// SaveSystem
// Holds all persistent progress (stardust, upgrade levels) as one serializable data object.
// Saves to a JSON file on desktop/editor, and to PlayerPrefs in WebGL builds so it
// persists in the browser. Other scripts read SaveSystem.Data and call Save().
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

    // Loads the save once per session; every later read uses the cached data object.
    public static void Load()
    {
        data = null;
        // WebGL: file writes to persistentDataPath are unreliable in the browser and the
        // folder can change between builds, so the save is kept in PlayerPrefs instead.
        // This branch was written with AI assistance (Claude, Anthropic).
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

    // Writes through PlayerPrefs in WebGL, or an atomic temp-file swap on desktop.
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
