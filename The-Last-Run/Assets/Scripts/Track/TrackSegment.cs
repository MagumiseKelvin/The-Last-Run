using UnityEngine;

/// <summary>
/// Attached to each track segment prefab.
/// Holds references to obstacle spawn points and the segment's lane layout.
/// The TrackManager moves this object; this script just describes the segment.
/// </summary>
public class TrackSegment : MonoBehaviour
{
    [Header("Spawn Points")]
    [Tooltip("Transform positions where obstacles can be placed on this segment")]
    public Transform[] obstacleSpawnPoints;

    [Tooltip("Transform positions where coins/relics can be placed")]
    public Transform[] collectibleSpawnPoints;

    [Header("Segment Info")]
    [Tooltip("Unique tag to identify segment type (e.g. Straight, Curve, Gap)")]
    public string segmentType = "Straight";

    [Tooltip("If true, this segment will never spawn obstacles (used for opening segments)")]
    public bool isSafeSegment = false;

    /// <summary>
    /// Returns a random obstacle spawn point from this segment.
    /// Returns null if none are defined.
    /// </summary>
    public Transform GetRandomObstacleSpawnPoint()
    {
        if (obstacleSpawnPoints == null || obstacleSpawnPoints.Length == 0)
            return null;

        return obstacleSpawnPoints[Random.Range(0, obstacleSpawnPoints.Length)];
    }

    /// <summary>
    /// Returns a random collectible spawn point from this segment.
    /// Returns null if none are defined.
    /// </summary>
    public Transform GetRandomCollectibleSpawnPoint()
    {
        if (collectibleSpawnPoints == null || collectibleSpawnPoints.Length == 0)
            return null;

        return collectibleSpawnPoints[Random.Range(0, collectibleSpawnPoints.Length)];
    }

    private void OnDrawGizmos()
    {
        // Visualize spawn points in the editor
        if (obstacleSpawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (var point in obstacleSpawnPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.3f);
            }
        }

        if (collectibleSpawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var point in collectibleSpawnPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.2f);
            }
        }
    }
}
