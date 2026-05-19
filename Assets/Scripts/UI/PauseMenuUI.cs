using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Phase 5 — Standalone pause menu controller.
/// Can be used independently or driven by GameHUD.
/// Handles resume, restart, settings, and main menu from pause state.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Pause Panel Elements")]
    public Button resumeButton;
    public Button restartButton;
    public Button settingsButton;
    public Button mainMenuButton;

    [Header("Settings Sub-Panel")]
    public GameObject settingsSubPanel;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Button settingsCloseButton;

    private void Start()
    {
        resumeButton?.onClick.AddListener(OnResume);
        restartButton?.onClick.AddListener(OnRestart);
        settingsButton?.onClick.AddListener(OnSettings);
        mainMenuButton?.onClick.AddListener(OnMainMenu);
        settingsCloseButton?.onClick.AddListener(CloseSettings);

        if (settingsSubPanel != null) settingsSubPanel.SetActive(false);

        // Init sliders
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("TheLastRun_MusicVol", 0.6f);
            musicSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetMusicVolume(v));
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("TheLastRun_SFXVol", 1f);
            sfxSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetSFXVolume(v));
        }
    }

    private void OnResume()
    {
        Time.timeScale = 1f;
        GameManager.Instance?.ResumeGame();
        gameObject.SetActive(false);
    }

    private void OnRestart()
    {
        Time.timeScale = 1f;
        GameManager.Instance?.RestartGame();
    }

    private void OnSettings()
    {
        if (settingsSubPanel != null)
            settingsSubPanel.SetActive(true);
    }

    private void CloseSettings()
    {
        if (settingsSubPanel != null)
            settingsSubPanel.SetActive(false);
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance?.GoToMainMenu();
    }
}
