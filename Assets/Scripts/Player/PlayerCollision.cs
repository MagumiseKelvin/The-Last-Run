using UnityEngine;

/// <summary>
/// Detects collisions between the player and obstacles or collectibles.
/// Uses Unity trigger/collision callbacks.
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

        // Check if we hit an obstacle
        if (((1 << hit.gameObject.layer) & obstacleLayer) != 0)
        {
            HandleObstacleHit(hit.gameObject);
        }
    }

    // Called when entering a trigger collider (coins use triggers)
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        // Check collectible
        if (((1 << other.gameObject.layer) & collectibleLayer) != 0)
        {
            HandleCollectible(other.gameObject);
        }

        // Also check obstacle triggers (some obstacles use triggers)
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            HandleObstacleHit(other.gameObject);
        }
    }

    private void HandleObstacleHit(GameObject obstacle)
    {
        Debug.Log($"[PlayerCollision] Hit obstacle: {obstacle.name}");
        _playerController?.OnHitObstacle();
    }

    private void HandleCollectible(GameObject collectible)
    {
        Debug.Log($"[PlayerCollision] Collected: {collectible.name}");

        // Add score
        ScoreManager.Instance?.AddCoinScore();

        // Notify the collectible so it can play effects and deactivate
        collectible.GetComponent<Collectible>()?.OnCollected();
    }
}
