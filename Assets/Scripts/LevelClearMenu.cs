using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelClearMenu : MonoBehaviour
{
    public static string LastLevel;

    public string[] levelScenes = { "Level 1", "Level 2", "Level 3" };
    public bool endlessAfterLastLevel = false;
    public string demoCompleteTitle = "Demo Complete!";
    public string menuSceneName = "MainMenu";

    public GameObject levelClearPanel;
    public TMP_Text titleText;
    public Button continueButton;
    public Button shopButton;
    public Button mainMenuButton;
    public Button settingsButton;
    public ShopUI shop;
    public SettingsUI settings;

    void Awake()
    {
        if (continueButton != null) continueButton.onClick.AddListener(Continue);
        if (shopButton != null) shopButton.onClick.AddListener(() => OpenPanel(shop != null ? shop.gameObject : null));
        if (settingsButton != null) settingsButton.onClick.AddListener(() => OpenPanel(settings != null ? settings.gameObject : null));
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(menuSceneName));
        if (shop != null) shop.returnPanel = levelClearPanel;
        if (settings != null) settings.returnPanel = levelClearPanel;
        Time.timeScale = 1f;
    }

    void Start()
    {
        bool demoDone = !endlessAfterLastLevel && LastLevelIndex() >= levelScenes.Length - 1;
        if (demoDone)
        {
            if (titleText != null) titleText.text = demoCompleteTitle;
            if (continueButton != null) continueButton.gameObject.SetActive(false);
        }
    }

    int LastLevelIndex()
    {
        return System.Array.IndexOf(levelScenes, LastLevel);
    }

    void Continue()
    {
        if (levelScenes == null || levelScenes.Length == 0) return;
        int next = LastLevelIndex() + 1;
        if (next >= levelScenes.Length)
        {
            if (!endlessAfterLastLevel) return;
            next = Random.Range(0, levelScenes.Length);
        }
        SceneManager.LoadScene(levelScenes[next]);
    }

    void OpenPanel(GameObject panel)
    {
        if (panel == null) return;
        if (levelClearPanel != null) levelClearPanel.SetActive(false);
        panel.SetActive(true);
    }
}
