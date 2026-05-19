using UnityEngine;

/// <summary>
/// Tracks current score, high score, distance traveled, and coins collected.
/// Supports score multiplier from PowerUpManager.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [Tooltip("Score points earned per second of survival")]
    public float scorePerSecond = 10f;
    [Tooltip("Base score points earned per coin collected")]
    public int scorePerCoin = 50;

    public float CurrentScore    { get; private set; } = 0f;
    public float HighScore       { get; private set; } = 0f;
    public float DistanceTraveled{ get; private set; } = 0f;
    public int   CoinsCollected  { get; private set; } = 0;
    public bool  IsNewHighScore  { get; private set; } = false;

    private const string HIGH_SCORE_KEY  = "TheLastRun_HighScore";
    private const string BEST_COINS_KEY  = "TheLastRun_BestCoins";

    public int BestCoins { get; private set; } = 0;

    // Events
    public System.Action<float> OnScoreChanged;
    public System.Action        OnNewHighScore;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        HighScore = PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0f);
        BestCoins = PlayerPrefs.GetInt(BEST_COINS_KEY, 0);
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver   += HandleGameOver;
            GameManager.Instance.OnGameRestart += ResetScore;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver   -= HandleGameOver;
            GameManager.Instance.OnGameRestart -= ResetScore;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        // Score per second, scaled by multiplier
        float multiplier = PowerUpManager.Instance != null
            ? PowerUpManager.Instance.GetScoreMultiplier()
            : 1f;

        AddScore(scorePerSecond * multiplier * Time.deltaTime);

        // Distance
        DistanceTraveled += GameManager.Instance.GetCurrentSpeed() * Time.deltaTime;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void AddScore(float amount)
    {
        CurrentScore += amount;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>Call when a coin is collected. Multiplier applied externally.</summary>
    public void AddCoinScore(float multiplier = 1f)
    {
        CoinsCollected++;
        AddScore(scorePerCoin * multiplier);
    }

    // ── Game Over ─────────────────────────────────────────────────────────────

    private void HandleGameOver()
    {
        IsNewHighScore = false;

        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            IsNewHighScore = true;
            PlayerPrefs.SetFloat(HIGH_SCORE_KEY, HighScore);
            OnNewHighScore?.Invoke();
            AudioManager.Instance?.PlayNewHighScore();
            Debug.Log($"[ScoreManager] New High Score: {HighScore:F0}");
        }

        if (CoinsCollected > BestCoins)
        {
            BestCoins = CoinsCollected;
            PlayerPrefs.SetInt(BEST_COINS_KEY, BestCoins);
        }

        PlayerPrefs.Save();
    }

    private void ResetScore()
    {
        CurrentScore   = 0f;
        DistanceTraveled = 0f;
        CoinsCollected = 0;
        IsNewHighScore = false;
    }
}
