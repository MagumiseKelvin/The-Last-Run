using UnityEngine;

/// <summary>
/// Tracks score (coins only), distance, and high score.
/// Score = coins collected x coin value only. Distance is tracked separately.
/// This matches Temple Run style — you earn points by collecting coins, not by surviving.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [Tooltip("Points per coin collected")]
    public int scorePerCoin = 50;

    // Public read-only state
    public int   CurrentScore     { get; private set; } = 0;
    public int   HighScore        { get; private set; } = 0;
    public float DistanceTraveled { get; private set; } = 0f;
    public int   CoinsCollected   { get; private set; } = 0;
    public int   BestCoins        { get; private set; } = 0;
    public bool  IsNewHighScore   { get; private set; } = false;

    private const string HIGH_SCORE_KEY = "TheLastRun_HighScore";
    private const string BEST_COINS_KEY = "TheLastRun_BestCoins";

    // Event fired when score changes (coin collected)
    public System.Action<int> OnScoreChanged;
    public System.Action      OnNewHighScore;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        HighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        BestCoins = PlayerPrefs.GetInt(BEST_COINS_KEY, 0);
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver    += HandleGameOver;
            GameManager.Instance.OnGameRestart += ResetScore;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver    -= HandleGameOver;
            GameManager.Instance.OnGameRestart -= ResetScore;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        // Track distance — this is NOT the score, just meters run
        DistanceTraveled += GameManager.Instance.GetCurrentSpeed() * Time.deltaTime;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Call when a coin is collected. Multiplier from power-ups.</summary>
    public void AddCoinScore(float multiplier = 1f)
    {
        CoinsCollected++;
        int points = Mathf.RoundToInt(scorePerCoin * multiplier);
        CurrentScore += points;
        OnScoreChanged?.Invoke(CurrentScore);
        Debug.Log($"[ScoreManager] Coin collected! Score: {CurrentScore} | Coins: {CoinsCollected}");
    }

    // ── Game Over ─────────────────────────────────────────────────────────────

    private void HandleGameOver()
    {
        IsNewHighScore = false;

        if (CurrentScore > HighScore)
        {
            HighScore      = CurrentScore;
            IsNewHighScore = true;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, HighScore);
            OnNewHighScore?.Invoke();
            AudioManager.Instance?.PlayNewHighScore();
            Debug.Log($"[ScoreManager] New High Score: {HighScore}");
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
        CurrentScore     = 0;
        DistanceTraveled = 0f;
        CoinsCollected   = 0;
        IsNewHighScore   = false;
    }
}
