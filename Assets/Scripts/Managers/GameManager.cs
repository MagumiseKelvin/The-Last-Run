using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager — controls game state, speed progression, and game over logic.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool IsGameRunning { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;

    [Header("Speed Settings")]
    [Tooltip("Starting forward speed of the world")]
    public float startSpeed = 8f;
    [Tooltip("Maximum speed the game can reach")]
    public float maxSpeed = 25f;
    [Tooltip("How fast the speed increases over time")]
    public float speedIncreaseRate = 0.1f;

    private float currentSpeed;

    // Events other systems can subscribe to
    public System.Action OnGameStart;
    public System.Action OnGameOver;
    public System.Action OnGameRestart;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentSpeed = startSpeed;
        // Game starts when player hits Play — called from UI
    }

    private void Update()
    {
        if (!IsGameRunning || IsGameOver) return;

        // Gradually increase speed over time
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += speedIncreaseRate * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }
    }

    /// <summary>Returns the current world movement speed.</summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    /// <summary>Call this to start the game (from main menu or UI).</summary>
    public void StartGame()
    {
        IsGameRunning = true;
        IsGameOver = false;
        currentSpeed = startSpeed;
        OnGameStart?.Invoke();
        Debug.Log("[GameManager] Game Started");
    }

    /// <summary>Call this when the player hits an obstacle.</summary>
    public void TriggerGameOver()
    {
        if (IsGameOver) return;

        IsGameRunning = false;
        IsGameOver = true;
        OnGameOver?.Invoke();
        Debug.Log("[GameManager] Game Over");
    }

    /// <summary>Reload the current scene to restart.</summary>
    public void RestartGame()
    {
        OnGameRestart?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Load the main menu scene.</summary>
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
