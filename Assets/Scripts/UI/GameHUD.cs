using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the in-game HUD — score display, distance, and game over panel.
/// Requires TextMeshPro (included with Unity).
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("Score UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI distanceText;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverHighScoreText;
    public Button restartButton;
    public Button menuButton;

    [Header("Start Panel")]
    public GameObject startPanel;
    public Button startButton;

    private void Start()
    {
        // Hide game over panel at start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Show start panel
        if (startPanel != null) startPanel.SetActive(true);

        // Wire up buttons
        if (restartButton != null)
            restartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());

        if (menuButton != null)
            menuButton.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonPressed);

        // Update high score display
        UpdateHighScore();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += ShowGameOverPanel;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= ShowGameOverPanel;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        }
    }

    private void Update()
    {
        // Update distance display every frame
        if (distanceText != null && ScoreManager.Instance != null)
        {
            distanceText.text = $"{ScoreManager.Instance.DistanceTraveled:F0}m";
        }
    }

    private void OnStartButtonPressed()
    {
        if (startPanel != null) startPanel.SetActive(false);
        GameManager.Instance?.StartGame();
    }

    private void UpdateScoreDisplay(float score)
    {
        if (scoreText != null)
            scoreText.text = $"{score:F0}";
    }

    private void UpdateHighScore()
    {
        if (highScoreText != null && ScoreManager.Instance != null)
            highScoreText.text = $"Best: {ScoreManager.Instance.HighScore:F0}";
    }

    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (gameOverScoreText != null && ScoreManager.Instance != null)
            gameOverScoreText.text = $"Score: {ScoreManager.Instance.CurrentScore:F0}";

        if (gameOverHighScoreText != null && ScoreManager.Instance != null)
            gameOverHighScoreText.text = $"Best: {ScoreManager.Instance.HighScore:F0}";
    }
}
