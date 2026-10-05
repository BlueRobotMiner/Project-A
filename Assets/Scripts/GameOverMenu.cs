// GameOverMenu
// The game over overlay: freezes the game behind it, offers Restart Level (reloads the
// current scene) and Main Menu, plus Shop and Quit. Escape is blocked while it's up,
// so a dead run can't be resumed.
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    public static GameOverMenu Instance;
    public static bool IsGameOver;

    public GameObject gameOverPanel;
    [UnityEngine.Serialization.FormerlySerializedAs("retryButton")] public Button restartButton;
    public Button menuButton;
    public Button shopButton;
    public Button quitButton;
    public ShopUI shop;
    public string menuSceneName = "MainMenu";

    void Awake()
    {
        Instance = this;
        IsGameOver = false;
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
        if (menuButton != null) menuButton.onClick.AddListener(Menu);
        if (shopButton != null) shopButton.onClick.AddListener(OpenShop);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void Show()
    {
        IsGameOver = true;
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    // Unfreezes time and reloads whatever level the player died in.
    void RestartLevel()
    {
        ClearState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Menu()
    {
        ClearState();
        SceneManager.LoadScene(menuSceneName);
    }

    void OpenShop()
    {
        if (shop == null) return;
        shop.returnPanel = gameOverPanel;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        shop.gameObject.SetActive(true);
    }

    void Quit()
    {
        ClearState();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void ClearState()
    {
        IsGameOver = false;
        Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        if (Instance != this) return;
        Instance = null;
        ClearState();
    }
}
