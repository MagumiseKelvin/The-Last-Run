using UnityEngine;

/// <summary>
/// Detects collisions between the player and obstacles or collectibles.
/// Supports shield power-up absorption.
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    private PlayerController _playerController;

    [Header("Layer Settings")]
    [Tooltip("Layer assigned to obstacle objects")]
    public LayerMask obstacleLayer;
    [Tooltip("Layer assigned to collectible objects (coins/relics)")]
    public LayerMask collectibleLayer;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    // Called when CharacterController hits a collider
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        if (((1 << hit.gameObject.layer) & obstacleLayer) != 0)
        {
            HandleObstacleHit(hit.gameObject);
        }
    }

    // Called when entering a trigger collider
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        // Collectible
        if (((1 << other.gameObject.layer) & collectibleLayer) != 0)
        {
            HandleCollectible(other.gameObject);
            return;
        }

        // Obstacle trigger
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            HandleObstacleHit(other.gameObject);
        }
    }

    private void HandleObstacleHit(GameObject obstacle)
    {
        // Check if shield absorbs the hit
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.TryAbsorbHit())
        {
            Debug.Log("[PlayerCollision] Shield absorbed hit from: " + obstacle.name);
            return;
        }

        Debug.Log("[PlayerCollision] Hit obstacle: " + obstacle.name);
        AudioManager.Instance?.PlayCollision();
        _playerController?.OnHitObstacle();
    }

    private void HandleCollectible(GameObject collectible)
    {
        Debug.Log("[PlayerCollision] Collected: " + collectible.name);

        // Score with multiplier
        float multiplier = PowerUpManager.Instance != null
            ? PowerUpManager.Instance.GetScoreMultiplier()
            : 1f;

        ScoreManager.Instance?.AddCoinScore(multiplier);
        AudioManager.Instance?.PlayCoinPickup();

        // Notify the collectible
        collectible.GetComponent<Collectible>()?.OnCollected();
    }
}
