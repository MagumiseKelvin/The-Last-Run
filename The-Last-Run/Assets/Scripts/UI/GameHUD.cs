using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Phase 5 — Full in-game HUD.
/// Displays score, distance, coins, speed, power-up indicators,
/// pause menu, and game over screen with animated transitions.
/// </summary>
public class GameHUD : MonoBehaviour
{
    // ── Live Stats ────────────────────────────────────────────────────────────
    [Header("Live Stats")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI coinCountText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI multiplierText;   // shows "2x" when active

    // ── Power-Up Indicators ───────────────────────────────────────────────────
    [Header("Power-Up Indicators")]
    public GameObject shieldIndicator;
    public GameObject magnetIndicator;
    public GameObject multiplierIndicator;
    public Image      shieldTimerFill;
    public Image      magnetTimerFill;
    public Image      multiplierTimerFill;

    // ── Countdown ─────────────────────────────────────────────────────────────
    [Header("Countdown")]
    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;

    // ── Pause ─────────────────────────────────────────────────────────────────
    [Header("Pause")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button pauseRestartButton;
    public Button pauseMenuButton;

    // ── Game Over ─────────────────────────────────────────────────────────────
    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverHighScoreText;
    public TextMeshProUGUI gameOverDistanceText;
    public TextMeshProUGUI gameOverCoinsText;
    public TextMeshProUGUI newHighScoreBanner;   // "NEW BEST!" label
    public Button restartButton;
    public Button menuButton;
    public CanvasGroup gameOverCanvasGroup;      // for fade-in animation

    // ── Internal ──────────────────────────────────────────────────────────────
    private bool _isPaused = false;
    private float _powerUpTimers_shield;
    private float _powerUpTimers_magnet;
    private float _powerUpTimers_multiplier;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        // Hide panels
        SetActive(gameOverPanel, false);
        SetActive(pausePanel, false);
        SetActive(countdownPanel, false);
        SetActive(shieldIndicator, false);
        SetActive(magnetIndicator, false);
        SetActive(multiplierIndicator, false);
        SetActive(multiplierText?.gameObject, false);

        // Buttons — use direct scene reload for WebGL reliability
        pauseButton?.onClick.AddListener(TogglePause);
        resumeButton?.onClick.AddListener(TogglePause);
        pauseRestartButton?.onClick.AddListener(() =>
        {
            ResumeTime();
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        });
        pauseMenuButton?.onClick.AddListener(() =>
        {
            ResumeTime();
            GameManager.Instance?.GoToMainMenu();
        });
        restartButton?.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        });
        menuButton?.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            GameManager.Instance?.GoToMainMenu();
        });

        // Start with countdown then launch game
        StartCoroutine(CountdownRoutine());
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += HandleGameOver;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnPowerUpActivated += HandlePowerUpActivated;
            PowerUpManager.Instance.OnPowerUpExpired   += HandlePowerUpExpired;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= HandleGameOver;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnPowerUpActivated -= HandlePowerUpActivated;
            PowerUpManager.Instance.OnPowerUpExpired   -= HandlePowerUpExpired;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        if (ScoreManager.Instance != null)
        {
            // Score = coins only (not time-based)
            if (scoreText != null)
                scoreText.text = $"{ScoreManager.Instance.CurrentScore}";

            // Distance = meters run
            if (distanceText != null)
                distanceText.text = $"{ScoreManager.Instance.DistanceTraveled:F0}m";

            // Coins collected count
            if (coinCountText != null)
                coinCountText.text = $"Coins: {ScoreManager.Instance.CoinsCollected}";

            // Best score (from previous runs)
            if (highScoreText != null)
                highScoreText.text = $"Best: {ScoreManager.Instance.HighScore}";
        }

        // Speed display
        if (speedText != null)
            speedText.text = $"{GameManager.Instance.GetCurrentSpeed():F0} m/s";

        // Power-up timer fills
        UpdatePowerUpTimers();

        // Pause input
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            TogglePause();
    }

    // ── Countdown ─────────────────────────────────────────────────────────────

    private IEnumerator CountdownRoutine()
    {
        SetActive(countdownPanel, true);
        string[] steps = { "3", "2", "1", "GO!" };

        foreach (string step in steps)
        {
            if (countdownText != null) countdownText.text = step;
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.countdownSFX);
            yield return new WaitForSeconds(step == "GO!" ? 0.6f : 0.9f);
        }

        SetActive(countdownPanel, false);
        GameManager.Instance?.StartGame();
    }

    // ── Score ─────────────────────────────────────────────────────────────────

    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.text = $"{score}";
    }

    // ── Pause ─────────────────────────────────────────────────────────────────

    private void TogglePause()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

        _isPaused = !_isPaused;
        SetActive(pausePanel, _isPaused);

        if (_isPaused)
        {
            Time.timeScale = 0f;
            GameManager.Instance.PauseGame();
        }
        else
        {
            ResumeTime();
            GameManager.Instance.ResumeGame();
        }
    }

    private void ResumeTime()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        SetActive(pausePanel, false);
    }

    // ── Game Over ─────────────────────────────────────────────────────────────

    private void HandleGameOver()
    {
        StartCoroutine(ShowGameOverRoutine());
    }

    private IEnumerator ShowGameOverRoutine()
    {
        // Short delay so player sees the crash
        yield return new WaitForSecondsRealtime(1.0f);

        SetActive(gameOverPanel, true);

        // Make sure CanvasGroup is fully visible
        if (gameOverCanvasGroup != null)
            gameOverCanvasGroup.alpha = 1f;

        if (ScoreManager.Instance != null)
        {
            if (gameOverScoreText     != null) gameOverScoreText.text     = $"{ScoreManager.Instance.CurrentScore}";
            if (gameOverHighScoreText != null) gameOverHighScoreText.text = $"Best: {ScoreManager.Instance.HighScore}";
            if (gameOverDistanceText  != null) gameOverDistanceText.text  = $"{ScoreManager.Instance.DistanceTraveled:F0}m";
            if (gameOverCoinsText     != null) gameOverCoinsText.text     = $"Coins: {ScoreManager.Instance.CoinsCollected}";
            if (newHighScoreBanner    != null)
                newHighScoreBanner.gameObject.SetActive(ScoreManager.Instance.IsNewHighScore);
        }

        // Fade in smoothly using unscaled time (works even if timeScale = 0)
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * 2.5f;
                gameOverCanvasGroup.alpha = Mathf.Clamp01(t);
                yield return null;
            }
            gameOverCanvasGroup.alpha = 1f;
        }
    }

    // ── Power-Up UI ───────────────────────────────────────────────────────────

    private void HandlePowerUpActivated(PowerUpManager.PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUpManager.PowerUpType.Shield:
                SetActive(shieldIndicator, true);
                _powerUpTimers_shield = duration;
                break;
            case PowerUpManager.PowerUpType.Magnet:
                SetActive(magnetIndicator, true);
                _powerUpTimers_magnet = duration;
                break;
            case PowerUpManager.PowerUpType.ScoreMultiplier:
                SetActive(multiplierIndicator, true);
                SetActive(multiplierText?.gameObject, true);
                if (multiplierText != null) multiplierText.text = "2x";
                _powerUpTimers_multiplier = duration;
                break;
        }
    }

    private void HandlePowerUpExpired(PowerUpManager.PowerUpType type)
    {
        switch (type)
        {
            case PowerUpManager.PowerUpType.Shield:
                SetActive(shieldIndicator, false);
                break;
            case PowerUpManager.PowerUpType.Magnet:
                SetActive(magnetIndicator, false);
                break;
            case PowerUpManager.PowerUpType.ScoreMultiplier:
                SetActive(multiplierIndicator, false);
                SetActive(multiplierText?.gameObject, false);
                break;
        }
    }

    private void UpdatePowerUpTimers()
    {
        float dt = Time.deltaTime;

        if (_powerUpTimers_shield > 0f)
        {
            _powerUpTimers_shield -= dt;
            // fill is set externally by duration tracking — simplified here
        }
        if (_powerUpTimers_magnet > 0f)
            _powerUpTimers_magnet -= dt;

        if (_powerUpTimers_multiplier > 0f)
            _powerUpTimers_multiplier -= dt;
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    private void SetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }
}
