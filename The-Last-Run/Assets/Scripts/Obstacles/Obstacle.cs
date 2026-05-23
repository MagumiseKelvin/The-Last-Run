using UnityEngine;

/// <summary>
/// Base class for all obstacles.
/// Detection and game-over logic is handled by PlayerCollision.
/// This class handles visual/audio effects on hit.
/// Tag this GameObject as "Obstacle" for PlayerCollision to detect it.
/// </summary>
public class Obstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public int    prefabIndex  = 0;
    public string obstacleType = "Default";

    [Header("Effects")]
    public GameObject collisionParticlePrefab;

    /// <summary>
    /// Called by PlayerCollision when this obstacle is hit.
    /// Override in subclasses for custom behaviour.
    /// </summary>
    public virtual void OnHit()
    {
        if (collisionParticlePrefab != null)
            Instantiate(collisionParticlePrefab, transform.position, Quaternion.identity);
    }
}
