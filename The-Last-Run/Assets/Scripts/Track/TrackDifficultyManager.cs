using UnityEngine;

/// <summary>
/// Phase 6 — Adjusts obstacle spawn chance and collectible density
/// based on how long the player has survived. Makes the game progressively harder.
/// </summary>
public class TrackDifficultyManager : MonoBehaviour
{
    public static TrackDifficultyManager Instance { get; private set; }

    [Header("Difficulty Curve")]
    [Tooltip("Seconds before difficulty starts increasing")]
    public float gracePeriod = 10f;
    [Tooltip("How many seconds to reach max difficulty")]
    public float maxDifficultyTime = 120f;

    [Header("Obstacle Spawn Chance")]
    public float minObstacleChance = 0.25f;
    public float maxObstacleChance = 0.70f;

    [Header("Collectible Spawn Chance")]
    public float minCoinChance = 0.50f;
    public float maxCoinChance = 0.85f;

    private float _elapsed = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;
        _elapsed += Time.deltaTime;
    }

    /// <summary>Returns current obstacle spawn probability (0-1).</summary>
    public float GetObstacleChance()
    {
        float t = Mathf.Clamp01((_elapsed - gracePeriod) / maxDifficultyTime);
        return Mathf.Lerp(minObstacleChance, maxObstacleChance, t);
    }

    /// <summary>Returns current coin spawn probability (0-1).</summary>
    public float GetCoinChance()
    {
        float t = Mathf.Clamp01((_elapsed - gracePeriod) / maxDifficultyTime);
        return Mathf.Lerp(minCoinChance, maxCoinChance, t);
    }

    public void Reset() => _elapsed = 0f;
}
