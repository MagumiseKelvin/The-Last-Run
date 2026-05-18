using UnityEngine;

/// <summary>
/// Tracks current score, high score, and distance traveled.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [Tooltip("Score points earned per second of survival")]
    public float scorePerSecond = 10f;
    [Tooltip("Score points earned per coin collected")]
    public int scorePerCoin = 50;

    public float CurrentScore { get; private set; } = 0f;
    public float HighScore { get; private set; } = 0f;
    public float DistanceTraveled { get; private set; } = 0f;

    private const string HIGH_SCORE_KEY = "TheLastRun_HighScore";

    // Event fired whenever score changes (UI can subscribe)
    public System.Action<float> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Load saved high score
        HighScore = PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0f);
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += HandleGameOver;
            GameManager.Instance.OnGameRestart += ResetScore;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= HandleGameOver;
            GameManager.Instance.OnGameRestart -= ResetScore;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        // Add score over time
        float earned = scorePerSecond * Time.deltaTime;
        AddScore(earned);

        // Track distance based on current speed
        DistanceTraveled += GameManager.Instance.GetCurrentSpeed() * Time.deltaTime;
    }

    /// <summary>Add points to the current score.</summary>
    public void AddScore(float amount)
    {
        CurrentScore += amount;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>Call when a coin is collected.</summary>
    public void AddCoinScore()
    {
        AddScore(scorePerCoin);
    }

    private void HandleGameOver()
    {
        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            PlayerPrefs.SetFloat(HIGH_SCORE_KEY, HighScore);
            PlayerPrefs.Save();
            Debug.Log($"[ScoreManager] New High Score: {HighScore}");
        }
    }

    private void ResetScore()
    {
        CurrentScore = 0f;
        DistanceTraveled = 0f;
    }
}
