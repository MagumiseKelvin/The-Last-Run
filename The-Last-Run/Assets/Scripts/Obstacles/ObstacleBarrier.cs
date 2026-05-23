using UnityEngine;

/// <summary>
/// A solid barrier obstacle that blocks the full lane.
/// Player must switch lanes to avoid it.
/// Inherits from Obstacle base class.
/// </summary>
public class ObstacleBarrier : Obstacle
{
    [Header("Barrier Settings")]
    [Tooltip("If true, the barrier spans multiple lanes and cannot be avoided by switching")]
    public bool isFullWidth = false;

    private void Awake()
    {
        obstacleType = "Barrier";
    }

    protected override void TriggerCollision(GameObject playerObject)
    {
        Debug.Log("[ObstacleBarrier] Player hit a barrier!");
        base.TriggerCollision(playerObject);
    }
}
