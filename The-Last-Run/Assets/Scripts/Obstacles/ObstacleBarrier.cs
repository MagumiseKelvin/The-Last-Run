using UnityEngine;

/// <summary>
/// Solid barrier — player must switch lanes to avoid.
/// Tag: "Obstacle"
/// </summary>
public class ObstacleBarrier : Obstacle
{
    private void Awake() => obstacleType = "Barrier";

    public override void OnHit()
    {
        Debug.Log("[ObstacleBarrier] Player hit a barrier!");
        base.OnHit();
    }
}
