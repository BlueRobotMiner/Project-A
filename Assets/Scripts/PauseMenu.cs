using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused;

    public GameObject pausePanel;
    public GameObject shopPanel;
    public GameObject settingsPanel;
    public Button resumeButton;
    public Button shopButton;
    public Button endRunButton;
    public Button settingsButton;
    public Button quitButton;
    public string menuSceneName = "MainMenu";
    public KeyCode pauseKey = KeyCode.Escape;

    void Awake()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (shopButton != null) shopButton.onClick.AddListener(() => OpenPanel(shopPanel));
        if (settingsButton != null) settingsButton.onClick.AddListener(() => OpenPanel(settingsPanel));
        if (endRunButton != null) endRunButton.onClick.AddListener(EndRun);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);
        SetPaused(false);
    }

    void Update()
    {
        if (GameOverMenu.IsGameOver || !Input.GetKeyDown(pauseKey)) return;
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        SetPaused(true);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
        if (pausePanel != null) pausePanel.SetActive(paused);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void OpenPanel(GameObject panel)
    {
        if (panel == null) return;
        if (pausePanel != null) pausePanel.SetActive(false);
        panel.SetActive(true);
    }

    void EndRun()
    {
        SetPaused(false);
        SceneManager.LoadScene(menuSceneName);
    }

    void Quit()
    {
        SetPaused(false);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
