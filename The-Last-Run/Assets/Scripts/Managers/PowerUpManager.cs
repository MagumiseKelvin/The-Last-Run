using UnityEngine;
using System.Collections;

/// <summary>
/// Phase 4 — Manages active power-up states.
/// Power-ups are collected via the Collectible system and activated here.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    // ── Power-Up Types ────────────────────────────────────────────────────────
    public enum PowerUpType
    {
        None,
        ScoreMultiplier,    // 2x score for a duration
        Magnet,             // Auto-collect nearby coins
        Shield              // One-hit protection
    }

    // ── Settings ──────────────────────────────────────────────────────────────
    [Header("Score Multiplier")]
    public float scoreMultiplierValue = 2f;
    public float scoreMultiplierDuration = 8f;

    [Header("Magnet")]
    public float magnetRadius = 4f;
    public float magnetDuration = 8f;

    [Header("Shield")]
    public float shieldDuration = 6f;

    // ── State ─────────────────────────────────────────────────────────────────
    public bool IsScoreMultiplierActive { get; private set; }
    public bool IsMagnetActive          { get; private set; }
    public bool IsShieldActive          { get; private set; }

    // Events for UI to show/hide power-up indicators
    public System.Action<PowerUpType, float> OnPowerUpActivated;   // type, duration
    public System.Action<PowerUpType>        OnPowerUpExpired;

    private Transform _playerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    private void Update()
    {
        if (IsMagnetActive && _playerTransform != null)
        {
            PullNearbyCoins();
        }
    }

    // ── Activation ────────────────────────────────────────────────────────────

    public void ActivatePowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.ScoreMultiplier:
                StartCoroutine(ScoreMultiplierRoutine());
                break;
            case PowerUpType.Magnet:
                StartCoroutine(MagnetRoutine());
                break;
            case PowerUpType.Shield:
                StartCoroutine(ShieldRoutine());
                break;
        }
    }

    // ── Shield: absorb one hit ────────────────────────────────────────────────

    private IEnumerator ShieldRoutine()
    {
        IsShieldActive = true;
        OnPowerUpActivated?.Invoke(PowerUpType.Shield, shieldDuration);
        Debug.Log("[PowerUpManager] Shield activated");

        yield return new WaitForSeconds(shieldDuration);

        IsShieldActive = false;
        OnPowerUpExpired?.Invoke(PowerUpType.Shield);
        Debug.Log("[PowerUpManager] Shield expired");
    }

    /// <summary>
    /// Called by PlayerCollision before triggering game over.
    /// Returns true if the shield absorbed the hit.
    /// </summary>
    public bool TryAbsorbHit()
    {
        if (!IsShieldActive) return false;

        IsShieldActive = false;
        StopCoroutine(ShieldRoutine());
        OnPowerUpExpired?.Invoke(PowerUpType.Shield);
        Debug.Log("[PowerUpManager] Shield absorbed a hit!");
        return true;
    }

    // ── Score Multiplier ──────────────────────────────────────────────────────

    private IEnumerator ScoreMultiplierRoutine()
    {
        IsScoreMultiplierActive = true;
        OnPowerUpActivated?.Invoke(PowerUpType.ScoreMultiplier, scoreMultiplierDuration);
        Debug.Log("[PowerUpManager] Score multiplier activated");

        yield return new WaitForSeconds(scoreMultiplierDuration);

        IsScoreMultiplierActive = false;
        OnPowerUpExpired?.Invoke(PowerUpType.ScoreMultiplier);
        Debug.Log("[PowerUpManager] Score multiplier expired");
    }

    /// <summary>Returns the current score multiplier (1x or 2x).</summary>
    public float GetScoreMultiplier()
    {
        return IsScoreMultiplierActive ? scoreMultiplierValue : 1f;
    }

    // ── Magnet ────────────────────────────────────────────────────────────────

    private IEnumerator MagnetRoutine()
    {
        IsMagnetActive = true;
        OnPowerUpActivated?.Invoke(PowerUpType.Magnet, magnetDuration);
        Debug.Log("[PowerUpManager] Magnet activated");

        yield return new WaitForSeconds(magnetDuration);

        IsMagnetActive = false;
        OnPowerUpExpired?.Invoke(PowerUpType.Magnet);
        Debug.Log("[PowerUpManager] Magnet expired");
    }

    private void PullNearbyCoins()
    {
        // Find all active collectibles within magnet radius and move them toward player
        Collider[] nearby = Physics.OverlapSphere(_playerTransform.position, magnetRadius);
        foreach (var col in nearby)
        {
            Collectible c = col.GetComponent<Collectible>();
            if (c != null && col.gameObject.activeSelf)
            {
                col.transform.position = Vector3.MoveTowards(
                    col.transform.position,
                    _playerTransform.position,
                    15f * Time.deltaTime
                );
            }
        }
    }
}
