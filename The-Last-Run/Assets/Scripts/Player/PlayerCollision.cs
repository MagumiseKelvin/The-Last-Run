using UnityEngine;

/// <summary>
/// Detects collisions between the player and obstacles or collectibles.
/// Uses tags ("Obstacle", "Collectible") instead of layers — no layer setup needed.
/// Also falls back to component detection for robustness.
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    private PlayerController _playerController;

    // Keep LayerMask fields so existing Inspector assignments aren't broken,
    // but detection now works via tags + components regardless.
    [Header("Layer Settings (optional — tag detection is primary)")]
    public LayerMask obstacleLayer;
    public LayerMask collectibleLayer;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    // CharacterController solid collisions (non-trigger obstacles)
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!IsGameRunning()) return;
        if (IsObstacle(hit.gameObject))
            HandleObstacleHit(hit.gameObject);
    }

    // Trigger collisions (coins + trigger-based obstacles)
    private void OnTriggerEnter(Collider other)
    {
        if (!IsGameRunning()) return;

        if (IsCollectible(other.gameObject))
        {
            HandleCollectible(other.gameObject);
            return;
        }

        if (IsObstacle(other.gameObject))
            HandleObstacleHit(other.gameObject);
    }

    // ── Detection helpers ─────────────────────────────────────────────────────

    private bool IsObstacle(GameObject go)
    {
        // Tag check (primary)
        if (go.CompareTag("Obstacle")) return true;

        // Component check (fallback — works even without tag)
        if (go.GetComponent<Obstacle>() != null) return true;
        if (go.GetComponentInParent<Obstacle>() != null) return true;

        // Layer mask check (legacy fallback)
        if (obstacleLayer.value != 0 && ((1 << go.layer) & obstacleLayer) != 0)
            return true;

        return false;
    }

    private bool IsCollectible(GameObject go)
    {
        // Tag check (primary)
        if (go.CompareTag("Collectible")) return true;

        // Component check (fallback)
        if (go.GetComponent<Collectible>() != null) return true;
        if (go.GetComponentInParent<Collectible>() != null) return true;

        // Layer mask check (legacy fallback)
        if (collectibleLayer.value != 0 && ((1 << go.layer) & collectibleLayer) != 0)
            return true;

        return false;
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private void HandleObstacleHit(GameObject obstacle)
    {
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.TryAbsorbHit())
        {
            Debug.Log("[PlayerCollision] Shield absorbed hit from: " + obstacle.name);
            return;
        }

        Debug.Log("[PlayerCollision] Hit obstacle: " + obstacle.name);

        // Trigger obstacle visual/audio effects
        Obstacle obs = obstacle.GetComponent<Obstacle>()
                    ?? obstacle.GetComponentInParent<Obstacle>();
        obs?.OnHit();

        AudioManager.Instance?.PlayCollision();
        _playerController?.OnHitObstacle();
    }

    private void HandleCollectible(GameObject collectible)
    {
        // Find the Collectible component — may be on parent
        Collectible c = collectible.GetComponent<Collectible>()
                     ?? collectible.GetComponentInParent<Collectible>();

        if (c == null) return;

        Debug.Log("[PlayerCollision] Collected: " + collectible.name);

        float multiplier = PowerUpManager.Instance != null
            ? PowerUpManager.Instance.GetScoreMultiplier() : 1f;

        ScoreManager.Instance?.AddCoinScore(multiplier);
        AudioManager.Instance?.PlayCoinPickup();
        c.OnCollected();
    }

    private bool IsGameRunning()
    {
        return GameManager.Instance != null && GameManager.Instance.IsGameRunning;
    }
}
