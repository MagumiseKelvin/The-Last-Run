using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager — controls game state, speed progression,
/// pause/resume, and game over logic.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── State ─────────────────────────────────────────────────────────────────
    public bool IsGameRunning { get; private set; } = false;
    public bool IsGameOver    { get; private set; } = false;
    public bool IsPaused      { get; private set; } = false;

    // ── Speed ─────────────────────────────────────────────────────────────────
    [Header("Speed Settings")]
    [Tooltip("Starting forward speed of the world")]
    public float startSpeed = 8f;
    [Tooltip("Maximum speed the game can reach")]
    public float maxSpeed = 25f;
    [Tooltip("How fast the speed increases per second")]
    public float speedIncreaseRate = 0.1f;

    private float _currentSpeed;

    // ── Events ────────────────────────────────────────────────────────────────
    public System.Action OnGameStart;
    public System.Action OnGameOver;
    public System.Action OnGameRestart;
    public System.Action OnGamePause;
    public System.Action OnGameResume;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _currentSpeed = startSpeed;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (!IsGameRunning || IsGameOver || IsPaused) return;

        // Ramp up speed over time
        if (_currentSpeed < maxSpeed)
        {
            _currentSpeed += speedIncreaseRate * Time.deltaTime;
            _currentSpeed  = Mathf.Min(_currentSpeed, maxSpeed);
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Returns the current world movement speed.</summary>
    public float GetCurrentSpeed() => _currentSpeed;

    /// <summary>Start the game — called after countdown.</summary>
    public void StartGame()
    {
        IsGameRunning = true;
        IsGameOver    = false;
        IsPaused      = false;
        _currentSpeed = startSpeed;
        Time.timeScale = 1f;
        OnGameStart?.Invoke();
        Debug.Log("[GameManager] Game Started");
    }

    /// <summary>Pause the game.</summary>
    public void PauseGame()
    {
        if (!IsGameRunning || IsGameOver) return;
        IsPaused = true;
        OnGamePause?.Invoke();
        Debug.Log("[GameManager] Game Paused");
    }

    /// <summary>Resume from pause.</summary>
    public void ResumeGame()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        OnGameResume?.Invoke();
        Debug.Log("[GameManager] Game Resumed");
    }

    /// <summary>Trigger game over — called when player hits an obstacle.</summary>
    public void TriggerGameOver()
    {
        if (IsGameOver) return;

        IsGameRunning = false;
        IsGameOver    = true;
        OnGameOver?.Invoke();
        Debug.Log("[GameManager] Game Over");
    }

    /// <summary>Reload the current scene to restart.</summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        OnGameRestart?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Load the main menu scene.</summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
