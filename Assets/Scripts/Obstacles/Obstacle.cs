using UnityEngine;

/// <summary>
/// Base class for all obstacles.
/// Handles collision detection with the player and returns itself to the pool on cleanup.
/// </summary>
public class Obstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [Tooltip("Index matching the prefab index in ObstacleSpawner for pooling")]
    public int prefabIndex = 0;

    [Tooltip("Type label for this obstacle (used for effects/audio variation)")]
    public string obstacleType = "Default";

    [Header("Effects")]
    [Tooltip("Optional particle effect to play on collision")]
    public GameObject collisionParticlePrefab;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player hit this obstacle
        PlayerCollision player = other.GetComponent<PlayerCollision>();
        if (player != null)
        {
            TriggerCollision(other.gameObject);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PlayerCollision player = hit.controller.GetComponent<PlayerCollision>();
        if (player != null)
        {
            TriggerCollision(hit.controller.gameObject);
        }
    }

    protected virtual void TriggerCollision(GameObject playerObject)
    {
        // Spawn collision particles if assigned
        if (collisionParticlePrefab != null)
        {
            Instantiate(collisionParticlePrefab, transform.position, Quaternion.identity);
        }

        // Notify the player
        PlayerController controller = playerObject.GetComponent<PlayerController>();
        controller?.OnHitObstacle();
    }
}
