using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Phase 5 — Controls the Main Menu scene UI.
/// Handles Play, Settings, and Quit buttons, plus high score display.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    [Header("Main Panel")]
    public Button playButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI bestCoinsText;
    public TextMeshProUGUI versionText;

    [Header("Settings Panel")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI musicVolumeLabel;
    public TextMeshProUGUI sfxVolumeLabel;
    public Button settingsBackButton;

    [Header("Credits Panel")]
    public Button creditsBackButton;

    [Header("Scene")]
    [Tooltip("Name of the gameplay scene to load")]
    public string gameSceneName = "Game";

    private void Start()
    {
        // Show main panel, hide others
        ShowPanel(mainPanel);

        // Display saved stats
        float highScore = PlayerPrefs.GetFloat("TheLastRun_HighScore", 0f);
        int bestCoins   = PlayerPrefs.GetInt("TheLastRun_BestCoins", 0);

        if (highScoreText != null)
            highScoreText.text = $"Best Score: {highScore:F0}";

        if (bestCoinsText != null)
            bestCoinsText.text = $"Best Coins: {bestCoins}";

        if (versionText != null)
            versionText.text = $"v{Application.version}";

        // Wire buttons
        playButton?.onClick.AddListener(OnPlayPressed);
        settingsButton?.onClick.AddListener(() => ShowPanel(settingsPanel));
        creditsButton?.onClick.AddListener(() => ShowPanel(creditsPanel));
        quitButton?.onClick.AddListener(OnQuitPressed);
        settingsBackButton?.onClick.AddListener(() => ShowPanel(mainPanel));
        creditsBackButton?.onClick.AddListener(() => ShowPanel(mainPanel));

        // Settings sliders
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("TheLastRun_MusicVol", 0.6f);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            UpdateMusicLabel(musicVolumeSlider.value);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("TheLastRun_SFXVol", 1f);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            UpdateSFXLabel(sfxVolumeSlider.value);
        }

        // Play menu music
        AudioManager.Instance?.PlayMusic(AudioManager.Instance.mainMenuMusic);
    }

    private void OnPlayPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
        UpdateMusicLabel(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        UpdateSFXLabel(value);
    }

    private void UpdateMusicLabel(float value)
    {
        if (musicVolumeLabel != null)
            musicVolumeLabel.text = $"Music: {Mathf.RoundToInt(value * 100)}%";
    }

    private void UpdateSFXLabel(float value)
    {
        if (sfxVolumeLabel != null)
            sfxVolumeLabel.text = $"SFX: {Mathf.RoundToInt(value * 100)}%";
    }

    private void ShowPanel(GameObject panel)
    {
        if (mainPanel    != null) mainPanel.SetActive(mainPanel == panel);
        if (settingsPanel != null) settingsPanel.SetActive(settingsPanel == panel);
        if (creditsPanel  != null) creditsPanel.SetActive(creditsPanel == panel);
    }
}
